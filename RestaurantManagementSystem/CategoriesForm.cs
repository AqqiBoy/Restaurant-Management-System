using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class CategoriesForm : Form
    {
        private Label lblName;
        private TextBox txtName;
        private Button btnSave;
        private DataGridView dgvCategories;

        public CategoriesForm()
        {
            this.Text = "Manage Food Categories";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadCategories();
        }

        private void InitializeLayout()
        {
            lblName = new Label() { Text = "Category Name:", Location = new Point(20, 20), AutoSize = true };
            this.Controls.Add(lblName);

            txtName = new TextBox() { Location = new Point(20, 45), Width = 250 };
            this.Controls.Add(txtName);

            btnSave = new Button() { Text = "Save Category", Location = new Point(20, 80), Width = 120, BackColor = Color.LightYellow };
            btnSave.Click += BtnSave_Click;
            this.Controls.Add(btnSave);

            dgvCategories = new DataGridView();
            dgvCategories.Location = new Point(20, 130);
            dgvCategories.Size = new Size(340, 300);
            dgvCategories.ReadOnly = true;
            dgvCategories.AllowUserToAddRows = false;
            dgvCategories.RowHeadersVisible = false;
            dgvCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvCategories);
        }

        private void LoadCategories()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var adapter = new SQLiteDataAdapter("SELECT * FROM Categories", conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvCategories.DataSource = dt;
                }
            }
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text)) return;

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var cmd = new SQLiteCommand("INSERT INTO Categories (name) VALUES (@name)", conn))
                {
                    cmd.Parameters.AddWithValue("@name", txtName.Text.Trim());
                    try
                    {
                        cmd.ExecuteNonQuery();
                        txtName.Clear();
                        LoadCategories();
                    }
                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
                }
            }
        }
    }
}