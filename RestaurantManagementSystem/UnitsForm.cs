using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class UnitsForm : Form
    {
        private Panel contentPanel;
        private Label lblName;
        private TextBox txtUnitName;
        private Label lblCode;
        private TextBox txtShortCode;
        private CheckBox chkAllowDecimal;
        private Button btnSave;
        private DataGridView dgvUnits;

        public UnitsForm()
        {
            this.Text = "Manage Units";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(800, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            InitializeLayout();
            LoadUnits();
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

            // ---------- LABEL: Unit Name ----------
            lblName = new Label();
            lblName.Text = "Unit Name:";
            lblName.Location = new Point(x, y);
            lblName.AutoSize = true;
            lblName.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblName);

            // ---------- TEXTBOX: Unit Name ----------
            txtUnitName = new TextBox();
            txtUnitName.Location = new Point(x, y + 30);
            txtUnitName.Width = 300;
            txtUnitName.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(txtUnitName);

            // ---------- LABEL: Short Code ----------
            lblCode = new Label();
            lblCode.Text = "Short Code (e.g. kg):";
            lblCode.Location = new Point(x, y + 80);
            lblCode.AutoSize = true;
            lblCode.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblCode);

            // ---------- TEXTBOX: Short Code ----------
            txtShortCode = new TextBox();
            txtShortCode.Location = new Point(x, y + 110);
            txtShortCode.Width = 150;
            txtShortCode.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(txtShortCode);

            // ---------- CHECKBOX: Allow Decimals ----------
            chkAllowDecimal = new CheckBox();
            chkAllowDecimal.Text = "Allow Decimals?";
            chkAllowDecimal.Location = new Point(x + 180, y + 110);
            chkAllowDecimal.AutoSize = true;
            chkAllowDecimal.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            contentPanel.Controls.Add(chkAllowDecimal);

            // ---------- BUTTON: Save ----------
            btnSave = new Button();
            btnSave.Text = "Save Unit";
            btnSave.Location = new Point(x, y + 160);
            btnSave.Width = 160;
            btnSave.Height = 35;
            btnSave.BackColor = Color.LightCoral;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            contentPanel.Controls.Add(btnSave);

            // ---------- DATAGRIDVIEW: Units ----------
            dgvUnits = new DataGridView();
            dgvUnits.Location = new Point(x, y + 220);
            dgvUnits.Size = new Size(760, 300);
            dgvUnits.ReadOnly = true;
            dgvUnits.AllowUserToAddRows = false;
            dgvUnits.RowHeadersVisible = false;
            dgvUnits.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUnits.Font = new Font("Segoe UI", 11, FontStyle.Regular);
            dgvUnits.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            dgvUnits.ColumnHeadersDefaultCellStyle.BackColor = Color.LightGray;
            dgvUnits.EnableHeadersVisualStyles = false;
            contentPanel.Controls.Add(dgvUnits);

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

        private void LoadUnits()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "SELECT unit_id, unit_name, short_code, allow_decimals FROM Units";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvUnits.DataSource = dt;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtUnitName.Text) || string.IsNullOrWhiteSpace(txtShortCode.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "INSERT INTO Units (unit_name, short_code, allow_decimals) VALUES (@name, @code, @decimal)";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@name", txtUnitName.Text.Trim());
                    cmd.Parameters.AddWithValue("@code", txtShortCode.Text.Trim());
                    cmd.Parameters.AddWithValue("@decimal", chkAllowDecimal.Checked ? 1 : 0);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        MessageBox.Show("Unit Saved Successfully!");
                        txtUnitName.Clear();
                        txtShortCode.Clear();
                        chkAllowDecimal.Checked = false;
                        LoadUnits();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void UnitsForm_Load(object sender, EventArgs e)
        {

        }
    }
}
