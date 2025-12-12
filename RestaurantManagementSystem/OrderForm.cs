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

        // Optional external host for categories (e.g., Form1 sidebar)
        private readonly Control categoriesHost;
        private readonly bool useExternalCategories;

        // --- CONTROLS ---
        private FlowLayoutPanel flowMenu;
        private TextBox txtSearch;
        private DataGridView dgvBill;
        private Label lblTotal;
        private Button btnPlaceOrder;
        private Label lblPaid;
        private TextBox txtPaid;
        private Label lblChange;
        private Label lblChangeValue;

        // Dynamic Inputs
        private Label lblInfo;
        private TextBox txtInfo;
        private Label lblPhone;
        private TextBox txtPhone;
        private Label lblAddress;
        private TextBox txtAddress;

        // --- DATA VARIABLES ---
        private string CurrentOrderType;
        private DataTable dtBill;
        private double totalAmount = 0;

        public OrderForm(string orderType)
            : this(orderType, null)
        {
        }

        public OrderForm(string orderType, Control categoriesHost)
        {
            this.CurrentOrderType = orderType;
            this.categoriesHost = categoriesHost;
            this.useExternalCategories = categoriesHost != null;

            this.Text = $"{orderType} Order";
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            InitializeLayout();
            InitializeBillTable();

            LoadCategories();
            LoadMenuItems(0, "");
        }

        private void InitializeLayoutLegacy()
        {
            this.SuspendLayout();

            // Root layout: 3 columns (Left | Mid | Right).
            // This avoids any dock/z-order overlap when the form is hosted inside another panel.
            TableLayoutPanel mainLayout = new TableLayoutPanel();
            mainLayout.Dock = DockStyle.Fill;
            mainLayout.BackColor = Color.White;
            mainLayout.ColumnCount = 3;
            mainLayout.RowCount = 1;
            mainLayout.Padding = new Padding(0);
            mainLayout.Margin = new Padding(0);

            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));   // Left fixed
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // Mid fills
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));   // Right fixed
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

            this.Controls.Add(mainLayout);

            // 1. LEFT PANEL (Categories)
            pnlLeft_Categories = new Panel();
            pnlLeft_Categories.Dock = DockStyle.Fill;
            pnlLeft_Categories.BackColor = Color.WhiteSmoke;
            pnlLeft_Categories.Margin = new Padding(0);
            mainLayout.Controls.Add(pnlLeft_Categories, 0, 0);

            // 2. MID PANEL (Menu)
            pnlMid_Menu = new Panel();
            pnlMid_Menu.Dock = DockStyle.Fill;
            pnlMid_Menu.BackColor = Color.White;
            pnlMid_Menu.Padding = new Padding(10);
            pnlMid_Menu.Margin = new Padding(0);
            mainLayout.Controls.Add(pnlMid_Menu, 1, 0);

            // 3. RIGHT PANEL (Bill)
            pnlRight_Bill = new Panel();
            pnlRight_Bill.Dock = DockStyle.Fill;
            pnlRight_Bill.BackColor = Color.WhiteSmoke;
            pnlRight_Bill.Padding = new Padding(10);
            pnlRight_Bill.Margin = new Padding(0);
            mainLayout.Controls.Add(pnlRight_Bill, 2, 0);

            // ==========================================
            //      SETUP CONTENT (Same as before)
            // ==========================================

            // --- SETUP LEFT PANEL CONTENT ---
            Button btnBack = new Button();
            btnBack.Text = "🡸 BACK";
            btnBack.Dock = DockStyle.Top;
            btnBack.Height = 50;
            btnBack.BackColor = Color.FromArgb(230, 230, 230);
            btnBack.ForeColor = Color.Black;
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            btnBack.Click += (s, e) => this.Close();
            pnlLeft_Categories.Controls.Add(btnBack);

            Label lblCatTitle = new Label() { Text = "CATEGORIES", Dock = DockStyle.Top, Height = 50, ForeColor = Color.Black, TextAlign = ContentAlignment.MiddleCenter, Font = new Font("Segoe UI", 14, FontStyle.Bold) };
            pnlLeft_Categories.Controls.Add(lblCatTitle);

            // --- SETUP RIGHT PANEL CONTENT ---
            SetupRightPanel();

            // --- SETUP MID PANEL CONTENT ---
            // Search Bar Area
            Panel pnlSearch = new Panel() { Dock = DockStyle.Top, Height = 60 };
            pnlMid_Menu.Controls.Add(pnlSearch);

            Label lblSearch = new Label() { Text = "Search Item:", Location = new Point(10, 18), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pnlSearch.Controls.Add(lblSearch);

            txtSearch = new TextBox() { Location = new Point(100, 15), Height = 30, Font = new Font("Segoe UI", 11) };
            txtSearch.Width = 300;
            txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.TextChanged += (s, e) => LoadMenuItems(0, txtSearch.Text);
            pnlSearch.Controls.Add(txtSearch);

            // Menu Items Flow
            flowMenu = new FlowLayoutPanel();
            flowMenu.Dock = DockStyle.Fill;
            flowMenu.AutoScroll = true;
            flowMenu.BackColor = Color.White;
            pnlMid_Menu.Controls.Add(flowMenu);
            flowMenu.BringToFront();

            this.ResumeLayout(true);
        }

        private void InitializeLayout()
        {
            this.SuspendLayout();

            if (useExternalCategories)
            {
                // Root layout: 2 columns (Mid | Right). Categories are hosted by Form1 sidebar.
                TableLayoutPanel mainLayout = new TableLayoutPanel();
                mainLayout.Dock = DockStyle.Fill;
                mainLayout.BackColor = Color.White;
                mainLayout.ColumnCount = 2;
                mainLayout.RowCount = 1;
                mainLayout.Padding = new Padding(0);
                mainLayout.Margin = new Padding(0);

                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // Mid fills
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));  // Right fixed
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                this.Controls.Add(mainLayout);

                // MID PANEL (Menu)
                pnlMid_Menu = new Panel();
                pnlMid_Menu.Dock = DockStyle.Fill;
                pnlMid_Menu.BackColor = Color.White;
                pnlMid_Menu.Padding = new Padding(10);
                pnlMid_Menu.Margin = new Padding(0);
                mainLayout.Controls.Add(pnlMid_Menu, 0, 0);

                // RIGHT PANEL (Bill)
                pnlRight_Bill = new Panel();
                pnlRight_Bill.Dock = DockStyle.Fill;
                pnlRight_Bill.BackColor = Color.WhiteSmoke;
                pnlRight_Bill.Padding = new Padding(10);
                pnlRight_Bill.Margin = new Padding(0);
                mainLayout.Controls.Add(pnlRight_Bill, 1, 0);
            }
            else
            {
                // Root layout: 3 columns (Left | Mid | Right).
                TableLayoutPanel mainLayout = new TableLayoutPanel();
                mainLayout.Dock = DockStyle.Fill;
                mainLayout.BackColor = Color.White;
                mainLayout.ColumnCount = 3;
                mainLayout.RowCount = 1;
                mainLayout.Padding = new Padding(0);
                mainLayout.Margin = new Padding(0);

                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 200F));   // Left fixed
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));   // Mid fills
                mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 400F));   // Right fixed
                mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

                this.Controls.Add(mainLayout);

                // LEFT PANEL (Categories)
                pnlLeft_Categories = new Panel();
                pnlLeft_Categories.Dock = DockStyle.Fill;
                pnlLeft_Categories.BackColor = Color.WhiteSmoke;
                pnlLeft_Categories.Margin = new Padding(0);
                mainLayout.Controls.Add(pnlLeft_Categories, 0, 0);

                // MID PANEL (Menu)
                pnlMid_Menu = new Panel();
                pnlMid_Menu.Dock = DockStyle.Fill;
                pnlMid_Menu.BackColor = Color.White;
                pnlMid_Menu.Padding = new Padding(10);
                pnlMid_Menu.Margin = new Padding(0);
                mainLayout.Controls.Add(pnlMid_Menu, 1, 0);

                // RIGHT PANEL (Bill)
                pnlRight_Bill = new Panel();
                pnlRight_Bill.Dock = DockStyle.Fill;
                pnlRight_Bill.BackColor = Color.WhiteSmoke;
                pnlRight_Bill.Padding = new Padding(10);
                pnlRight_Bill.Margin = new Padding(0);
                mainLayout.Controls.Add(pnlRight_Bill, 2, 0);

                // LEFT PANEL CONTENT (internal mode only)
                Label lblCatTitle = new Label()
                {
                    Text = "CATEGORIES",
                    Dock = DockStyle.Top,
                    Height = 50,
                    ForeColor = Color.Black,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 14, FontStyle.Bold)
                };
                pnlLeft_Categories.Controls.Add(lblCatTitle);
            }

            // --- SETUP RIGHT PANEL CONTENT ---
            SetupRightPanel();

            // --- SETUP MID PANEL CONTENT ---
            Panel pnlSearch = new Panel() { Dock = DockStyle.Top, Height = 60 };
            pnlMid_Menu.Controls.Add(pnlSearch);

            Label lblSearch = new Label() { Text = "Search Item:", Location = new Point(10, 18), AutoSize = true, Font = new Font("Segoe UI", 10, FontStyle.Bold) };
            pnlSearch.Controls.Add(lblSearch);

            txtSearch = new TextBox() { Location = new Point(100, 15), Height = 30, Font = new Font("Segoe UI", 11) };
            txtSearch.Width = 300;
            txtSearch.Anchor = AnchorStyles.Left | AnchorStyles.Right;
            txtSearch.TextChanged += (s, e) => LoadMenuItems(0, txtSearch.Text);
            pnlSearch.Controls.Add(txtSearch);

            flowMenu = new FlowLayoutPanel();
            flowMenu.Dock = DockStyle.Fill;
            flowMenu.AutoScroll = true;
            flowMenu.BackColor = Color.White;
            pnlMid_Menu.Controls.Add(flowMenu);
            flowMenu.BringToFront();

            this.ResumeLayout(true);
        }

        private void SetupRightPanel()
        {
            // 1. Title
            Label lblTitle = new Label() { Text = $"New {CurrentOrderType}", Dock = DockStyle.Top, Height = 50, Font = new Font("Segoe UI", 18, FontStyle.Bold), ForeColor = Color.DarkSlateBlue };
            pnlRight_Bill.Controls.Add(lblTitle);

            // 2. Info Panel (Table/Name/Phone/Address)
            int infoHeight = (CurrentOrderType == "Delivery") ? 140 : 70;
            Panel pnlInfo = new Panel() { Dock = DockStyle.Top, Height = infoHeight };
            pnlRight_Bill.Controls.Add(pnlInfo);

            lblInfo = new Label() { Location = new Point(0, 5), AutoSize = true, Font = new Font("Segoe UI", 10) };
            txtInfo = new TextBox() { Location = new Point(0, 30), Width = 160, Font = new Font("Segoe UI", 10) };
            pnlInfo.Controls.Add(lblInfo);
            pnlInfo.Controls.Add(txtInfo);

            if (CurrentOrderType == "Dine In")
            {
                lblInfo.Text = "Table Number:";
            }
            else
            {
                lblInfo.Text = "Customer Name:";
                txtInfo.Width = 180;

                lblPhone = new Label() { Text = "Phone:", Location = new Point(200, 5), AutoSize = true, Font = new Font("Segoe UI", 10) };
                txtPhone = new TextBox() { Location = new Point(200, 30), Width = 150, Font = new Font("Segoe UI", 10) };
                pnlInfo.Controls.Add(lblPhone);
                pnlInfo.Controls.Add(txtPhone);

                if (CurrentOrderType == "Delivery")
                {
                    lblAddress = new Label() { Text = "Address:", Location = new Point(0, 70), AutoSize = true, Font = new Font("Segoe UI", 10) };
                    txtAddress = new TextBox()
                    {
                        Location = new Point(0, 90),
                        Width = 350,
                        Height = 40,
                        Multiline = true,
                        Font = new Font("Segoe UI", 10),
                        Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top
                    };
                    pnlInfo.Controls.Add(lblAddress);
                    pnlInfo.Controls.Add(txtAddress);
                }
            }

            // 3. Bottom Section (Total + Calculator + Button)
            Panel pnlBottom = new Panel() { Dock = DockStyle.Bottom, Height = 200 };
            pnlRight_Bill.Controls.Add(pnlBottom);

            TableLayoutPanel bottomLayout = new TableLayoutPanel();
            bottomLayout.Dock = DockStyle.Fill;
            bottomLayout.ColumnCount = 1;
            bottomLayout.RowCount = 3;
            bottomLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));  // Total
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 80F));  // Calculator
            bottomLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 60F));  // Place order
            pnlBottom.Controls.Add(bottomLayout);

            // Total Label
            lblTotal = new Label()
            {
                Text = "Total: 0.00",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 20, FontStyle.Bold),
                ForeColor = Color.DarkRed
            };
            bottomLayout.Controls.Add(lblTotal, 0, 0);

            // Calculator Panel
            Panel pnlCalc = new Panel() { Dock = DockStyle.Fill, BackColor = Color.WhiteSmoke };
            bottomLayout.Controls.Add(pnlCalc, 0, 1);

            TableLayoutPanel calcLayout = new TableLayoutPanel();
            calcLayout.Dock = DockStyle.Fill;
            calcLayout.ColumnCount = 2;
            calcLayout.RowCount = 2;
            calcLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
            calcLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            calcLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            calcLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
            pnlCalc.Controls.Add(calcLayout);

            lblPaid = new Label()
            {
                Text = "Paid:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            calcLayout.Controls.Add(lblPaid, 0, 0);

            txtPaid = new TextBox()
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 11)
            };
            txtPaid.TextChanged += (s, e) => UpdateChange();
            calcLayout.Controls.Add(txtPaid, 1, 0);

            lblChange = new Label()
            {
                Text = "Change:",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            calcLayout.Controls.Add(lblChange, 0, 1);

            lblChangeValue = new Label()
            {
                Text = "0.00",
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.Black
            };
            calcLayout.Controls.Add(lblChangeValue, 1, 1);

            // Place Order Button
            btnPlaceOrder = new Button()
            {
                Text = "PLACE ORDER",
                Dock = DockStyle.Fill,
                BackColor = Color.SeaGreen,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 14, FontStyle.Bold),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand
            };
            btnPlaceOrder.Click += BtnPlaceOrder_Click;
            bottomLayout.Controls.Add(btnPlaceOrder, 0, 2);

            // 4. Grid (The Bill) - Fills the middle
            dgvBill = new DataGridView();
            dgvBill.Dock = DockStyle.Fill;
            dgvBill.BackgroundColor = Color.White;
            dgvBill.BorderStyle = BorderStyle.None;
            dgvBill.RowHeadersVisible = false;
            dgvBill.AllowUserToAddRows = false;
            dgvBill.ReadOnly = true;
            dgvBill.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBill.CellClick += DgvBill_CellClick;
            pnlRight_Bill.Controls.Add(dgvBill);
            dgvBill.BringToFront();
        }

        // --- DATA LOADING (Same as before) ---
        private void InitializeBillTable()
        {
            dtBill = new DataTable();
            dtBill.Columns.Add("menu_item_id", typeof(int));
            dtBill.Columns.Add("Item", typeof(string));
            dtBill.Columns.Add("Qty", typeof(int));
            dtBill.Columns.Add("Price", typeof(double));
            dtBill.Columns.Add("Total", typeof(double));

            dgvBill.DataSource = dtBill;
            dgvBill.Columns["menu_item_id"].Visible = false;
            dgvBill.Columns["Item"].FillWeight = 110;
            dgvBill.Columns["Qty"].FillWeight = 40;
            dgvBill.Columns["Price"].Visible = false;
            dgvBill.Columns["Total"].FillWeight = 60;

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
            if (useExternalCategories && categoriesHost != null)
            {
                categoriesHost.SuspendLayout();
                categoriesHost.Controls.Clear();
            }

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

            if (useExternalCategories && categoriesHost != null)
            {
                categoriesHost.ResumeLayout(true);
                categoriesHost.PerformLayout();
                categoriesHost.Refresh();
            }
        }

        private void CreateCategoryButton(int id, string name)
        {
            Button btn = new Button();
            btn.Text = " " + name; // Padding
            btn.Tag = id;
            btn.Dock = DockStyle.Top;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Cursor = Cursors.Hand;

            if (useExternalCategories)
            {
                btn.Height = 45;
                btn.BackColor = Color.WhiteSmoke;
                btn.ForeColor = Color.FromArgb(64, 64, 64);
                btn.Font = new Font("Segoe UI", 15, FontStyle.Bold);
                btn.UseVisualStyleBackColor = false;
                btn.Padding = new Padding(10, 0, 0, 0);
            }
            else
            {
                btn.Height = 55;
                btn.BackColor = Color.WhiteSmoke;
                btn.ForeColor = Color.FromArgb(64, 64, 64);
                btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            }

            btn.Click += (s, e) => {
                LoadMenuItems(id, "");
                txtSearch.Clear();
            };

            btn.MouseEnter += (s, e) => {
                btn.BackColor = Color.Gainsboro;
                btn.ForeColor = Color.Black;
            };

            btn.MouseLeave += (s, e) => {
                btn.BackColor = Color.WhiteSmoke;
                btn.ForeColor = Color.FromArgb(64, 64, 64);
            };

            if (useExternalCategories && categoriesHost != null)
            {
                categoriesHost.Controls.Add(btn);
                categoriesHost.Controls.SetChildIndex(btn, 0);
            }
            else
            {
                pnlLeft_Categories.Controls.Add(btn);
                pnlLeft_Categories.Controls.SetChildIndex(btn, 0);
            }
        }

        private void LoadMenuItems(int categoryId, string searchText)
        {
            flowMenu.Controls.Clear();
            flowMenu.SuspendLayout();

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
            btn.Size = new Size(130, 90);
            btn.BackColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            // Add a border to make them visible against white background
            btn.FlatAppearance.BorderColor = Color.Silver;
            btn.FlatAppearance.BorderSize = 1;

            btn.Tag = new { Id = id, Name = name, Price = price };

            btn.Text = $"{name}\n\n{price}";
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.ForeColor = Color.DarkSlateGray;
            btn.Cursor = Cursors.Hand;

            btn.Click += MenuItem_Click;
            flowMenu.Controls.Add(btn);
        }

        private void MenuItem_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            dynamic data = btn.Tag;

            foreach (DataRow row in dtBill.Rows)
            {
                if (Convert.ToInt32(row["menu_item_id"]) == data.Id)
                {
                    int newQty = Convert.ToInt32(row["Qty"]) + 1;
                    row["Qty"] = newQty;
                    row["Total"] = newQty * data.Price;
                    UpdateTotal();
                    return;
                }
            }

            dtBill.Rows.Add(data.Id, data.Name, 1, data.Price, data.Price);
            UpdateTotal();
        }

        private void DgvBill_CellClick(object sender, DataGridViewCellEventArgs e)
        {
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
            UpdateChange();
        }

        private void UpdateChange()
        {
            if (txtPaid == null || lblChange == null || lblChangeValue == null) return;

            if (double.TryParse(txtPaid.Text, out double paidAmount))
            {
                double diff = paidAmount - totalAmount;
                if (diff >= 0)
                {
                    lblChange.Text = "Change:";
                    lblChangeValue.Text = diff.ToString("N2");
                    lblChangeValue.ForeColor = Color.DarkGreen;
                }
                else
                {
                    lblChange.Text = "Due:";
                    lblChangeValue.Text = (-diff).ToString("N2");
                    lblChangeValue.ForeColor = Color.DarkRed;
                }
            }
            else
            {
                lblChange.Text = "Change:";
                lblChangeValue.Text = "0.00";
                lblChangeValue.ForeColor = Color.Black;
            }
        }

        private void BtnPlaceOrder_Click(object sender, EventArgs e)
        {
            if (dtBill.Rows.Count == 0) { MessageBox.Show("Cart is empty"); return; }
            if (string.IsNullOrWhiteSpace(txtInfo.Text)) { MessageBox.Show("Please enter Table No / Name"); return; }
            if (CurrentOrderType == "Delivery" && (txtAddress == null || string.IsNullOrWhiteSpace(txtAddress.Text)))
            {
                MessageBox.Show("Please enter Address");
                return;
            }
            if (txtPaid == null || !double.TryParse(txtPaid.Text, out double paidAmount))
            {
                MessageBox.Show("Please enter Paid Amount");
                return;
            }
            if (paidAmount < totalAmount)
            {
                MessageBox.Show("Paid amount is less than total");
                return;
            }

            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    try
                    {
                        string sqlOrder = @"
                            INSERT INTO Orders (order_type, table_number, customer_name, customer_phone, customer_address, total_amount) 
                            VALUES (@type, @table, @name, @phone, @address, @total); 
                            SELECT last_insert_rowid();";

                        long orderId;
                        using (var cmd = new SQLiteCommand(sqlOrder, conn))
                        {
                            cmd.Parameters.AddWithValue("@type", CurrentOrderType);
                            cmd.Parameters.AddWithValue("@total", totalAmount);

                            if (CurrentOrderType == "Dine In")
                            {
                                cmd.Parameters.AddWithValue("@table", txtInfo.Text);
                                cmd.Parameters.AddWithValue("@name", DBNull.Value);
                                cmd.Parameters.AddWithValue("@phone", DBNull.Value);
                                cmd.Parameters.AddWithValue("@address", DBNull.Value);
                            }
                            else
                            {
                                cmd.Parameters.AddWithValue("@table", DBNull.Value);
                                cmd.Parameters.AddWithValue("@name", txtInfo.Text);
                                cmd.Parameters.AddWithValue("@phone", txtPhone != null ? txtPhone.Text : "");
                                if (CurrentOrderType == "Delivery")
                                {
                                    cmd.Parameters.AddWithValue("@address", txtAddress != null ? txtAddress.Text : "");
                                }
                                else
                                {
                                    cmd.Parameters.AddWithValue("@address", DBNull.Value);
                                }
                            }

                            orderId = (long)cmd.ExecuteScalar();
                        }

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

                        dtBill.Rows.Clear();
                        UpdateTotal();
                        txtInfo.Clear();
                        if (txtPhone != null) txtPhone.Clear();
                        if (txtAddress != null) txtAddress.Clear();
                        if (txtPaid != null) txtPaid.Clear();
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
