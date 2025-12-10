using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class MenuForm : Form
    {
        private Panel contentPanel;
        private Label lblName;
        private TextBox txtName;
        private Label lblPrice;
        private TextBox txtPrice;
        private Label lblCategory;
        private ComboBox cmbCategory;
        private Button btnSave;
        private DataGridView dgvMenu;

        public MenuForm()
        {
            this.Text = "Manage Menu Items";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadCategoriesIntoDropdown();
            LoadMenu();
        }

        private void InitializeLayout()
        {
            // ---------- MAIN CONTENT PANEL ----------
            contentPanel = new Panel();
            contentPanel.Size = new Size(800, 550);
            contentPanel.BackColor = Color.WhiteSmoke;
            this.Controls.Add(contentPanel);

            int x = 20, y = 20;

            // ---------- LABEL: Item Name ----------
            lblName = new Label();
            lblName.Text = "Item Name:";
            lblName.Location = new Point(x, y);
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblName);

            // ---------- TEXTBOX: Item Name ----------
            txtName = new TextBox();
            txtName.Location = new Point(x, y + 30);
            txtName.Width = 300;
            txtName.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(txtName);

            // ---------- LABEL: Selling Price ----------
            y += 70;
            lblPrice = new Label();
            lblPrice.Text = "Selling Price:";
            lblPrice.Location = new Point(x, y);
            lblPrice.AutoSize = true;
            lblPrice.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblPrice);

            // ---------- TEXTBOX: Selling Price ----------
            txtPrice = new TextBox();
            txtPrice.Location = new Point(x, y + 30);
            txtPrice.Width = 150;
            txtPrice.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(txtPrice);

            // ---------- LABEL: Category ----------
            y += 70;
            lblCategory = new Label();
            lblCategory.Text = "Category:";
            lblCategory.Location = new Point(x, y);
            lblCategory.AutoSize = true;
            lblCategory.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblCategory);

            // ---------- COMBOBOX: Category ----------
            cmbCategory = new ComboBox();
            cmbCategory.Location = new Point(x, y + 30);
            cmbCategory.Width = 250;
            cmbCategory.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            cmbCategory.DropDownStyle = ComboBoxStyle.DropDownList;
            contentPanel.Controls.Add(cmbCategory);

            // ---------- BUTTON: Save ----------
            y += 70;
            btnSave = new Button();
            btnSave.Text = "Add to Menu";
            btnSave.Location = new Point(x, y);
            btnSave.Width = 160;
            btnSave.Height = 40;
            btnSave.BackColor = Color.LightCoral;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            contentPanel.Controls.Add(btnSave);

            // ---------- DATAGRIDVIEW: Menu ----------
            dgvMenu = new DataGridView();
            dgvMenu.Location = new Point(x, y + 60);
            dgvMenu.Size = new Size(760, 250);
            dgvMenu.ReadOnly = true;
            dgvMenu.AllowUserToAddRows = false;
            dgvMenu.RowHeadersVisible = false;
            dgvMenu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMenu.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            dgvMenu.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvMenu.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dgvMenu.EnableHeadersVisualStyles = false;
            contentPanel.Controls.Add(dgvMenu);

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

        // ---------- DATABASE LOGIC (unchanged) ----------
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
                MessageBox.Show("Please fill all fields.");
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price))
            {
                MessageBox.Show("Invalid price.");
                return;
            }

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
                        MessageBox.Show("Menu item added successfully!");
                        txtName.Clear();
                        txtPrice.Clear();
                        LoadMenu();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void MenuForm_Load(object sender, EventArgs e)
        {

        }
    }
}
