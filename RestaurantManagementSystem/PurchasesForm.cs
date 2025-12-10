using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class PurchasesForm : Form
    {
        private Panel contentPanel;
        private Label lblIngredient;
        private ComboBox cmbIngredients;
        private Label lblQty;
        private TextBox txtQty;
        private Label lblCost;
        private TextBox txtCost;
        private Label lblDate;
        private DateTimePicker dtpDate;
        private Button btnSave;
        private DataGridView dgvHistory;

        public PurchasesForm()
        {
            this.Text = "Record Purchase (Expense)";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(850, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadIngredientsDropdown();
            LoadPurchaseHistory();
        }

        private void InitializeLayout()
        {
            // ---------- MAIN CONTENT PANEL ----------
            contentPanel = new Panel();
            contentPanel.Size = new Size(800, 550);
            contentPanel.BackColor = Color.WhiteSmoke;
            this.Controls.Add(contentPanel);

            int x = 20;
            int y = 20;

            // ---------- LABEL: Ingredient ----------
            lblIngredient = new Label();
            lblIngredient.Text = "Select Ingredient:";
            lblIngredient.Location = new Point(x, y);
            lblIngredient.AutoSize = true;
            lblIngredient.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblIngredient);

            // ---------- COMBOBOX: Ingredients ----------
            cmbIngredients = new ComboBox();
            cmbIngredients.Location = new Point(x, y + 30);
            cmbIngredients.Width = 300;
            cmbIngredients.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            cmbIngredients.DropDownStyle = ComboBoxStyle.DropDownList;
            contentPanel.Controls.Add(cmbIngredients);

            // ---------- LABEL: Quantity ----------
            y += 70;
            lblQty = new Label();
            lblQty.Text = "Quantity Bought:";
            lblQty.Location = new Point(x, y);
            lblQty.AutoSize = true;
            lblQty.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblQty);

            // ---------- TEXTBOX: Quantity ----------
            txtQty = new TextBox();
            txtQty.Location = new Point(x, y + 30);
            txtQty.Width = 120;
            txtQty.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(txtQty);

            // ---------- LABEL: Cost ----------
            lblCost = new Label();
            lblCost.Text = "Total Cost:";
            lblCost.Location = new Point(x + 200, y);
            lblCost.AutoSize = true;
            lblCost.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblCost);

            // ---------- TEXTBOX: Cost ----------
            txtCost = new TextBox();
            txtCost.Location = new Point(x + 200, y + 30);
            txtCost.Width = 120;
            txtCost.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(txtCost);

            // ---------- LABEL: Date ----------
            y += 70;
            lblDate = new Label();
            lblDate.Text = "Purchase Date:";
            lblDate.Location = new Point(x, y);
            lblDate.AutoSize = true;
            lblDate.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            contentPanel.Controls.Add(lblDate);

            // ---------- DATEPICKER ----------
            dtpDate = new DateTimePicker();
            dtpDate.Location = new Point(x, y + 30);
            dtpDate.Width = 200;
            dtpDate.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            dtpDate.Format = DateTimePickerFormat.Short;
            contentPanel.Controls.Add(dtpDate);

            // ---------- BUTTON: Save ----------
            y += 70;
            btnSave = new Button();
            btnSave.Text = "Save Purchase";
            btnSave.Location = new Point(x, y);
            btnSave.Width = 160;
            btnSave.Height = 40;
            btnSave.BackColor = Color.LightCoral;
            btnSave.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            contentPanel.Controls.Add(btnSave);

            // ---------- DATAGRIDVIEW: Purchase History ----------
            dgvHistory = new DataGridView();
            dgvHistory.Location = new Point(x, y + 60);
            dgvHistory.Size = new Size(760, 250);
            dgvHistory.ReadOnly = true;
            dgvHistory.AllowUserToAddRows = false;
            dgvHistory.RowHeadersVisible = false;
            dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvHistory.Font = new Font("Segoe UI", 11, FontStyle.Regular);
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

        // ---------- DATABASE LOGIC (unchanged) ----------
        private void LoadIngredientsDropdown()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT i.ingredient_id, i.name || ' (' || u.short_code || ')' as DisplayName 
                    FROM Ingredients i
                    JOIN Units u ON i.unit_id = u.unit_id";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbIngredients.DataSource = dt;
                    cmbIngredients.DisplayMember = "DisplayName";
                    cmbIngredients.ValueMember = "ingredient_id";
                }
            }
        }

        private void LoadPurchaseHistory()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT 
                        p.purchase_date, 
                        i.name as Item, 
                        p.quantity, 
                        p.total_cost 
                    FROM Purchases p
                    JOIN Ingredients i ON p.ingredient_id = i.ingredient_id
                    ORDER BY p.purchase_date DESC";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvHistory.DataSource = dt;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cmbIngredients.SelectedIndex == -1) { MessageBox.Show("Select an ingredient."); return; }
            if (!double.TryParse(txtQty.Text, out double qty)) { MessageBox.Show("Invalid Quantity."); return; }
            if (!decimal.TryParse(txtCost.Text, out decimal cost)) { MessageBox.Show("Invalid Cost."); return; }

            int ingredientId = Convert.ToInt32(cmbIngredients.SelectedValue);

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlPurchase = @"
                            INSERT INTO Purchases (ingredient_id, quantity, total_cost, purchase_date) 
                            VALUES (@id, @qty, @cost, @date)";
                        using (var cmd = new SQLiteCommand(sqlPurchase, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", ingredientId);
                            cmd.Parameters.AddWithValue("@qty", qty);
                            cmd.Parameters.AddWithValue("@cost", cost);
                            cmd.Parameters.AddWithValue("@date", dtpDate.Value);
                            cmd.ExecuteNonQuery();
                        }

                        string sqlUpdateStock = "UPDATE Ingredients SET current_stock = current_stock + @qty WHERE ingredient_id = @id";
                        using (var cmd = new SQLiteCommand(sqlUpdateStock, conn))
                        {
                            cmd.Parameters.AddWithValue("@qty", qty);
                            cmd.Parameters.AddWithValue("@id", ingredientId);
                            cmd.ExecuteNonQuery();
                        }

                        transaction.Commit();
                        MessageBox.Show("Purchase Saved & Stock Updated!");
                        txtQty.Clear();
                        txtCost.Clear();
                        LoadPurchaseHistory();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void PurchasesForm_Load(object sender, EventArgs e)
        {

        }
    }
}
