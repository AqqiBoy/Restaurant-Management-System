using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class WastageForm : Form
    {
        private Panel contentPanel;

        private Label lblIngredient;
        private ComboBox cmbIngredients;

        private Label lblAction;
        private ComboBox cmbAction;

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
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadIngredients();
            LoadHistory();
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

            // ---------- LABEL: Select Ingredient ----------
            lblIngredient = new Label
            {
                Text = "Select Ingredient:",
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            contentPanel.Controls.Add(lblIngredient);

            // ---------- COMBOBOX: Ingredients ----------
            cmbIngredients = new ComboBox
            {
                Location = new Point(x, y + 30),
                Width = 300,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            cmbIngredients.SelectedIndexChanged += (s, e) => UpdateUnitLabel();
            contentPanel.Controls.Add(cmbIngredients);

            // ---------- LABEL: Action ----------
            y += 80;
            lblAction = new Label
            {
                Text = "Action:",
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            contentPanel.Controls.Add(lblAction);

            // ---------- COMBOBOX: Action ----------
            cmbAction = new ComboBox
            {
                Location = new Point(x, y + 30),
                Width = 250,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            cmbAction.Items.Add("Remove (Wastage / Spoilage)");
            cmbAction.Items.Add("Add (Correction / Found)");
            cmbAction.SelectedIndex = 0;
            contentPanel.Controls.Add(cmbAction);

            // ---------- LABEL: Quantity ----------
            y += 80;
            lblQty = new Label
            {
                Text = "Quantity:",
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            contentPanel.Controls.Add(lblQty);

            // ---------- TEXTBOX: Quantity ----------
            txtQty = new TextBox
            {
                Location = new Point(x, y + 30),
                Width = 100,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            contentPanel.Controls.Add(txtQty);

            // ---------- LABEL: Unit ----------
            lblUnit = new Label
            {
                Text = "...",
                Location = new Point(x + 110, y + 33),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Italic)
            };
            contentPanel.Controls.Add(lblUnit);

            // ---------- LABEL: Reason ----------
            y += 80;
            lblReason = new Label
            {
                Text = "Reason (e.g. 'Fell on floor'):",
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            contentPanel.Controls.Add(lblReason);

            // ---------- TEXTBOX: Reason ----------
            txtReason = new TextBox
            {
                Location = new Point(x, y + 30),
                Width = 300,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            contentPanel.Controls.Add(txtReason);

            // ---------- BUTTON: Adjust Stock ----------
            y += 80;
            btnSave = new Button
            {
                Text = "Adjust Stock",
                Location = new Point(x, y),
                Width = 160,
                Height = 35,
                BackColor = Color.LightCoral,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            contentPanel.Controls.Add(btnSave);

            // ---------- DATAGRIDVIEW: History ----------
            dgvHistory = new DataGridView
            {
                Location = new Point(x, y + 60),
                Size = new Size(760, 300), // same width and height logic as UnitsForm
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
            double finalChange = (cmbAction.SelectedIndex == 0) ? -inputQty : inputQty;

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlUpdate = "UPDATE Ingredients SET current_stock = ROUND(current_stock + @change, 3) WHERE ingredient_id = @id";
                        using (var cmd = new SQLiteCommand(sqlUpdate, conn))
                        {
                            cmd.Parameters.AddWithValue("@change", finalChange);
                            cmd.Parameters.AddWithValue("@id", ingId);
                            cmd.ExecuteNonQuery();
                        }

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

        private void WastageForm_Load(object sender, EventArgs e)
        {

        }
    }
}
