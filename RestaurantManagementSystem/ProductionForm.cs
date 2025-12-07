using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class ProductionForm : Form
    {
        // UI Controls
        private GroupBox grpInput;
        private Label lblSource;
        private ComboBox cmbSource; // "Mutton Mince"
        private Label lblUsedQty;
        private TextBox txtUsedQty;
        private Label lblSourceUnit;

        private GroupBox grpOutput;
        private Label lblTarget;
        private ComboBox cmbTarget; // "Frozen Kebab Patty"
        private Label lblProducedQty;
        private TextBox txtProducedQty;
        private Label lblTargetUnit;

        private Button btnConvert;
        private DataGridView dgvHistory;

        public ProductionForm()
        {
            this.Text = "Kitchen Production (Pre-Cooking)";
            this.Size = new Size(600, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadIngredients();
            LoadProductionHistory();
        }

        private void InitializeLayout()
        {
            int x = 20; int y = 20;

            // --- SECTION 1: INPUT (Raw Material) ---
            grpInput = new GroupBox() { Text = "Step 1: Raw Material Used (Stock OUT)", Location = new Point(x, y), Size = new Size(540, 100) };
            this.Controls.Add(grpInput);

            lblSource = new Label() { Text = "Ingredient:", Location = new Point(20, 30), AutoSize = true };
            grpInput.Controls.Add(lblSource);

            cmbSource = new ComboBox() { Location = new Point(20, 55), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbSource.SelectedIndexChanged += (s, e) => UpdateUnitLabel(cmbSource, lblSourceUnit);
            grpInput.Controls.Add(cmbSource);

            lblUsedQty = new Label() { Text = "Qty Used:", Location = new Point(240, 30), AutoSize = true };
            grpInput.Controls.Add(lblUsedQty);

            txtUsedQty = new TextBox() { Location = new Point(240, 55), Width = 100 };
            grpInput.Controls.Add(txtUsedQty);

            lblSourceUnit = new Label() { Text = "...", Location = new Point(350, 58), AutoSize = true };
            grpInput.Controls.Add(lblSourceUnit);

            // --- SECTION 2: OUTPUT (Finished Product) ---
            y += 120;
            grpOutput = new GroupBox() { Text = "Step 2: Produced Item (Stock IN)", Location = new Point(x, y), Size = new Size(540, 100) };
            this.Controls.Add(grpOutput);

            lblTarget = new Label() { Text = "Item Made:", Location = new Point(20, 30), AutoSize = true };
            grpOutput.Controls.Add(lblTarget);

            cmbTarget = new ComboBox() { Location = new Point(20, 55), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbTarget.SelectedIndexChanged += (s, e) => UpdateUnitLabel(cmbTarget, lblTargetUnit);
            grpOutput.Controls.Add(cmbTarget);

            lblProducedQty = new Label() { Text = "Qty Made:", Location = new Point(240, 30), AutoSize = true };
            grpOutput.Controls.Add(lblProducedQty);

            txtProducedQty = new TextBox() { Location = new Point(240, 55), Width = 100 };
            grpOutput.Controls.Add(txtProducedQty);

            lblTargetUnit = new Label() { Text = "...", Location = new Point(350, 58), AutoSize = true };
            grpOutput.Controls.Add(lblTargetUnit);

            // --- BUTTON ---
            y += 120;
            btnConvert = new Button() { Text = "Record Production", Location = new Point(x, y), Width = 200, Height = 40, BackColor = Color.LightGoldenrodYellow };
            btnConvert.Click += BtnConvert_Click;
            this.Controls.Add(btnConvert);

            // --- HISTORY GRID ---
            dgvHistory = new DataGridView();
            dgvHistory.Location = new Point(20, y + 60);
            dgvHistory.Size = new Size(540, 250);
            dgvHistory.ReadOnly = true;
            dgvHistory.AllowUserToAddRows = false;
            dgvHistory.RowHeadersVisible = false;
            dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvHistory);
        }

        // --- DATABASE LOGIC ---

        private void LoadIngredients()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                // Get Name, ID, and Unit Code
                string sql = "SELECT i.ingredient_id, i.name, u.short_code FROM Ingredients i JOIN Units u ON i.unit_id = u.unit_id ORDER BY i.name";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    // We need two separate copies of the data for two dropdowns
                    cmbSource.DataSource = dt.Copy();
                    cmbSource.DisplayMember = "name";
                    cmbSource.ValueMember = "ingredient_id";

                    cmbTarget.DataSource = dt.Copy();
                    cmbTarget.DisplayMember = "name";
                    cmbTarget.ValueMember = "ingredient_id";
                }
            }
        }

        // Helper to show "kg" or "pcs" when user picks an item
        private void UpdateUnitLabel(ComboBox cmb, Label lbl)
        {
            if (cmb.SelectedIndex != -1 && cmb.SelectedItem is DataRowView row)
            {
                lbl.Text = row["short_code"].ToString();
            }
        }

        private void LoadProductionHistory()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        p.production_date,
                        src.name || ' (' || p.used_quantity || ')' as Input,
                        tgt.name || ' (' || p.produced_quantity || ')' as Output
                    FROM ProductionLogs p
                    JOIN Ingredients src ON p.source_ingredient_id = src.ingredient_id
                    JOIN Ingredients tgt ON p.target_ingredient_id = tgt.ingredient_id
                    ORDER BY p.production_id DESC";

                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvHistory.DataSource = dt;
                }
            }
        }

        private void BtnConvert_Click(object sender, EventArgs e)
        {
            if (cmbSource.SelectedIndex == -1 || cmbTarget.SelectedIndex == -1) return;

            if (!double.TryParse(txtUsedQty.Text, out double usedQty) ||
                !double.TryParse(txtProducedQty.Text, out double producedQty))
            {
                MessageBox.Show("Invalid Quantities."); return;
            }

            int sourceId = Convert.ToInt32(cmbSource.SelectedValue);
            int targetId = Convert.ToInt32(cmbTarget.SelectedValue);

            if (sourceId == targetId) { MessageBox.Show("Source and Target cannot be the same!"); return; }

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Check if we have enough Source Stock
                        // (You can skip this if you want to allow negative stock, but it's safer to check)

                        // 2. Subtract from Source (Mince) - ROUNDED to 3 decimal places
                        string sqlDed = "UPDATE Ingredients SET current_stock = ROUND(current_stock - @used, 1) WHERE ingredient_id = @id";
                        using (var cmd = new SQLiteCommand(sqlDed, conn))
                        {
                            cmd.Parameters.AddWithValue("@used", usedQty);
                            cmd.Parameters.AddWithValue("@id", sourceId);
                            cmd.ExecuteNonQuery();
                        }

                        // 3. Add to Target (Frozen Patties) - ROUNDED to 3 decimal places
                        string sqlAdd = "UPDATE Ingredients SET current_stock = ROUND(current_stock + @made, 1) WHERE ingredient_id = @id";
                        using (var cmd = new SQLiteCommand(sqlAdd, conn))
                        {
                            cmd.Parameters.AddWithValue("@made", producedQty);
                            cmd.Parameters.AddWithValue("@id", targetId);
                            cmd.ExecuteNonQuery();
                        }

                        // 4. Log the record
                        string sqlLog = "INSERT INTO ProductionLogs (source_ingredient_id, used_quantity, target_ingredient_id, produced_quantity) VALUES (@sid, @uqty, @tid, @pqty)";
                        using (var cmd = new SQLiteCommand(sqlLog, conn))
                        {
                            cmd.Parameters.AddWithValue("@sid", sourceId);
                            cmd.Parameters.AddWithValue("@uqty", usedQty);
                            cmd.Parameters.AddWithValue("@tid", targetId);
                            cmd.Parameters.AddWithValue("@pqty", producedQty);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Production Recorded Successfully!");
                        txtUsedQty.Clear();
                        txtProducedQty.Clear();
                        LoadProductionHistory();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }
    }
}