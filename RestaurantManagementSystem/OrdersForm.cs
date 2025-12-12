using System;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class OrdersForm : Form
    {
        private DataGridView dgvOrders;
        private DataGridView dgvDetails;
        private Label lblTitle;
        private Label lblDetailsTitle;

        public OrdersForm()
        {
            this.Text = "Orders";
            this.Size = new Size(900, 600);
            this.StartPosition = FormStartPosition.CenterParent;

            InitializeLayout();
            LoadOrders();
        }

        private void InitializeLayout()
        {
            this.Dock = DockStyle.Fill;
            this.BackColor = Color.White;

            lblTitle = new Label()
            {
                Text = "Orders History",
                Dock = DockStyle.Fill,
                ForeColor = Color.DarkSlateBlue,
                Font = new Font("Segoe UI", 16, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                BackColor = Color.White
            };
            SplitContainer split = new SplitContainer();
            split.Dock = DockStyle.Fill;
            split.Orientation = Orientation.Horizontal;
            split.SplitterDistance = 260;
            split.BackColor = Color.White;
            split.Panel1.Padding = new Padding(10);
            split.Panel2.Padding = new Padding(10);
            this.Controls.Add(split);

            dgvOrders = new DataGridView();
            dgvOrders.Dock = DockStyle.Fill;
            dgvOrders.BackgroundColor = Color.White;
            dgvOrders.BorderStyle = BorderStyle.None;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.ReadOnly = true;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvOrders.MultiSelect = false;
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrders.CellClick += (s, e) => OnOrderSelected();
            dgvOrders.SelectionChanged += (s, e) => OnOrderSelected();
            split.Panel1.Controls.Add(dgvOrders);

            // Orders header inside the top panel; add AFTER the Fill control
            // so it is processed first in dock layout (prevents overlap).
            Panel pnlOrdersHeader = new Panel();
            pnlOrdersHeader.Dock = DockStyle.Top;
            pnlOrdersHeader.Height = 45;
            pnlOrdersHeader.BackColor = Color.White;
            pnlOrdersHeader.Controls.Add(lblTitle);
            split.Panel1.Controls.Add(pnlOrdersHeader);

            dgvDetails = new DataGridView();
            dgvDetails.Dock = DockStyle.Fill;
            dgvDetails.BackgroundColor = Color.White;
            dgvDetails.BorderStyle = BorderStyle.None;
            dgvDetails.RowHeadersVisible = false;
            dgvDetails.AllowUserToAddRows = false;
            dgvDetails.ReadOnly = true;
            dgvDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            split.Panel2.Controls.Add(dgvDetails);

            lblDetailsTitle = new Label()
            {
                Text = "Order Items",
                Dock = DockStyle.Top,
                Height = 30,
                ForeColor = Color.Black,
                Font = new Font("Segoe UI", 11, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            split.Panel2.Controls.Add(lblDetailsTitle);
        }

        private void LoadOrders()
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();
                DataTable dt = new DataTable();

                string sqlWithAddress = @"
                    SELECT 
                        order_id AS 'Order ID',
                        order_date AS 'Date',
                        order_type AS 'Type',
                        COALESCE(table_number,'') AS 'Table',
                        COALESCE(customer_name,'') AS 'Customer',
                        COALESCE(customer_phone,'') AS 'Phone',
                        COALESCE(customer_address,'') AS 'Address',
                        total_amount AS 'Total'
                    FROM Orders
                    ORDER BY order_date DESC;";

                try
                {
                    using (var adapter = new SQLiteDataAdapter(sqlWithAddress, conn))
                    {
                        adapter.Fill(dt);
                    }
                }
                catch (SQLiteException ex) when (ex.Message.IndexOf("customer_address", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    dt.Clear();
                    string sqlFallback = @"
                        SELECT 
                            order_id AS 'Order ID',
                            order_date AS 'Date',
                            order_type AS 'Type',
                            COALESCE(table_number,'') AS 'Table',
                            COALESCE(customer_name,'') AS 'Customer',
                            COALESCE(customer_phone,'') AS 'Phone',
                            total_amount AS 'Total'
                        FROM Orders
                        ORDER BY order_date DESC;";

                    using (var adapter = new SQLiteDataAdapter(sqlFallback, conn))
                    {
                        adapter.Fill(dt);
                    }
                }

                dgvOrders.DataSource = dt;

                if (dgvOrders.Columns.Contains("Total"))
                {
                    dgvOrders.Columns["Total"].DefaultCellStyle.Format = "N2";
                }
                if (dgvOrders.Columns.Contains("Date"))
                {
                    dgvOrders.Columns["Date"].DefaultCellStyle.Format = "g";
                }
            }

            if (dgvOrders.Rows.Count > 0)
            {
                dgvOrders.Rows[0].Selected = true;
                OnOrderSelected();
            }
        }

        private void OnOrderSelected()
        {
            if (dgvOrders.CurrentRow == null) return;
            if (!dgvOrders.Columns.Contains("Order ID")) return;

            if (!int.TryParse(dgvOrders.CurrentRow.Cells["Order ID"].Value?.ToString(), out int orderId))
            {
                return;
            }

            LoadOrderDetails(orderId);
        }

        private void LoadOrderDetails(int orderId)
        {
            using (var conn = new SQLiteConnection(DatabaseHelper.GetConnectionString()))
            {
                conn.Open();

                string sql = @"
                    SELECT 
                        d.menu_item_id AS 'Item ID',
                        m.name AS 'Item',
                        d.quantity AS 'Qty',
                        d.price_per_item AS 'Price',
                        d.total_price AS 'Total'
                    FROM OrderDetails d
                    JOIN MenuItems m ON m.menu_item_id = d.menu_item_id
                    WHERE d.order_id = @oid;";

                using (var cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@oid", orderId);
                    using (var adapter = new SQLiteDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvDetails.DataSource = dt;
                    }
                }

                if (dgvDetails.Columns.Contains("Item ID"))
                {
                    dgvDetails.Columns["Item ID"].Visible = false;
                }
                if (dgvDetails.Columns.Contains("Price"))
                {
                    dgvDetails.Columns["Price"].DefaultCellStyle.Format = "N2";
                }
                if (dgvDetails.Columns.Contains("Total"))
                {
                    dgvDetails.Columns["Total"].DefaultCellStyle.Format = "N2";
                }
            }
        }
    }
}
