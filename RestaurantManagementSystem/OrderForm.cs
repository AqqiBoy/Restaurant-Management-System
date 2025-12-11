using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class OrderForm : Form
    {
        // --- UI PANELS ---
        private Panel pnlLeft_Categories;
        private Panel pnlMid_Menu;
        private Panel pnlRight_Bill;

        // --- CONTROLS ---
        private FlowLayoutPanel flowMenu; // Middle area for buttons
        private TextBox txtSearch;        // Search bar
        private DataGridView dgvBill;     // The Bill
        private Label lblTotal;           // Total Amount
        private Button btnPlaceOrder;     // Save Button

        // Dynamic Inputs for Order Info
        private Label lblInfo;            // "Table #" or "Customer Name"
        private TextBox txtInfo;          // Input box
        private Label lblPhone;           // "Phone" (Only for delivery)
        private TextBox txtPhone;         // Input box

        // --- DATA VARIABLES ---
        private string CurrentOrderType; // "DineIn", "TakeAway", "Delivery"
        private DataTable dtBill;        // In-memory bill
        private double totalAmount = 0;

        // --- CONSTRUCTOR ---
        public OrderForm(string orderType)
        {
            this.CurrentOrderType = orderType;

            this.Text = $"{orderType} Order";
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            InitializeLayout();
            InitializeBillTable();

            LoadCategories();
            LoadMenuItems(0, ""); // Load All
        }

        // --- 1. DESIGN THE LAYOUT ---
        private void InitializeLayout()
        {
            // A. LEFT PANEL (Categories)
            pnlLeft_Categories = new Panel();
            pnlLeft_Categories.Dock = DockStyle.Left;
            pnlLeft_Categories.Width = 180;
            pnlLeft_Categories.BackColor = Color.WhiteSmoke;
            this.Controls.Add(pnlLeft_Categories);

            // --- NEW: ADD BACK BUTTON ---
            Button btnBack = new Button();
            btnBack.Text = "🡸 BACK"; // Arrow symbol
            btnBack.Dock = DockStyle.Top;
            btnBack.Height = 50;
            btnBack.BackColor = Color.FromArgb(40, 40, 50); // Slightly darker
            btnBack.ForeColor = Color.White;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            btnBack.Click += (s, e) => this.Close(); // This triggers the Dashboard to reappear
            pnlLeft_Categories.Controls.Add(btnBack);

            Label lblCatTitle = new Label() { Text = "CATEGORIES", Dock = DockStyle.Top, Height = 40, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 15, FontStyle.Bold) };
            pnlLeft_Categories.Controls.Add(lblCatTitle);

            // B. RIGHT PANEL (Bill & Checkout)
            pnlRight_Bill = new Panel();
            pnlRight_Bill.Dock = DockStyle.Right;
            pnlRight_Bill.Width = 380;
            pnlRight_Bill.BackColor = Color.WhiteSmoke;
            pnlRight_Bill.Padding = new Padding(10);
            this.Controls.Add(pnlRight_Bill);

            SetupRightPanel(); // Helper function below

            // C. MID PANEL (Menu Grid)
            pnlMid_Menu = new Panel();
            pnlMid_Menu.Dock = DockStyle.Fill;
            pnlMid_Menu.BackColor = Color.White;
            pnlMid_Menu.Padding = new Padding(10);
            this.Controls.Add(pnlMid_Menu);

            // Search Bar at Top of Mid Panel
            Panel pnlSearch = new Panel() { Dock = DockStyle.Top, Height = 50 };
            pnlMid_Menu.Controls.Add(pnlSearch);

            Label lblSearch = new Label() { Text = "Search Item:", Location = new Point(0, 15), AutoSize = true, Font = new Font("Segoe UI", 10) };
            pnlSearch.Controls.Add(lblSearch);

            txtSearch = new TextBox() { Location = new Point(90, 12), Width = 250, Font = new Font("Segoe UI", 10) };
            txtSearch.TextChanged += (s, e) => LoadMenuItems(0, txtSearch.Text); // Live Search
            pnlSearch.Controls.Add(txtSearch);

            // The Flow Layout for Menu Items
            flowMenu = new FlowLayoutPanel();
            flowMenu.Dock = DockStyle.Fill;
            flowMenu.AutoScroll = true;
            pnlMid_Menu.Controls.Add(flowMenu);
            flowMenu.BringToFront();
        }

        private void SetupRightPanel()
        {
            // 1. Title
            Label lblTitle = new Label() { Text = $"New {CurrentOrderType}", Dock = DockStyle.Top, Height = 40, Font = new Font("Segoe UI", 16, FontStyle.Bold, GraphicsUnit.Point) };
            pnlRight_Bill.Controls.Add(lblTitle);

            // 2. Dynamic Inputs (Table vs Name)
            Panel pnlInfo = new Panel() { Dock = DockStyle.Top, Height = 80 };
            pnlRight_Bill.Controls.Add(pnlInfo);

            lblInfo = new Label() { Location = new Point(5, 10), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtInfo = new TextBox() { Location = new Point(5, 35), Width = 150, Font = new Font("Segoe UI", 10) };
            pnlInfo.Controls.Add(lblInfo);
            pnlInfo.Controls.Add(txtInfo);

            if (CurrentOrderType == "Dine In")
            {
                lblInfo.Text = "Table Number:";
            }
            else // Takeaway or Delivery
            {
                lblInfo.Text = "Customer Name:";
                txtInfo.Width = 200;

                // Add Phone Field for non-DineIn
                lblPhone = new Label() { Text = "Phone:", Location = new Point(220, 10), AutoSize = true, Font = new Font("Segoe UI", 10) };
                txtPhone = new TextBox() { Location = new Point(220, 35), Width = 120, Font = new Font("Segoe UI", 10) };
                pnlInfo.Controls.Add(lblPhone);
                pnlInfo.Controls.Add(txtPhone);
            }

            // 3. Bottom Section (Total & Button)
            Panel pnlBottom = new Panel() { Dock = DockStyle.Bottom, Height = 100 };
            pnlRight_Bill.Controls.Add(pnlBottom);

            btnPlaceOrder = new Button() { Text = "PLACE ORDER", Dock = DockStyle.Bottom, Height = 50, BackColor = Color.SeaGreen, ForeColor = Color.White, Font = new Font("Segoe UI", 12, FontStyle.Bold), FlatStyle = FlatStyle.Flat };
            btnPlaceOrder.Click += BtnPlaceOrder_Click;
            pnlBottom.Controls.Add(btnPlaceOrder);

            lblTotal = new Label() { Text = "Total: 0.00", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.DarkRed };
            pnlBottom.Controls.Add(lblTotal);

            // 4. Grid (The Bill)
            dgvBill = new DataGridView();
            dgvBill.Dock = DockStyle.Fill;
            dgvBill.BackgroundColor = Color.White;
            dgvBill.BorderStyle = BorderStyle.None;
            dgvBill.RowHeadersVisible = false;
            dgvBill.AllowUserToAddRows = false;
            dgvBill.ReadOnly = true;
            dgvBill.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBill.CellClick += DgvBill_CellClick; // For Remove Button
            pnlRight_Bill.Controls.Add(dgvBill);
            dgvBill.BringToFront(); // Put it between Top and Bottom panels
        }

        // --- 2. DATA LOADING LOGIC ---

        private void InitializeBillTable()
        {
            dtBill = new DataTable();
            dtBill.Columns.Add("menu_item_id", typeof(int));
            dtBill.Columns.Add("Item", typeof(string));
            dtBill.Columns.Add("Qty", typeof(int));
            dtBill.Columns.Add("Price", typeof(double));
            dtBill.Columns.Add("Total", typeof(double));

            dgvBill.DataSource = dtBill;

            // Format Grid
            dgvBill.Columns["menu_item_id"].Visible = false;
            dgvBill.Columns["Item"].FillWeight = 120;
            dgvBill.Columns["Qty"].FillWeight = 40;
            dgvBill.Columns["Price"].Visible = false; // Hide single price to save space
            dgvBill.Columns["Total"].FillWeight = 60;

            // Add Remove Button Column
            DataGridViewButtonColumn btnRemove = new DataGridViewButtonColumn();
            btnRemove.Name = "Remove";
            btnRemove.HeaderText = "";
            btnRemove.Text = "X";
            btnRemove.UseColumnTextForButtonValue = true;
            btnRemove.FillWeight = 30;
            btnRemove.FlatStyle = FlatStyle.Popup;
            btnRemove.DefaultCellStyle.ForeColor = Color.Red;
            dgvBill.Columns.Add(btnRemove);
        }

        private void LoadCategories()
        {
            // "All" Button
            CreateCategoryButton(0, "ALL ITEMS");

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var adapter = new SQLiteDataAdapter("SELECT category_id, name FROM Categories ORDER BY name", conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    foreach (DataRow row in dt.Rows)
                    {
                        CreateCategoryButton(Convert.ToInt32(row["category_id"]), row["name"].ToString());
                    }
                }
            }
        }

        private void CreateCategoryButton(int id, string name)
        {
            Button btn = new Button();
            btn.Text = name;
            btn.Tag = id;
            btn.Dock = DockStyle.Top; // Stack vertically
            btn.Height = 50;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = Color.WhiteSmoke;
            btn.ForeColor = Color.FromArgb(64, 64, 64);
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            btn.Click += (s, e) => {
                LoadMenuItems(id, ""); // Reset search when category changes
                txtSearch.Clear();
            };

            // Hover
            // 3. CHANGE: Update Hover Effects for Light Theme
            btn.MouseEnter += (s, e) => {
                btn.BackColor = Color.Gainsboro; // Light Gray when hovering
                btn.ForeColor = Color.Black;
            };

            btn.MouseLeave += (s, e) => {
                btn.BackColor = Color.WhiteSmoke; // Back to normal
                btn.ForeColor = Color.FromArgb(64, 64, 64);
            };

            pnlLeft_Categories.Controls.Add(btn);
            pnlLeft_Categories.Controls.SetChildIndex(btn, 0); // Correct order
        }

        private void LoadMenuItems(int categoryId, string searchText)
        {
            flowMenu.Controls.Clear();
            flowMenu.SuspendLayout(); // Optimization

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                string sql = "SELECT * FROM MenuItems WHERE 1=1";

                if (categoryId > 0) sql += $" AND category_id = {categoryId}";
                if (!string.IsNullOrEmpty(searchText)) sql += $" AND name LIKE '%{searchText}%'";

                using (var adapter = new SQLiteDataAdapter(sql, conn))
                {
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    foreach (DataRow row in dt.Rows)
                    {
                        CreateMenuItemButton(row);
                    }
                }
            }
            flowMenu.ResumeLayout();
        }

        private void CreateMenuItemButton(DataRow row)
        {
            int id = Convert.ToInt32(row["menu_item_id"]);
            string name = row["name"].ToString();
            double price = Convert.ToDouble(row["price"]);

            Button btn = new Button();
            btn.Size = new Size(130, 80);
            btn.BackColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderColor = Color.LightGray;

            // Store data in the button
            btn.Tag = new { Id = id, Name = name, Price = price };

            // Text Layout
            btn.Text = $"{name}\n\n{price}";
            btn.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btn.ForeColor = Color.DarkSlateGray;

            btn.Click += MenuItem_Click;
            flowMenu.Controls.Add(btn);
        }

        // --- 3. INTERACTION LOGIC ---

        private void MenuItem_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            dynamic data = btn.Tag;

            // Check if exists
            foreach (DataRow row in dtBill.Rows)
            {
                if (Convert.ToInt32(row["menu_item_id"]) == data.Id)
                {
                    // Increment Qty
                    int newQty = Convert.ToInt32(row["Qty"]) + 1;
                    row["Qty"] = newQty;
                    row["Total"] = newQty * data.Price;
                    UpdateTotal();
                    return;
                }
            }

            // Add new
            dtBill.Rows.Add(data.Id, data.Name, 1, data.Price, data.Price);
            UpdateTotal();
        }

        private void DgvBill_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            // If "Remove" button clicked (Last Column)
            if (e.RowIndex >= 0 && dgvBill.Columns[e.ColumnIndex].Name == "Remove")
            {
                dtBill.Rows.RemoveAt(e.RowIndex);
                UpdateTotal();
            }
        }

        private void UpdateTotal()
        {
            totalAmount = 0;
            foreach (DataRow row in dtBill.Rows)
            {
                totalAmount += Convert.ToDouble(row["Total"]);
            }
            lblTotal.Text = "Total: " + totalAmount.ToString("N2");
        }

        private void BtnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (dtBill.Rows.Count == 0) { MessageBox.Show("Cart is empty"); return; }
            if (string.IsNullOrWhiteSpace(txtInfo.Text)) { MessageBox.Show("Please enter Table No / Name"); return; }

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Insert Order Header
                        string sqlOrder = @"
                            INSERT INTO Orders (order_type, table_number, customer_name, customer_phone, total_amount) 
                            VALUES (@type, @table, @name, @phone, @total); 
                            SELECT last_insert_rowid();";

                        long orderId;
                        using (var cmd = new SQLiteCommand(sqlOrder, conn))
                        {
                            cmd.Parameters.AddWithValue("@type", CurrentOrderType);
                            cmd.Parameters.AddWithValue("@total", totalAmount);

                            // Handle logic based on type
                            if (CurrentOrderType == "Dine In")
                            {
                                cmd.Parameters.AddWithValue("@table", txtInfo.Text);
                                cmd.Parameters.AddWithValue("@name", DBNull.Value);
                                cmd.Parameters.AddWithValue("@phone", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@table", DBNull.Value);
                                cmd.Parameters.AddWithValue("@name", txtInfo.Text);
                                cmd.Parameters.AddWithValue("@phone", txtPhone != null ? txtPhone.Text : "");
                            }

                            orderId = (long)cmd.ExecuteScalar();
                        }

                        // 2. Insert Details & Update Stock
                        string sqlDetail = "INSERT INTO OrderDetails (order_id, menu_item_id, quantity, price_per_item, total_price) VALUES (@oid, @mid, @qty, @price, @total)";

                        string sqlStock = @"
                            UPDATE Ingredients 
                            SET current_stock = ROUND(current_stock - (SELECT quantity_required * @qty FROM Recipes WHERE menu_item_id = @mid AND ingredient_id = Ingredients.ingredient_id), 3)
                            WHERE ingredient_id IN (SELECT ingredient_id FROM Recipes WHERE menu_item_id = @mid)";

                        foreach (DataRow row in dtBill.Rows)
                        {
                            int mid = Convert.ToInt32(row["menu_item_id"]);
                            int qty = Convert.ToInt32(row["Qty"]);

                            using (var cmd = new SQLiteCommand(sqlDetail, conn))
                            {
                                cmd.Parameters.AddWithValue("@oid", orderId);
                                cmd.Parameters.AddWithValue("@mid", mid);
                                cmd.Parameters.AddWithValue("@qty", qty);
                                cmd.Parameters.AddWithValue("@price", row["Price"]);
                                cmd.Parameters.AddWithValue("@total", row["Total"]);
                                cmd.ExecuteNonQuery();
                            }

                            using (var cmd = new SQLiteCommand(sqlStock, conn))
                            {
                                cmd.Parameters.AddWithValue("@qty", qty);
                                cmd.Parameters.AddWithValue("@mid", mid);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        transaction.Commit();
                        MessageBox.Show("Order Placed Successfully!");

                        // Clear Screen
                        dtBill.Rows.Clear();
                        UpdateTotal();
                        txtInfo.Clear();
                        if (txtPhone != null) txtPhone.Clear();
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