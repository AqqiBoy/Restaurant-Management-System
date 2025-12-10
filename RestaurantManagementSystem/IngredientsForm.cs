using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class IngredientsForm : Form
    {
        private Panel contentPanel;
        private Label lblName;
        private TextBox txtName;
        private Label lblUnit;
        private ComboBox cmbUnits;
        private Label lblStock;
        private TextBox txtStock;
        private Button btnSave;
        private DataGridView dgvIngredients;

        public IngredientsForm()
        {
            this.Text = "Manage Ingredients";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadUnitsIntoDropdown();
            LoadIngredients();
        }

        private void InitializeLayout()
        {
            // ---------- MAIN CONTENT PANEL ----------
            contentPanel = new Panel();
            contentPanel.Size = new Size(800, 550);
            contentPanel.BackColor = Color.WhiteSmoke;
            contentPanel.Anchor = AnchorStyles.None;
            this.Controls.Add(contentPanel);

            int x = 20;
            int y = 20;

            // ---------- LABEL: Ingredient Name ----------
            lblName = new Label();
            lblName.Text = "Ingredient Name:";
            lblName.Location = new Point(x, y);
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblName);

            // ---------- TEXTBOX: Ingredient Name ----------
            txtName = new TextBox();
            txtName.Location = new Point(x, y + 30);
            txtName.Width = 300;
            txtName.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(txtName);

            // ---------- LABEL: Unit ----------
            lblUnit = new Label();
            lblUnit.Text = "Select Unit:";
            lblUnit.Location = new Point(x, y + 80);
            lblUnit.AutoSize = true;
            lblUnit.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblUnit);

            // ---------- COMBOBOX: Units ----------
            cmbUnits = new ComboBox();
            cmbUnits.Location = new Point(x, y + 110);
            cmbUnits.Width = 150;
            cmbUnits.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            cmbUnits.DropDownStyle = ComboBoxStyle.DropDownList;
            contentPanel.Controls.Add(cmbUnits);

            // ---------- LABEL: Stock ----------
            lblStock = new Label();
            lblStock.Text = "Initial Stock:";
            lblStock.Location = new Point(x + 200, y + 80);
            lblStock.AutoSize = true;
            lblStock.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblStock);

            // ---------- TEXTBOX: Stock ----------
            txtStock = new TextBox();
            txtStock.Location = new Point(x + 200, y + 110);
            txtStock.Width = 100;
            txtStock.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            txtStock.Text = "0";
            contentPanel.Controls.Add(txtStock);

            // ---------- BUTTON: Save ----------
            btnSave = new Button();
            btnSave.Text = "Save Ingredient";
            btnSave.Location = new Point(x, y + 160);
            btnSave.Width = 160;
            btnSave.Height = 35;
            btnSave.BackColor = Color.LightCoral;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            contentPanel.Controls.Add(btnSave);

            // ---------- DATAGRIDVIEW: Ingredients ----------
            dgvIngredients = new DataGridView();
            dgvIngredients.Location = new Point(x, y + 220);
            dgvIngredients.Size = new Size(760, 300);
            dgvIngredients.ReadOnly = true;
            dgvIngredients.AllowUserToAddRows = false;
            dgvIngredients.RowHeadersVisible = false;
            dgvIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvIngredients.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            dgvIngredients.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvIngredients.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dgvIngredients.EnableHeadersVisualStyles = false;
            contentPanel.Controls.Add(dgvIngredients);

            // ---------- CENTER CONTENT PANEL ON RESIZE ----------
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

        private void LoadUnitsIntoDropdown()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "SELECT unit_id, unit_name FROM Units";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbUnits.DataSource = dt;
                    cmbUnits.DisplayMember = "unit_name";
                    cmbUnits.ValueMember = "unit_id";
                }
            }
        }

        private void LoadIngredients()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT i.ingredient_id, i.name, u.unit_name as unit, i.current_stock 
                    FROM Ingredients i
                    JOIN Units u ON i.unit_id = u.unit_id";

                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvIngredients.DataSource = dt;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cmbUnits.SelectedIndex == -1)
            {
                MessageBox.Show("Please enter a name and select a unit.");
                return;
            }

            if (!double.TryParse(txtStock.Text, out double stockVal))
            {
                MessageBox.Show("Stock must be a valid number.");
                return;
            }

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "INSERT INTO Ingredients (name, unit_id, current_stock) VALUES (@name, @unitId, @stock)";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@unitId", cmbUnits.SelectedValue);
                    cmd.Parameters.AddWithValue("@stock", stockVal);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Ingredient Added Successfully!");
                        txtName.Clear();
                        txtStock.Text = "0";
                        LoadIngredients();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void IngredientsForm_Load(object sender, EventArgs e)
        {

        }
    }
}
