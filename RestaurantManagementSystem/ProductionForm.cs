using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class ProductionForm : Form
    {
        private Panel contentPanel;
        private GroupBox grpInput, grpOutput;
        private Label lblSource, lblUsedQty, lblSourceUnit;
        private ComboBox cmbSource;
        private TextBox txtUsedQty;
        private Label lblTarget, lblProducedQty, lblTargetUnit;
        private ComboBox cmbTarget;
        private TextBox txtProducedQty;
        private Button btnConvert;
        private DataGridView dgvHistory;

        public ProductionForm()
        {
            this.Text = "Kitchen Production (Pre-Cooking)";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(800, 700);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadIngredients();
            LoadProductionHistory();
        }

        private void ProductionForm_Load(object sender, EventArgs e)
        {

        }

        private void InitializeLayout()
        {
            // ---------- MAIN CONTENT PANEL ----------
            contentPanel = new Panel();
            contentPanel.Size = new Size(800, 550); // same as UnitsForm
            contentPanel.BackColor = Color.WhiteSmoke;
            contentPanel.Anchor = AnchorStyles.None;
            this.Controls.Add(contentPanel);

            int x = 20;
            int y = 20;

            // ---------- GROUPBOX: Raw Material Used ----------
            grpInput = new GroupBox
            {
                Text = "Step 1: Raw Material Used (Stock OUT)",
                Location = new Point(x, y),
                Size = new Size(760, 100),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            contentPanel.Controls.Add(grpInput);

            lblSource = new Label
            {
                Text = "Ingredient:",
                Location = new Point(20, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpInput.Controls.Add(lblSource);

            cmbSource = new ComboBox
            {
                Location = new Point(20, 55),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            cmbSource.SelectedIndexChanged += (s, e) => UpdateUnitLabel(cmbSource, lblSourceUnit);
            grpInput.Controls.Add(cmbSource);

            lblUsedQty = new Label
            {
                Text = "Qty Used:",
                Location = new Point(340, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpInput.Controls.Add(lblUsedQty);

            txtUsedQty = new TextBox
            {
                Location = new Point(340, 55),
                Width = 100,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpInput.Controls.Add(txtUsedQty);

            lblSourceUnit = new Label
            {
                Text = "...",
                Location = new Point(450, 58),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Italic)
            };
            grpInput.Controls.Add(lblSourceUnit);

            // ---------- GROUPBOX: Produced Item ----------
            y += 120;
            grpOutput = new GroupBox
            {
                Text = "Step 2: Produced Item (Stock IN)",
                Location = new Point(x, y),
                Size = new Size(760, 100),
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            contentPanel.Controls.Add(grpOutput);

            lblTarget = new Label
            {
                Text = "Item Made:",
                Location = new Point(20, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpOutput.Controls.Add(lblTarget);

            cmbTarget = new ComboBox
            {
                Location = new Point(20, 55),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            cmbTarget.SelectedIndexChanged += (s, e) => UpdateUnitLabel(cmbTarget, lblTargetUnit);
            grpOutput.Controls.Add(cmbTarget);

            lblProducedQty = new Label
            {
                Text = "Qty Made:",
                Location = new Point(340, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpOutput.Controls.Add(lblProducedQty);

            txtProducedQty = new TextBox
            {
                Location = new Point(340, 55),
                Width = 100,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpOutput.Controls.Add(txtProducedQty);

            lblTargetUnit = new Label
            {
                Text = "...",
                Location = new Point(450, 58),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Italic)
            };
            grpOutput.Controls.Add(lblTargetUnit);

            // ---------- BUTTON: Record Production ----------
            y += 120;
            btnConvert = new Button
            {
                Text = "Record Production",
                Location = new Point(x, y),
                Width = 160,
                Height = 35,
                BackColor = Color.LightCoral,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnConvert.FlatAppearance.BorderSize = 0;
            btnConvert.Click += BtnConvert_Click;
            contentPanel.Controls.Add(btnConvert);

            // ---------- DATAGRIDVIEW: History ----------
            dgvHistory = new DataGridView
            {
                Location = new Point(x, y + 60),
                Size = new Size(760, 300), // same as UnitsForm
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 11, FontStyle.Regular)
            };
            dgvHistory.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvHistory.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dgvHistory.EnableHeadersVisualStyles = false;
            contentPanel.Controls.Add(dgvHistory);

            // ---------- CENTER PANEL ON RESIZE ----------
            this.Resize += (s, e) =>
            {
                contentPanel.Location = new Point(
                    (this.Width - contentPanel.Width) / 2,
                    (this.Height - contentPanel.Height) / 2
                );
            };

            // Initial center
            contentPanel.Location = new Point(
                (this.Width - contentPanel.Width) / 2,
                (this.Height - contentPanel.Height) / 2
            );
        }


        // ---------- DATABASE LOGIC ----------
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

                    cmbSource.DataSource = dt.Copy();
                    cmbSource.DisplayMember = "name";
                    cmbSource.ValueMember = "ingredient_id";

                    cmbTarget.DataSource = dt.Copy();
                    cmbTarget.DisplayMember = "name";
                    cmbTarget.ValueMember = "ingredient_id";
                }
            }
        }

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
                        string sqlDed = "UPDATE Ingredients SET current_stock = ROUND(current_stock - @used, 1) WHERE ingredient_id = @id";
                        using (var cmd = new SQLiteCommand(sqlDed, conn))
                        {
                            cmd.Parameters.AddWithValue("@used", usedQty);
                            cmd.Parameters.AddWithValue("@id", sourceId);
                            cmd.ExecuteNonQuery();
                        }

                        string sqlAdd = "UPDATE Ingredients SET current_stock = ROUND(current_stock + @made, 1) WHERE ingredient_id = @id";
                        using (var cmd = new SQLiteCommand(sqlAdd, conn))
                        {
                            cmd.Parameters.AddWithValue("@made", producedQty);
                            cmd.Parameters.AddWithValue("@id", targetId);
                            cmd.ExecuteNonQuery();
                        }

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
