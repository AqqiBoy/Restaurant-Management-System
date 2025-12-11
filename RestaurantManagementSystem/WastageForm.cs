using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class WastageForm : Form
    {
        // UI Controls
        private Label lblIngredient;
        private ComboBox cmbIngredients;
        private Label lblAction;
        private ComboBox cmbAction; // "Remove (Wastage)" or "Add (Found)"
        private Label lblQty;
        private TextBox txtQty;
        private Label lblUnit;
        private Label lblReason;
        private TextBox txtReason;
        private Button btnSave;
        private DataGridView dgvHistory;

        public WastageForm()
        {
            this.Text = "Stock Adjustment / Wastage";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadIngredients();
            LoadHistory();
        }

        private void InitializeLayout()
        {

            this.Dock = DockStyle.Fill; // Allow it to stretch
            this.BackColor = Color.White; // Ensure background matches

            int x = 20; int y = 20;

            // 1. Select Ingredient
            lblIngredient = new Label() { Text = "Select Ingredient:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblIngredient);

            cmbIngredients = new ComboBox() { Location = new Point(x, y + 25), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbIngredients.SelectedIndexChanged += (s, e) => UpdateUnitLabel();
            this.Controls.Add(cmbIngredients);

            // 2. Action (Add or Remove)
            y += 60;
            lblAction = new Label() { Text = "Action:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblAction);

            cmbAction = new ComboBox() { Location = new Point(x, y + 25), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbAction.Items.Add("Remove (Wastage / Spoilage)");
            cmbAction.Items.Add("Add (Correction / Found)");
            cmbAction.SelectedIndex = 0; // Default to Remove
            this.Controls.Add(cmbAction);

            // 3. Quantity
            y += 60;
            lblQty = new Label() { Text = "Quantity:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblQty);

            txtQty = new TextBox() { Location = new Point(x, y + 25), Width = 100 };
            this.Controls.Add(txtQty);

            lblUnit = new Label() { Text = "...", Location = new Point(x + 110, y + 28), AutoSize = true };
            this.Controls.Add(lblUnit);

            // 4. Reason
            y += 60;
            lblReason = new Label() { Text = "Reason (e.g. 'Fell on floor'):", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblReason);

            txtReason = new TextBox() { Location = new Point(x, y + 25), Width = 250 };
            this.Controls.Add(txtReason);

            // 5. Save Button
            y += 60;
            btnSave = new Button() { Text = "Adjust Stock", Location = new Point(x, y), Width = 150, Height = 40, BackColor = Color.LightCoral };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            // 6. History Grid
            dgvHistory = new DataGridView();
            dgvHistory.Location = new Point(20, y + 60);
            // This tells the grid to stretch right and down as the form grows
            dgvHistory.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

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
                string sql = "SELECT i.ingredient_id, i.name, u.short_code FROM Ingredients i JOIN Units u ON i.unit_id = u.unit_id ORDER BY i.name";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbIngredients.DataSource = dt;
                    cmbIngredients.DisplayMember = "name";
                    cmbIngredients.ValueMember = "ingredient_id";
                }
            }
        }

        private void UpdateUnitLabel()
        {
            if (cmbIngredients.SelectedIndex != -1 && cmbIngredients.SelectedItem is DataRowView row)
            {
                lblUnit.Text = row["short_code"].ToString();
            }
        }

        private void LoadHistory()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                // Show positive/negative quantity and reason
                string sql = @"
                    SELECT 
                        s.adjustment_date, 
                        i.name, 
                        s.quantity_adjusted, 
                        s.reason 
                    FROM StockAdjustments s
                    JOIN Ingredients i ON s.ingredient_id = i.ingredient_id
                    ORDER BY s.adjustment_date DESC";

                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvHistory.DataSource = dt;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (cmbIngredients.SelectedIndex == -1) return;
            if (!double.TryParse(txtQty.Text, out double inputQty) || inputQty <= 0)
            {
                MessageBox.Show("Please enter a valid positive number."); return;
            }
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("Please enter a reason."); return;
            }

            int ingId = Convert.ToInt32(cmbIngredients.SelectedValue);

            // Determine if we are Adding (+) or Removing (-)
            // Index 0 = Remove, Index 1 = Add
            double finalChange = (cmbAction.SelectedIndex == 0) ? -inputQty : inputQty;

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Update Ingredient Stock (Using ROUND to prevent 7.300000007 errors)
                        string sqlUpdate = "UPDATE Ingredients SET current_stock = ROUND(current_stock + @change, 3) WHERE ingredient_id = @id";
                        using (var cmd = new SQLiteCommand(sqlUpdate, conn))
                        {
                            cmd.Parameters.AddWithValue("@change", finalChange);
                            cmd.Parameters.AddWithValue("@id", ingId);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Log in StockAdjustments
                        string sqlLog = "INSERT INTO StockAdjustments (ingredient_id, quantity_adjusted, reason) VALUES (@id, @qty, @reason)";
                        using (var cmd = new SQLiteCommand(sqlLog, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", ingId);
                            cmd.Parameters.AddWithValue("@qty", finalChange);
                            cmd.Parameters.AddWithValue("@reason", txtReason.Text.Trim());
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Stock Adjusted Successfully.");
                        txtQty.Clear();
                        txtReason.Clear();
                        LoadHistory();
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