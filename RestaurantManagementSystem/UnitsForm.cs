using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing; // Needed for Point(x, y) and Size(width, height)
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class UnitsForm : Form
    {
        // 1. Declare the Controls as class variables so we can access them everywhere
        private Label lblName;
        private TextBox txtUnitName;
        private Label lblCode;
        private TextBox txtShortCode;
        private CheckBox chkAllowDecimal;
        private Button btnSave;
        private DataGridView dgvUnits;

        public UnitsForm()
        {
            // Set up the Form's own properties
            this.Text = "Manage Units";
            this.Size = new Size(400, 500);
            this.StartPosition = FormStartPosition.CenterParent;

            // 2. Call our custom function to build the UI
            InitializeLayout();

            // 3. Load data
            LoadUnits();
        }

        private void InitializeLayout()
        {

            this.Dock = DockStyle.Fill; // Allow it to stretch
            this.BackColor = Color.White; // Ensure background matches

            // --- 1. Label: Unit Name ---
            lblName = new Label();
            lblName.Text = "Unit Name:";
            lblName.Location = new Point(20, 20); // X=20, Y=20
            lblName.AutoSize = true;
            this.Controls.Add(lblName); // Add to the Form

            // --- 2. TextBox: Unit Name ---
            txtUnitName = new TextBox();
            txtUnitName.Location = new Point(20, 45);
            txtUnitName.Width = 200;
            this.Controls.Add(txtUnitName);

            // --- 3. Label: Short Code ---
            lblCode = new Label();
            lblCode.Text = "Short Code (e.g. kg):";
            lblCode.Location = new Point(20, 80);
            lblCode.AutoSize = true;
            this.Controls.Add(lblCode);

            // --- 4. TextBox: Short Code ---
            txtShortCode = new TextBox();
            txtShortCode.Location = new Point(20, 105);
            txtShortCode.Width = 100;
            this.Controls.Add(txtShortCode);

            // --- 5. CheckBox: Allow Decimals ---
            chkAllowDecimal = new CheckBox();
            chkAllowDecimal.Text = "Allow Decimals?";
            chkAllowDecimal.Location = new Point(150, 105);
            chkAllowDecimal.AutoSize = true;
            this.Controls.Add(chkAllowDecimal);

            // --- 6. Button: Save ---
            btnSave = new Button();
            btnSave.Text = "Save Unit";
            btnSave.Location = new Point(20, 150);
            btnSave.Width = 100;
            btnSave.Height = 30;
            btnSave.BackColor = Color.LightBlue;
            btnSave.Click += new EventHandler(btnSave_Click); // Connect the click event
            this.Controls.Add(btnSave);

            // --- 7. DataGridView: The List ---
            dgvUnits = new DataGridView();
            dgvUnits.Location = new Point(20, 200);
            // This tells the grid to stretch right and down as the form grows
            dgvUnits.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

            dgvUnits.ReadOnly = true;
            dgvUnits.AllowUserToAddRows = false;
            dgvUnits.RowHeadersVisible = false;
            dgvUnits.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvUnits);
        }

        // --- DATABASE LOGIC (Same as before) ---

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
                        LoadUnits(); // Refresh grid
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