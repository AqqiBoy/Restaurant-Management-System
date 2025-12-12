using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class RecipesForm : Form
    {
        // UI Controls
        private Label lblSelectMenu;
        private ComboBox cmbMenu;

        private GroupBox grpIngredients;
        private Label lblIngredient;
        private ComboBox cmbIngredients;
        private Label lblQty;
        private TextBox txtQty;
        private Label lblUnitPreview;

        // Changed: This button now just adds to the grid, doesn't save to DB yet
        private Button btnAddToGrid;

        private DataGridView dgvRecipe;

        // NEW: The main save button
        private Button btnSaveRecipe;

        // Variable to hold the list in memory
        private DataTable dtCurrentRecipe;

        public RecipesForm()
        {
            this.Text = "Manage Recipes (Bulk Editor)";
            this.Size = new Size(650, 650);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();

            LoadMenuItems();
            LoadIngredients();
            InitializeRecipeTable(); // Setup the temporary list structure
        }

        private void InitializeLayout()
        {

            this.Dock = DockStyle.Fill; // Allow it to stretch
            this.BackColor = Color.White; // Ensure background matches

            int x = 20; int y = 20;

            // 1. Menu Selection
            lblSelectMenu = new Label() { Text = "1. Select Menu Item to Edit:", Location = new Point(x, y), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
            this.Controls.Add(lblSelectMenu);

            cmbMenu = new ComboBox() { Location = new Point(x, y + 25), Width = 300, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbMenu.SelectedIndexChanged += CmbMenu_SelectedIndexChanged;
            this.Controls.Add(cmbMenu);

            // 2. Ingredient Inputs
            y += 70;
            grpIngredients = new GroupBox() { Text = "2. Build the Recipe", Location = new Point(x, y), Size = new Size(580, 100) };
            this.Controls.Add(grpIngredients);

            lblIngredient = new Label() { Text = "Ingredient:", Location = new Point(15, 30), AutoSize = true };
            grpIngredients.Controls.Add(lblIngredient);

            cmbIngredients = new ComboBox() { Location = new Point(15, 55), Width = 200, DropDownStyle = ComboBoxStyle.DropDownList };
            cmbIngredients.SelectedIndexChanged += (s, e) => {
                if (cmbIngredients.SelectedItem is DataRowView row)
                    lblUnitPreview.Text = row["short_code"].ToString();
            };
            grpIngredients.Controls.Add(cmbIngredients);

            lblQty = new Label() { Text = "Quantity:", Location = new Point(230, 30), AutoSize = true };
            grpIngredients.Controls.Add(lblQty);

            txtQty = new TextBox() { Location = new Point(230, 55), Width = 80 };
            grpIngredients.Controls.Add(txtQty);

            lblUnitPreview = new Label() { Text = "...", Location = new Point(320, 58), AutoSize = true };
            grpIngredients.Controls.Add(lblUnitPreview);

            // Button: Add to Grid
            btnAddToGrid = new Button() { Text = "Add to List", Location = new Point(380, 53), Width = 100, BackColor = Color.LightBlue };
            btnAddToGrid.Click += BtnAddToGrid_Click;
            grpIngredients.Controls.Add(btnAddToGrid);

            // Button: Remove Row (Optional but useful)
            Button btnRemoveRow = new Button() { Text = "Remove Selected", Location = new Point(490, 53), Width = 80, BackColor = Color.LightGray, Font = new Font("Segoe UI", 8) };
            btnRemoveRow.Click += (s, e) => {
                if (dgvRecipe.SelectedRows.Count > 0)
                    dgvRecipe.Rows.RemoveAt(dgvRecipe.SelectedRows[0].Index);
            };
            grpIngredients.Controls.Add(btnRemoveRow);

            // 3. Lower Area (Grid + Save Button)
            int lowerTop = y + 120;
            Panel pnlLower = new Panel();
            pnlLower.Location = new Point(20, lowerTop);
            pnlLower.Size = new Size(580, this.ClientSize.Height - lowerTop - 20);
            pnlLower.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            pnlLower.BackColor = Color.White;
            this.Controls.Add(pnlLower);

            // The Grid (fills remaining space above Save)
            dgvRecipe = new DataGridView();
            dgvRecipe.Dock = DockStyle.Fill;
            dgvRecipe.ReadOnly = true;
            dgvRecipe.AllowUserToAddRows = false;
            dgvRecipe.RowHeadersVisible = false;
            dgvRecipe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvRecipe.BackgroundColor = Color.White;
            dgvRecipe.BorderStyle = BorderStyle.None;
            pnlLower.Controls.Add(dgvRecipe);

            // BIG SAVE BUTTON (always visible at bottom)
            btnSaveRecipe = new Button();
            btnSaveRecipe.Text = "SAVE ENTIRE RECIPE";
            btnSaveRecipe.Dock = DockStyle.Bottom;
            btnSaveRecipe.Height = 50;
            btnSaveRecipe.BackColor = Color.LightGreen;
            btnSaveRecipe.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnSaveRecipe.Click += BtnSaveRecipe_Click;
            pnlLower.Controls.Add(btnSaveRecipe);
        }

        // --- SETUP ---

        private void InitializeRecipeTable()
        {
            // Create a temporary table structure to hold data in memory
            dtCurrentRecipe = new DataTable();
            dtCurrentRecipe.Columns.Add("ingredient_id", typeof(int));
            dtCurrentRecipe.Columns.Add("IngredientName", typeof(string));
            dtCurrentRecipe.Columns.Add("Quantity", typeof(double));
            dtCurrentRecipe.Columns.Add("Unit", typeof(string));

            dgvRecipe.DataSource = dtCurrentRecipe;

            // Hide the ID column, users don't need to see it
            dgvRecipe.Columns["ingredient_id"].Visible = false;
        }

        private void LoadMenuItems()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var adapter = new SQLiteDataAdapter("SELECT menu_item_id, name FROM MenuItems ORDER BY name", conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbMenu.DataSource = dt;
                    cmbMenu.DisplayMember = "name";
                    cmbMenu.ValueMember = "menu_item_id";
                }
            }
        }

        private void LoadIngredients()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "SELECT i.ingredient_id, i.name, u.short_code FROM Ingredients i JOIN Units u ON i.unit_id = u.unit_id ORDER BY i.name";
                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    cmbIngredients.DataSource = dt;
                    cmbIngredients.DisplayMember = "name";
                    cmbIngredients.ValueMember = "ingredient_id";
                }
            }
        }

        // --- LOGIC ---

        // 1. Add item to the TEMPORARY list (Grid)
        private void BtnAddToGrid_Click(object sender, EventArgs e)
        {
            if (cmbIngredients.SelectedIndex == -1) return;
            if (!double.TryParse(txtQty.Text, out double qty) || qty <= 0)
            {
                MessageBox.Show("Invalid Quantity"); return;
            }

            // Get selected ingredient details
            DataRowView row = cmbIngredients.SelectedItem as DataRowView;
            int id = Convert.ToInt32(row["ingredient_id"]);
            string name = row["name"].ToString();
            string unit = row["short_code"].ToString();

            // Check if already in grid, if so, update quantity instead of adding duplicate
            foreach (DataRow r in dtCurrentRecipe.Rows)
            {
                if (Convert.ToInt32(r["ingredient_id"]) == id)
                {
                    r["Quantity"] = Convert.ToDouble(r["Quantity"]) + qty; // Add to existing
                    txtQty.Clear();
                    return;
                }
            }

            // Add new row to DataTable
            dtCurrentRecipe.Rows.Add(id, name, qty, unit);
            txtQty.Clear();
            cmbIngredients.Focus();
        }

        // 2. Load existing recipe from DB into the Grid (For Editing)
        private void CmbMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMenu.SelectedIndex == -1) return;

            // Clear current list
            dtCurrentRecipe.Rows.Clear();

            int menuId = Convert.ToInt32(cmbMenu.SelectedValue);

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT r.ingredient_id, i.name as IngredientName, r.quantity_required as Quantity, u.short_code as Unit
                    FROM Recipes r
                    JOIN Ingredients i ON r.ingredient_id = i.ingredient_id
                    JOIN Units u ON i.unit_id = u.unit_id
                    WHERE r.menu_item_id = @mid";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mid", menuId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dtCurrentRecipe.Rows.Add(
                                reader["ingredient_id"],
                                reader["IngredientName"],
                                reader["Quantity"],
                                reader["Unit"]
                            );
                        }
                    }
                }
            }
        }

        // 3. Save Everything to DB
        private void BtnSaveRecipe_Click(object sender, EventArgs e)
        {
            if (cmbMenu.SelectedIndex == -1) { MessageBox.Show("Select a Menu Item first."); return; }
            if (dtCurrentRecipe.Rows.Count == 0)
            {
                if (MessageBox.Show("The list is empty. This will clear the recipe. Continue?", "Warning", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            }

            int menuId = Convert.ToInt32(cmbMenu.SelectedValue);

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // Step A: DELETE existing recipe for this item
                        string sqlDelete = "DELETE FROM Recipes WHERE menu_item_id = @mid";
                        using (var cmd = new SQLiteCommand(sqlDelete, conn))
                        {
                            cmd.Parameters.AddWithValue("@mid", menuId);
                            cmd.ExecuteNonQuery();
                        }

                        // Step B: INSERT all rows from the grid
                        string sqlInsert = "INSERT INTO Recipes (menu_item_id, ingredient_id, quantity_required) VALUES (@mid, @iid, @qty)";

                        foreach (DataRow row in dtCurrentRecipe.Rows)
                        {
                            using (var cmd = new SQLiteCommand(sqlInsert, conn))
                            {
                                cmd.Parameters.AddWithValue("@mid", menuId);
                                cmd.Parameters.AddWithValue("@iid", row["ingredient_id"]);
                                cmd.Parameters.AddWithValue("@qty", row["Quantity"]);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("Recipe Saved Successfully!");
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }
    }
}
