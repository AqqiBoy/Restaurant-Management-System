using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class PurchasesForm : Form
    {
        // UI Controls
        private Label lblIngredient;
        private ComboBox cmbIngredients; // Dropdown to pick what we bought
        private Label lblQty;
        private TextBox txtQty;
        private Label lblCost;
        private TextBox txtCost;
        private Label lblDate;
        private DateTimePicker dtpDate; // Calendar picker
        private Button btnSave;
        private DataGridView dgvHistory; // Show recent purchases

        public PurchasesForm()
        {
            this.Text = "Record Purchase (Expense)";
            this.Size = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();

            LoadIngredientsDropdown();
            LoadPurchaseHistory();
        }

        private void InitializeLayout()
        {
            int x = 20;
            int y = 20;

            // 1. Ingredient Selector
            lblIngredient = new Label() { Text = "Select Ingredient:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblIngredient);

            cmbIngredients = new ComboBox() { Location = new Point(x, y + 25), Width = 250 };
            cmbIngredients.DropDownStyle = ComboBoxStyle.DropDownList;
            this.Controls.Add(cmbIngredients);

            // 2. Quantity Input
            y += 60;
            lblQty = new Label() { Text = "Quantity Bought:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblQty);

            txtQty = new TextBox() { Location = new Point(x, y + 25), Width = 100 };
            this.Controls.Add(txtQty);

            // 3. Cost Input
            // We put Cost next to Quantity to save space
            lblCost = new Label() { Text = "Total Cost:", Location = new Point(x + 150, y), AutoSize = true };
            this.Controls.Add(lblCost);

            txtCost = new TextBox() { Location = new Point(x + 150, y + 25), Width = 100 };
            this.Controls.Add(txtCost);

            // 4. Date Picker
            y += 60;
            lblDate = new Label() { Text = "Purchase Date:", Location = new Point(x, y), AutoSize = true };
            this.Controls.Add(lblDate);

            dtpDate = new DateTimePicker() { Location = new Point(x, y + 25), Width = 250, Format = DateTimePickerFormat.Short };
            this.Controls.Add(dtpDate);

            // 5. Save Button
            y += 60;
            btnSave = new Button() { Text = "Save Purchase", Location = new Point(x, y), Width = 150, Height = 40, BackColor = Color.LightSkyBlue };
            btnSave.Click += new EventHandler(btnSave_Click);
            this.Controls.Add(btnSave);

            // 6. History Grid
            dgvHistory = new DataGridView();
            dgvHistory.Location = new Point(20, y + 60);
            dgvHistory.Size = new Size(540, 250);
            dgvHistory.ReadOnly = true;
            dgvHistory.AllowUserToAddRows = false;
            dgvHistory.RowHeadersVisible = false;
            dgvHistory.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.Controls.Add(dgvHistory);
        }

        // --- DATABASE LOGIC ---

        private void LoadIngredientsDropdown()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                // We display "Raw Chicken (kg)" so the user knows the unit
                string sql = @"
                    SELECT i.ingredient_id, i.name || ' (' || u.short_code || ')' as DisplayName 
                    FROM Ingredients i
                    JOIN Units u ON i.unit_id = u.unit_id";

                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbIngredients.DataSource = dt;
                    cmbIngredients.DisplayMember = "DisplayName"; // Shows "Chicken (kg)"
                    cmbIngredients.ValueMember = "ingredient_id"; // Stores ID 101
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
            // Validation
            if (cmbIngredients.SelectedIndex == -1) { MessageBox.Show("Select an ingredient."); return; }
            if (!double.TryParse(txtQty.Text, out double qty)) { MessageBox.Show("Invalid Quantity."); return; }
            if (!decimal.TryParse(txtCost.Text, out decimal cost)) { MessageBox.Show("Invalid Cost."); return; }

            int ingredientId = Convert.ToInt32(cmbIngredients.SelectedValue);

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();

                // Start a Transaction (Safety: Ensure both Expense AND Stock update happen together)
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert into Purchases Table
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

                        // 2. Update Stock in Ingredients Table
                        string sqlUpdateStock = "UPDATE Ingredients SET current_stock = current_stock + @qty WHERE ingredient_id = @id";

                        using (var cmd = new SQLiteCommand(sqlUpdateStock, conn))
                        {
                            cmd.Parameters.AddWithValue("@qty", qty);
                            cmd.Parameters.AddWithValue("@id", ingredientId);
                            cmd.ExecuteNonQuery();
                        }

                        // Save Changes
                        transaction.Commit();
                        MessageBox.Show("Purchase Saved & Stock Updated!");

                        // Clear Inputs
                        txtQty.Clear();
                        txtCost.Clear();
                        LoadPurchaseHistory();
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback(); // Undo if error
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }
    }
}