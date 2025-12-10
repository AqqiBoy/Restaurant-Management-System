//using System;
//using System.Data;
//using System.Data.SQLite;
//using System.Drawing;
//using System.Windows.Forms;

//namespace RestaurantManagementSystem
//{
//    public partial class RecipesForm : Form
//    {
//        // UI Controls
//        private Label lblSelectMenu;
//        private ComboBox cmbMenu; // Pick "Chicken Tikka" here

//        private GroupBox grpIngredients; // Group box to make it look clean
//        private Label lblIngredient;
//        private ComboBox cmbIngredients; // Pick "Raw Chicken"
//        private Label lblQty;
//        private TextBox txtQty;
//        private Label lblUnitPreview; // Shows "kg" or "pcs" automatically
//        private Button btnAdd;

//        private DataGridView dgvRecipe; // Shows the recipe for the selected item

//        public RecipesForm()
//        {
//            this.Text = "Manage Recipes";
//            this.Size = new Size(600, 600);
//            this.StartPosition = FormStartPosition.CenterParent;

//            InitializeLayout();

//            LoadMenuItems();
//            LoadIngredients();
//        }

//        private void InitializeLayout()
//        {
//            int x = 20; int y = 20;

//            // 1. Master Selection (Menu Item)
//            lblSelectMenu = new Label() { Text = "1. Select Menu Item:", Location = new Point(x, y), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };
//            this.Controls.Add(lblSelectMenu);

//            cmbMenu = new ComboBox() { Location = new Point(x, y + 25), Width = 300 };
//            cmbMenu.DropDownStyle = ComboBoxStyle.DropDownList;
//            cmbMenu.SelectedIndexChanged += CmbMenu_SelectedIndexChanged; // Trigger update when changed
//            this.Controls.Add(cmbMenu);

//            // 2. Add Ingredient Section (Group Box)
//            y += 70;
//            grpIngredients = new GroupBox() { Text = "2. Add Ingredients to this Item", Location = new Point(x, y), Size = new Size(540, 150) };
//            this.Controls.Add(grpIngredients);

//            // Inside GroupBox
//            lblIngredient = new Label() { Text = "Ingredient:", Location = new Point(15, 30), AutoSize = true };
//            grpIngredients.Controls.Add(lblIngredient);

//            cmbIngredients = new ComboBox() { Location = new Point(15, 55), Width = 200 };
//            cmbIngredients.DropDownStyle = ComboBoxStyle.DropDownList;
//            cmbIngredients.SelectedIndexChanged += CmbIngredients_SelectedIndexChanged; // To show unit
//            grpIngredients.Controls.Add(cmbIngredients);

//            lblQty = new Label() { Text = "Quantity Required:", Location = new Point(230, 30), AutoSize = true };
//            grpIngredients.Controls.Add(lblQty);

//            txtQty = new TextBox() { Location = new Point(230, 55), Width = 100 };
//            grpIngredients.Controls.Add(txtQty);

//            // This label will change based on what ingredient you pick (e.g. "kg")
//            lblUnitPreview = new Label() { Text = "...", Location = new Point(340, 58), AutoSize = true, Font = new Font(this.Font, FontStyle.Italic) };
//            grpIngredients.Controls.Add(lblUnitPreview);

//            btnAdd = new Button() { Text = "Add to Recipe", Location = new Point(400, 53), Width = 120, BackColor = Color.LightGreen };
//            btnAdd.Click += BtnAdd_Click;
//            grpIngredients.Controls.Add(btnAdd);

//            // 3. Grid to show current recipe
//            dgvRecipe = new DataGridView();
//            dgvRecipe.Location = new Point(20, y + 170);
//            dgvRecipe.Size = new Size(540, 250);
//            dgvRecipe.ReadOnly = true;
//            dgvRecipe.AllowUserToAddRows = false;
//            dgvRecipe.RowHeadersVisible = false;
//            dgvRecipe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
//            this.Controls.Add(dgvRecipe);
//        }

//        // --- DATA LOADING ---

//        private void LoadMenuItems()
//        {
//            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
//            {
//                conn.Open();
//                using (var adapter = new SQLiteDataAdapter("SELECT menu_item_id, name FROM MenuItems ORDER BY name", conn))
//                {
//                    DataTable dt = new DataTable();
//                    adapter.Fill(dt);
//                    cmbMenu.DataSource = dt;
//                    cmbMenu.DisplayMember = "name";
//                    cmbMenu.ValueMember = "menu_item_id";
//                }
//            }
//        }

//        private void LoadIngredients()
//        {
//            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
//            {
//                conn.Open();
//                // Get ID, Name, and Unit so we can show the unit code
//                string sql = "SELECT i.ingredient_id, i.name, u.short_code FROM Ingredients i JOIN Units u ON i.unit_id = u.unit_id ORDER BY i.name";
//                using (var adapter = new SQLiteDataAdapter(sql, conn))
//                {
//                    DataTable dt = new DataTable();
//                    adapter.Fill(dt);
//                    cmbIngredients.DataSource = dt;
//                    cmbIngredients.DisplayMember = "name";
//                    cmbIngredients.ValueMember = "ingredient_id";
//                }
//            }
//        }

//        // --- EVENTS ---

//        // When user picks a Menu Item, load its existing recipe
//        private void CmbMenu_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            if (cmbMenu.SelectedValue == null) return;
//            LoadRecipeForSelectedMenu();
//        }

//        // When user picks an Ingredient, show its unit (e.g. "kg") next to the textbox
//        private void CmbIngredients_SelectedIndexChanged(object sender, EventArgs e)
//        {
//            if (cmbIngredients.SelectedIndex != -1)
//            {
//                DataRowView row = cmbIngredients.SelectedItem as DataRowView;
//                lblUnitPreview.Text = row["short_code"].ToString();
//            }
//        }

//        private void LoadRecipeForSelectedMenu()
//        {
//            // Safeguard: Ensure value is an integer
//            if (cmbMenu.SelectedValue is DataRowView) return;

//            int menuId = Convert.ToInt32(cmbMenu.SelectedValue);

//            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
//            {
//                conn.Open();
//                string sql = @"
//                    SELECT r.recipe_id, i.name as Ingredient, r.quantity_required, u.short_code as Unit
//                    FROM Recipes r
//                    JOIN Ingredients i ON r.ingredient_id = i.ingredient_id
//                    JOIN Units u ON i.unit_id = u.unit_id
//                    WHERE r.menu_item_id = @mid";

//                using (var adapter = new SQLiteDataAdapter(sql, conn))
//                {
//                    adapter.SelectCommand.Parameters.AddWithValue("@mid", menuId);
//                    DataTable dt = new DataTable();
//                    adapter.Fill(dt);
//                    dgvRecipe.DataSource = dt;
//                }
//            }
//        }

//        private void BtnAdd_Click(object sender, EventArgs e)
//        {
//            if (cmbMenu.SelectedIndex == -1 || cmbIngredients.SelectedIndex == -1) return;
//            if (!double.TryParse(txtQty.Text, out double qty)) { MessageBox.Show("Invalid Quantity"); return; }

//            int menuId = Convert.ToInt32(cmbMenu.SelectedValue);
//            int ingId = Convert.ToInt32(cmbIngredients.SelectedValue);

//            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
//            {
//                conn.Open();
//                string sql = "INSERT INTO Recipes (menu_item_id, ingredient_id, quantity_required) VALUES (@mid, @iid, @qty)";

//                using (var cmd = new SQLiteCommand(sql, conn))
//                {
//                    cmd.Parameters.AddWithValue("@mid", menuId);
//                    cmd.Parameters.AddWithValue("@iid", ingId);
//                    cmd.Parameters.AddWithValue("@qty", qty);
//                    try
//                    {
//                        cmd.ExecuteNonQuery();
//                        txtQty.Clear();
//                        LoadRecipeForSelectedMenu(); // Refresh grid
//                    }
//                    catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
//                }
//            }
//        }
//    }
//}



using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class RecipesForm : Form
    {
        private Panel contentPanel;

        private Label lblSelectMenu;
        private ComboBox cmbMenu;

        private GroupBox grpIngredients;
        private Label lblIngredient;
        private ComboBox cmbIngredients;
        private Label lblQty;
        private TextBox txtQty;
        private Label lblUnitPreview;
        private Button btnAdd;

        private DataGridView dgvRecipe;

        public RecipesForm()
        {
            this.Text = "Manage Recipes";
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;
            this.Size = new Size(700, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();

            LoadMenuItems();
            LoadIngredients();
        }

        private void InitializeLayout()
        {
            // ---------- MAIN CONTENT PANEL ----------
            contentPanel = new Panel
            {
                Size = new Size(760, 520), // slightly smaller than form
                BackColor = Color.WhiteSmoke
            };
            this.Controls.Add(contentPanel);

            // Give proper top-left margin
            contentPanel.Location = new Point(20, 20); 

           int x = 20;
            int y = 20;

            // ---------- LABEL: Select Menu ----------
            lblSelectMenu = new Label
            {
                Text = "Select Menu Item:",
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            contentPanel.Controls.Add(lblSelectMenu);

            // ---------- COMBOBOX: Menu Items ----------
            cmbMenu = new ComboBox
            {
                Location = new Point(x, y + 30),
                Width = 300,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbMenu.SelectedIndexChanged += CmbMenu_SelectedIndexChanged;
            contentPanel.Controls.Add(cmbMenu);

            // ---------- GROUPBOX: Ingredients ----------
            y += 80;
            grpIngredients = new GroupBox
            {
                Text = "Add Ingredients to this Item",
                Location = new Point(x, y),
                Size = new Size(760, 150), // increased height for spacing
                Font = new Font("Segoe UI", 12, FontStyle.Bold)
            };
            contentPanel.Controls.Add(grpIngredients);

            int gx = 15; // groupbox inner X margin
            int gy = 30; // groupbox inner Y margin

            // Ingredient Label
            lblIngredient = new Label
            {
                Text = "Ingredient:",
                Location = new Point(gx, gy),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpIngredients.Controls.Add(lblIngredient);

            // Ingredient ComboBox
            cmbIngredients = new ComboBox
            {
                Location = new Point(gx, gy + 30),
                Width = 250,
                Font = new Font("Segoe UI", 12, FontStyle.Regular),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cmbIngredients.SelectedIndexChanged += CmbIngredients_SelectedIndexChanged;
            grpIngredients.Controls.Add(cmbIngredients);

            // Quantity Label
            lblQty = new Label
            {
                Text = "Quantity Required:",
                Location = new Point(300, gy),
                AutoSize = true,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpIngredients.Controls.Add(lblQty);

            // Quantity TextBox
            txtQty = new TextBox
            {
                Location = new Point(300, gy + 30),
                Width = 100,
                Font = new Font("Segoe UI", 12, FontStyle.Regular)
            };
            grpIngredients.Controls.Add(txtQty);

            // Unit Preview
            lblUnitPreview = new Label
            {
                Text = "...",
                Location = new Point(410, gy + 33),
                AutoSize = true,
                Font = new Font("Segoe UI", 11, FontStyle.Italic)
            };
            grpIngredients.Controls.Add(lblUnitPreview);

            // Add Button
            btnAdd = new Button
            {
                Text = "Add to Recipe",
                Location = new Point(500, gy + 28),
                Width = 150,
                Height = 35,
                BackColor = Color.LightCoral,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat
            };
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.Click += BtnAdd_Click;
            grpIngredients.Controls.Add(btnAdd);

            // ---------- DATAGRIDVIEW: Recipe ----------
            dgvRecipe = new DataGridView
            {
                Location = new Point(x, y + 170),
                Size = new Size(760, 330), // adjust height to fit
                ReadOnly = true,
                AllowUserToAddRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                Font = new Font("Segoe UI", 11, FontStyle.Regular),
                ColumnHeadersDefaultCellStyle =
        {
            Font = new Font("Segoe UI", 11, FontStyle.Bold),
            BackColor = Color.LightGray
        },
                EnableHeadersVisualStyles = false
            };
            contentPanel.Controls.Add(dgvRecipe);

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




        // --- DATABASE LOGIC ---

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

        private void CmbMenu_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbMenu.SelectedValue == null) return;
            LoadRecipeForSelectedMenu();
        }

        private void CmbIngredients_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbIngredients.SelectedIndex != -1)
            {
                DataRowView row = cmbIngredients.SelectedItem as DataRowView;
                lblUnitPreview.Text = row["short_code"].ToString();
            }
        }

        private void LoadRecipeForSelectedMenu()
        {
            if (cmbMenu.SelectedValue is DataRowView) return;

            int menuId = Convert.ToInt32(cmbMenu.SelectedValue);
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = @"
                    SELECT r.recipe_id, i.name as Ingredient, r.quantity_required, u.short_code as Unit
                    FROM Recipes r
                    JOIN Ingredients i ON r.ingredient_id = i.ingredient_id
                    JOIN Units u ON i.unit_id = u.unit_id
                    WHERE r.menu_item_id = @mid";

                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    adapter.SelectCommand.Parameters.AddWithValue("@mid", menuId);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvRecipe.DataSource = dt;
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbMenu.SelectedIndex == -1 || cmbIngredients.SelectedIndex == -1) return;
            if (!double.TryParse(txtQty.Text, out double qty)) { MessageBox.Show("Invalid Quantity"); return; }

            int menuId = Convert.ToInt32(cmbMenu.SelectedValue);
            int ingId = Convert.ToInt32(cmbIngredients.SelectedValue);

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "INSERT INTO Recipes (menu_item_id, ingredient_id, quantity_required) VALUES (@mid, @iid, @qty)";
                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@mid", menuId);
                    cmd.Parameters.AddWithValue("@iid", ingId);
                    cmd.Parameters.AddWithValue("@qty", qty);

                    try
                    {
                        cmd.ExecuteNonQuery();
                        txtQty.Clear();
                        LoadRecipeForSelectedMenu();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error: " + ex.Message);
                    }
                }
            }
        }

        private void RecipesForm_Load(object sender, EventArgs e)
        {

        }

        private void RecipesForm_Load_1(object sender, EventArgs e)
        {

        }
    }
}
