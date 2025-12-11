using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class MenuForm : Form
    {
        private Label lblName;
        private TextBox txtName;
        private Label lblPrice;
        private TextBox txtPrice;
        private Label lblCategory;
        private ComboBox cmbCategory; // Now loaded from DB
        private Button btnSave;
        private DataGridView dgvMenu;

        public MenuForm()
        {
            this.Text = "Manage Menu Items";
            this.Size = new Size(500, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadCategoriesIntoDropdown(); // <--- This is new
            LoadMenu();
        }

        private void InitializeLayout()
        {

            this.Dock = DockStyle.Fill; // Allow it to stretch
            this.BackColor = Color.White; // Ensure background matches

            int x = 20; int y = 20;

            lblName = new Label() { Text = "Item Name:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblName);
            txtName = new TextBox() { Location = new Point(x, y + 25), Width = 250 };
            this.Controls.Add(txtName);

            y += 60;
            lblPrice = new Label() { Text = "Selling Price:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblPrice);
            txtPrice = new TextBox() { Location = new Point(x, y + 25), Width = 100 };
            this.Controls.Add(txtPrice);

            y += 60;
            lblCategory = new Label() { Text = "Category:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblCategory);

            cmbCategory = new ComboBox() { Location = new Point(x, y + 25), Width = 200 };
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbCategory);

            y += 60;
            btnSave = new Button() { Text = "Add to Menu", Location = new Point(x, y), Width = 150, Height = 40, BackColor = Color.LightSalmon };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            dgvMenu = new DataGridView();
            dgvMenu.Location = new Point(20, y + 60);
            // This tells the grid to stretch right and down as the form grows
            dgvMenu.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            dgvMenu.ReadOnly = true;
            dgvMenu.AllowUserToAddRows = false;
            dgvMenu.RowHeadersVisible = false;
            dgvMenu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvMenu);
        }

        // NEW FUNCTION: Fetch categories from DB
        private void LoadCategoriesIntoDropdown()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var adapter = new SQLiteDataAdapter("SELECT category_id, name FROM Categories", conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbCategory.DataSource = dt;
                    cmbCategory.DisplayMember = "name";
                    cmbCategory.ValueMember = "category_id";
                }
            }
        }

        private void LoadMenu()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                // Join with Categories table to show the name instead of ID
                string sql = @"
                    SELECT m.name, m.price, c.name as Category 
                    FROM MenuItems m 
                    JOIN Categories c ON m.category_id = c.category_id";

                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvMenu.DataSource = dt;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cmbCategory.SelectedIndex == -1)
            {
                MessageBox.Show("Missing Fields"); return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price)) return;

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "INSERT INTO MenuItems (name, price, category_id) VALUES (@name, @price, @catId)";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                    cmd.Parameters.AddWithValue("@price", price);
                    cmd.Parameters.AddWithValue("@catId", cmbCategory.SelectedValue);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Saved!");
                        txtName.Clear();
                        LoadMenu();
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                }
            }
        }
    }
}