using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class CategoriesForm : Form
    {
        private Panel contentPanel;
        private Label lblName;
        private TextBox txtName;
        private Button btnSave;
        private DataGridView dgvCategories;

        public CategoriesForm()
        {
            this.Text = "Manage Food Categories";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadCategories();
        }

        private void InitializeLayout()
        {
            // ---------- MAIN CONTENT PANEL ----------
            contentPanel = new Panel();
            contentPanel.Size = new Size(800, 550); // Match UnitsForm
            contentPanel.BackColor = Color.WhiteSmoke;
            contentPanel.Anchor = AnchorStyles.None;
            this.Controls.Add(contentPanel);

            int x = 20;
            int y = 20;

            // ---------- LABEL: Category Name ----------
            lblName = new Label();
            lblName.Text = "Category Name:";
            lblName.Location = new Point(x, y);
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblName);

            // ---------- TEXTBOX: Category Name ----------
            txtName = new TextBox();
            txtName.Location = new Point(x, y + 30);
            txtName.Width = 300;
            txtName.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(txtName);

            // ---------- BUTTON: Save ----------
            btnSave = new Button();
            btnSave.Text = "Save Category";
            btnSave.Location = new Point(x, y + 80);
            btnSave.Width = 160;
            btnSave.Height = 35;
            btnSave.BackColor = Color.LightCoral;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += BtnSave_Click;
            contentPanel.Controls.Add(btnSave);

            // ---------- DATAGRIDVIEW: Categories ----------
            dgvCategories = new DataGridView();
            dgvCategories.Location = new Point(x, y + 140);
            dgvCategories.Size = new Size(760, 350); // Match UnitsForm size
            dgvCategories.ReadOnly = true;
            dgvCategories.AllowUserToAddRows = false;
            dgvCategories.RowHeadersVisible = false;
            dgvCategories.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvCategories.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            dgvCategories.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvCategories.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dgvCategories.EnableHeadersVisualStyles = false;
            contentPanel.Controls.Add(dgvCategories);

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

        // ---------- DATABASE LOGIC (UNCHANGED) ----------
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

        private void CategoriesForm_Load(object sender, EventArgs e)
        {

        }
    }
}
