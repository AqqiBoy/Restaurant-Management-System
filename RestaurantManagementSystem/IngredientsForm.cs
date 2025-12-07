using System;
using System.Data;
using System.Data.SQLite; // Creating the connection
using System.Drawing;     // For UI design (Point, Size, Color)
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class IngredientsForm : Form
    {
        // Define UI Controls
        private Label lblName;
        private TextBox txtName;
        private Label lblUnit;
        private ComboBox cmbUnits; // <--- The Dropdown List
        private Label lblStock;
        private TextBox txtStock;
        private Button btnSave;
        private DataGridView dgvIngredients;

        public IngredientsForm()
        {
            // Form Settings
            this.Text = "Manage Ingredients";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            // Build the UI
            InitializeLayout();

            // Load Data
            LoadUnitsIntoDropdown();
            LoadIngredients();
        }

        private void InitializeLayout()
        {
            int x = 20;
            int y = 20;

            // 1. Ingredient Name
            lblName = new Label() { Text = "Ingredient Name:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblName);

            txtName = new TextBox() { Location = new Point(x, y + 25), Width = 250 };
            this.Controls.Add(txtName);

            // 2. Unit Dropdown
            y += 60;
            lblUnit = new Label() { Text = "Select Unit:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblUnit);

            cmbUnits = new ComboBox() { Location = new Point(x, y + 25), Width = 150 };
            cmbUnits.DropDownStyle = ComboBoxStyle.DropDownList; // User must pick from list
            this.Controls.Add(cmbUnits);

            // 3. Initial Stock
            y += 60;
            lblStock = new Label() { Text = "Initial Stock:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblStock);

            txtStock = new TextBox() { Location = new Point(x, y + 25), Width = 100, Text = "0" };
            this.Controls.Add(txtStock);

            // 4. Save Button
            y += 60;
            btnSave = new Button() { Text = "Save Ingredient", Location = new Point(x, y), Width = 150, Height = 35, BackColor = Color.LightGreen };
            btnSave.Click += new EventHandler(btnSave_Click);
            this.Controls.Add(btnSave);

            // 5. Data Grid (The List)
            dgvIngredients = new DataGridView();
            dgvIngredients.Location = new Point(20, y + 50);
            dgvIngredients.Size = new Size(440, 250);
            dgvIngredients.ReadOnly = true;
            dgvIngredients.AllowUserToAddRows = false;
            dgvIngredients.RowHeadersVisible = false;
            dgvIngredients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvIngredients);
        }

        // --- DATABASE LOGIC ---

        // 1. Fill the Dropdown with Units (Kg, Pcs, etc.)
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

                    // Bind Data: User sees 'unit_name', System uses 'unit_id'
                    cmbUnits.DataSource = dt;
                    cmbUnits.DisplayMember = "unit_name";
                    cmbUnits.ValueMember = "unit_id";
                }
            }
        }

        // 2. Load the Ingredients List for the Grid
        private void LoadIngredients()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                // JOIN query to get the Unit Name instead of just the ID number
                string sql = @"
                    SELECT 
                        i.ingredient_id, 
                        i.name, 
                        u.unit_name as unit, 
                        i.current_stock 
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

        // 3. Save Button Logic
        private void btnSave_Click(object sender, EventArgs e)
        {
            // Check if name is empty or no unit selected
            if (string.IsNullOrWhiteSpace(txtName.Text) || cmbUnits.SelectedIndex == -1)
            {
                MessageBox.Show("Please enter a name and select a unit.");
                return;
            }

            // Check if stock is a valid number
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

                        // Reset fields
                        txtName.Clear();
                        txtStock.Text = "0";
                        LoadIngredients(); // Refresh Grid
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }
    }
}