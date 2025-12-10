
using System;
using System.Data.SQLite;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class Form1 : Form
    {
        private Panel panelTop;
        private Panel panelMainArea;
        private Panel panelSide;
        private Panel panelContent;

        private Button btnDashboard;
        private Button btnDineIn;
        private Button btnTakeaway;
        private Button btnReport;
        private Button btnCloseApp;

        private Form activeForm = null;
        private Button selectedButton = null;
        private Button selectedTopButton = null;

        private readonly Color AccentColor = Color.FromArgb(0xF0, 0x80, 0x80); // Light Coral
        private readonly Color HoverColor = Color.FromArgb(0xF6, 0x72, 0x80);  // Pastel Red
        private readonly Color SelectedColor = ColorTranslator.FromHtml("#FEA3AA"); // Dusky Pink

        // Dashboard labels as fields
        private Label lblMainTitle;
        private Label lblSubtitle;

        public Form1()
        {
            this.Text = "Restaurant Management System";
            this.WindowState = FormWindowState.Maximized;
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.WhiteSmoke;

            InitializeLayout();
        }

        private void InitializeLayout()
        {
            // ---------------- TOP PANEL ----------------
            panelTop = new Panel();
            panelTop.Dock = DockStyle.Top;
            panelTop.Height = 60;
            panelTop.BackColor = HoverColor;
            this.Controls.Add(panelTop);

            // Thin black line below top panel
            Panel topSeparator = new Panel();
            topSeparator.Height = 1;
            topSeparator.Dock = DockStyle.Top;
            topSeparator.BackColor = Color.Black;
            this.Controls.Add(topSeparator);
            topSeparator.BringToFront();

            // ---------------- MAIN AREA ----------------
            panelMainArea = new Panel();
            panelMainArea.Dock = DockStyle.Fill;
            panelMainArea.BackColor = Color.WhiteSmoke;
            this.Controls.Add(panelMainArea);

            // ---------------- SIDE PANEL ----------------
            panelSide = new Panel();
            panelSide.Width = 240;
            panelSide.Dock = DockStyle.Left;
            panelSide.BackColor = AccentColor;
            panelMainArea.Controls.Add(panelSide);

            // Spacer at top
            Panel spacer = new Panel();
            spacer.Height = 70;
            spacer.Dock = DockStyle.Top;
            spacer.BackColor = AccentColor;
            panelSide.Controls.Add(spacer);

            // ---------------- CONTENT AREA ----------------
            panelContent = new Panel();
            panelContent.Dock = DockStyle.Fill;
            panelContent.BackColor = Color.WhiteSmoke;
            panelMainArea.Controls.Add(panelContent);

            // ===== Dashboard Labels =====
            lblMainTitle = new Label();
            lblMainTitle.Text = "ABC RESTAURANT";
            lblMainTitle.AutoSize = true;
            lblMainTitle.Font = new Font("Segoe UI", 40, FontStyle.Bold);
            lblMainTitle.ForeColor = Color.FromArgb(255, 100, 100);

            lblSubtitle = new Label();
            lblSubtitle.Text = "BBQ AND FAST FOOD";
            lblSubtitle.AutoSize = true;
            lblSubtitle.Font = new Font("Segoe UI", 28, FontStyle.Italic);
            lblSubtitle.ForeColor = Color.FromArgb(80, 80, 80);

            panelContent.Controls.Add(lblMainTitle);
            panelContent.Controls.Add(lblSubtitle);

            CenterDashboardLabels();
            panelContent.Resize += (s, e) => CenterDashboardLabels();

            // Load Top Buttons
            SetupTopButtons();

            // Load default Side Buttons (Dashboard)
            UpdateSideButtons("Dashboard");
        }


        private void CenterDashboardLabels()
        {
            // Force labels to update their size
            lblMainTitle.AutoSize = true;
            lblSubtitle.AutoSize = true;
            lblMainTitle.PerformLayout();
            lblSubtitle.PerformLayout();

            int totalHeight = lblMainTitle.Height + lblSubtitle.Height + 10;
            int startY = (panelContent.Height - totalHeight) / 2;

            // Fully centered horizontally
            lblMainTitle.Location = new Point(
                (panelContent.Width - lblMainTitle.PreferredWidth) / 2,
                startY
            );

            lblSubtitle.Location = new Point(
                (panelContent.Width - lblSubtitle.PreferredWidth) / 2,
                lblMainTitle.Bottom + 10
            );
        }



        // ---------------- TOP BUTTONS ----------------
        private void SetupTopButtons()
        {
            int x = 0;

            // Dashboard
            btnDashboard = CreateTopButton("Dashboard", ref x);
            AddTopButtonHoverEffect(btnDashboard, "Dashboard");
            panelTop.Controls.Add(btnDashboard);

            // Dine In
            btnDineIn = CreateTopButton("Dine In", ref x);
            AddTopButtonHoverEffect(btnDineIn, "Dine In");
            panelTop.Controls.Add(btnDineIn);

            // Takeaway
            btnTakeaway = CreateTopButton("Takeaway", ref x);
            AddTopButtonHoverEffect(btnTakeaway, "Takeaway");
            panelTop.Controls.Add(btnTakeaway);

            // Delivery
            Button btnDelivery = CreateTopButton("Delivery", ref x);
            AddTopButtonHoverEffect(btnDelivery, "Delivery");
            panelTop.Controls.Add(btnDelivery);

            // Report
            btnReport = CreateTopButton("Report", ref x);
            AddTopButtonHoverEffect(btnReport, "Report");
            panelTop.Controls.Add(btnReport);

            // Close button
            btnCloseApp = new Button();
            btnCloseApp.Text = "X";
            btnCloseApp.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            btnCloseApp.Dock = DockStyle.Right;
            btnCloseApp.Width = 60;
            btnCloseApp.FlatStyle = FlatStyle.Flat;
            btnCloseApp.FlatAppearance.BorderSize = 0;
            btnCloseApp.BackColor = HoverColor;
            btnCloseApp.ForeColor = Color.Black;
            btnCloseApp.Click += (s, e) => Application.Exit();
            btnCloseApp.MouseEnter += (s, e) => { btnCloseApp.BackColor = Color.DarkRed; };
            btnCloseApp.MouseLeave += (s, e) => { btnCloseApp.BackColor = AccentColor; };
            panelTop.Controls.Add(btnCloseApp);
        }

        private void AddTopButtonHoverEffect(Button btn, string buttonName)
        {
            btn.MouseEnter += (s, e) =>
            {
                if (btn != selectedTopButton)
                    btn.BackColor = HoverColor;
            };
            btn.MouseLeave += (s, e) =>
            {
                if (btn != selectedTopButton)
                    btn.BackColor = panelTop.BackColor;
            };
            btn.Click += (s, e) =>
            {
                if (selectedTopButton != null && selectedTopButton != btn)
                    selectedTopButton.BackColor = panelTop.BackColor;

                selectedTopButton = btn;
                selectedTopButton.BackColor = SelectedColor;

                // Update sidebar according to top button
                UpdateSideButtons(buttonName);

                // Clear content panel first
                panelContent.Controls.Clear();

                if (buttonName == "Dashboard")
                {
                    panelContent.Controls.Add(lblMainTitle);
                    panelContent.Controls.Add(lblSubtitle);
                    CenterDashboardLabels();
                }
                else if (buttonName == "Dine In" || buttonName == "Takeaway" || buttonName == "Delivery")
                {
                    LoadMenuItemsUI(); // show menu items UI
                }

            };
        }

        private Button CreateTopButton(string text, ref int x)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btn.Size = new Size(120, 60);
            btn.Location = new Point(x, 0);
            btn.BackColor = panelTop != null ? panelTop.BackColor : Color.White;
            btn.ForeColor = Color.Black;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            x += 120;
            return btn;
        }

        // ---------------- SIDEBAR BUTTONS ----------------
        private void UpdateSideButtons(string topButtonName)
        {
            panelSide.Controls.Clear();

            // Add top spacer
            Panel spacer = new Panel();
            spacer.Height = 70;
            spacer.Dock = DockStyle.Top;
            spacer.BackColor = AccentColor;
            panelSide.Controls.Add(spacer);

            // If Dine In / Takeaway / Delivery → Only show Categories
            if (topButtonName == "Dine In" || topButtonName == "Takeaway" || topButtonName == "Delivery")
            {
                LoadCategoriesIntoSidePanel();
            }
            else
            {
                panelSide.Controls.Add(CreateSideButton("UNIT", typeof(UnitsForm)));
                panelSide.Controls.Add(CreateSideButton("INGREDIENTS", typeof(IngredientsForm)));
                panelSide.Controls.Add(CreateSideButton("PURCHASES", typeof(PurchasesForm)));
                panelSide.Controls.Add(CreateSideButton("CATEGORIES", typeof(CategoriesForm)));
                panelSide.Controls.Add(CreateSideButton("MENU ITEMS", typeof(MenuForm)));
                panelSide.Controls.Add(CreateSideButton("RECIPES", typeof(RecipesForm)));
                panelSide.Controls.Add(CreateSideButton("KITCHEN PRODUCTION", typeof(ProductionForm)));
                panelSide.Controls.Add(CreateSideButton("STOCK ADJUSTMENT", typeof(WastageForm)));
            }

            // Bring to front
            foreach (Control ctrl in panelSide.Controls)
                ctrl.BringToFront();
        }

        private void LoadCategoriesIntoSidePanel()
        {
            panelSide.Controls.Clear();

            Panel spacer = new Panel();
            spacer.Height = 70;
            spacer.Dock = DockStyle.Top;
            spacer.BackColor = AccentColor;
            panelSide.Controls.Add(spacer);

            using (var conn = new SQLiteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                string query = "SELECT category_id, name FROM Categories";

                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int catId = Convert.ToInt32(reader["category_id"]);
                        string catName = reader["name"].ToString();

                        Panel container = new Panel();
                        container.Height = 55;
                        container.Dock = DockStyle.Top;

                        Button btnCategory = new Button();
                        btnCategory.Text = catName.ToUpper();
                        btnCategory.Tag = catId;
                        btnCategory.Dock = DockStyle.Fill;
                        btnCategory.TextAlign = ContentAlignment.MiddleLeft;
                        btnCategory.Padding = new Padding(15, 0, 0, 0);
                        btnCategory.BackColor = AccentColor;
                        btnCategory.ForeColor = Color.Black;
                        btnCategory.FlatStyle = FlatStyle.Flat;
                        btnCategory.FlatAppearance.BorderSize = 0;

                        btnCategory.MouseEnter += (s, e) => { btnCategory.BackColor = HoverColor; };
                        btnCategory.MouseLeave += (s, e) => { btnCategory.BackColor = AccentColor; };

                        btnCategory.Click += (s, e) =>
                        {
                            foreach (Control ctrl in panelSide.Controls)
                            {
                                if (ctrl is Panel p && p.Controls.Count > 0 && p.Controls[0] is Button b)
                                    b.BackColor = AccentColor;
                            }
                            btnCategory.BackColor = SelectedColor;
                        };

                        Panel line = new Panel();
                        line.Height = 1;
                        line.Dock = DockStyle.Bottom;
                        line.BackColor = Color.Black;

                        container.Controls.Add(btnCategory);
                        container.Controls.Add(line);

                        panelSide.Controls.Add(container);
                        container.BringToFront();
                    }
                }
            }
        }
        private void LoadMenuItemsUI()
        {
            panelContent.Controls.Clear();
            panelContent.AutoScroll = true;

            // --- New Square/Grid Layout Parameters ---
            int cardSize = 150; // Use a single variable for both width and height to make it square
            int panelWidth = cardSize;
            int panelHeight = cardSize;
            int spacing = 15; // Space between cards
            int leftMargin = 228; // Margin from the left edge of panelContent
            int topMargin = 100; // Margin from the top edge of panelContent

            int x = leftMargin; // Current X-coordinate for placement
            int y = topMargin; // Current Y-coordinate for placement

            // Calculate how many cards can fit horizontally (columns)
            int columns = (panelContent.Width - 2 * leftMargin) / (cardSize + spacing);
            if (columns < 1) columns = 1; // Ensure at least one column
            int cardCount = 0; // Counter for determining X-coordinate

            // Calculate the centered starting X-position for the grid to look good
            // This centers the *entire grid* within panelContent.
            int totalGridWidth = columns * cardSize + (columns - 1) * spacing;
            int centeredStartX = (panelContent.Width - totalGridWidth) / 2;
            if (centeredStartX < leftMargin) centeredStartX = leftMargin; // Fallback

            x = centeredStartX; // Initialize x with the centered starting point

            // --- Database Operations remain the same ---
            using (var conn = new SQLiteConnection(DatabaseHelper.ConnectionString))
            {
                conn.Open();
                string query = "SELECT menu_item_id, name, price FROM MenuItems";

                using (var cmd = new SQLiteCommand(query, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int menuId = Convert.ToInt32(reader["menu_item_id"]);
                        string name = reader["name"].ToString();
                        decimal price = Convert.ToDecimal(reader["price"]);

                        // --- Panel for each menu item (now a square "card") ---
                        Panel itemPanel = new Panel();
                        itemPanel.Size = new Size(panelWidth, panelHeight);

                        // --- Placement Logic (Grid) ---
                        itemPanel.Location = new Point(x, y);
                        itemPanel.BorderStyle = BorderStyle.FixedSingle;
                        itemPanel.BackColor = Color.White;
                        itemPanel.Anchor = AnchorStyles.Top; // Anchor remains Top

                        // --- Name Label (Centered/Top) ---
                        Label lblName = new Label();
                        lblName.Text = name;
                        lblName.Location = new Point(10, 10);
                        lblName.MaximumSize = new Size(panelWidth - 20, 0); // Constrain width
                        lblName.AutoSize = true;
                        lblName.Font = new Font("Segoe UI", 11, FontStyle.Bold);

                        // --- Price Label (Centered/Middle) ---
                        Label lblPrice = new Label();
                        lblPrice.Text = $"{price} Rs";
                        // Center price horizontally and place it around the middle vertically
                        lblPrice.Location = new Point(
                            (panelWidth - lblPrice.PreferredWidth) / 2, // Center horizontally
                            (panelHeight / 2) - 10 // Center vertically (approx)
                        );
                        lblPrice.AutoSize = true;
                        lblPrice.Font = new Font("Segoe UI", 12, FontStyle.Regular);

                        // --- Add Button (Bottom) ---
                        Button btnAdd = new Button();
                        btnAdd.Text = "Add";
                        btnAdd.Size = new Size(panelWidth - 30, 30); // Width is almost full panel width
                                                                     // Place button near the bottom
                        btnAdd.Location = new Point(
                            15, // A small margin from the left
                            panelHeight - btnAdd.Height - 15 // Distance from the bottom
                        );
                        btnAdd.BackColor = Color.LightGreen;
                        btnAdd.FlatStyle = FlatStyle.Flat;
                        btnAdd.FlatAppearance.BorderSize = 0;
                        btnAdd.Tag = menuId;

                        // You would typically attach an event handler here:
                        // btnAdd.Click += BtnAdd_Click; 

                        // Add controls to panel
                        itemPanel.Controls.Add(lblName);
                        itemPanel.Controls.Add(lblPrice);
                        itemPanel.Controls.Add(btnAdd);

                        // Add panel to content
                        panelContent.Controls.Add(itemPanel);

                        // --- Update Coordinates for Next Card ---
                        cardCount++;

                        // Check if we need to start a new row
                        if (cardCount % columns == 0)
                        {
                            // Start a new row: Reset X and increase Y
                            x = centeredStartX;
                            y += panelHeight + spacing;
                        }
                        else
                        {
                            // Continue on the current row: Increase X
                            x += panelWidth + spacing;
                        }
                    }
                }
            }
        }

        private Panel CreateSideButton(string text, Type childFormType)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Dock = DockStyle.Fill;
            btn.Height = 55;
            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Padding = new Padding(12, 0, 0, 0);
            btn.BackColor = AccentColor;
            btn.ForeColor = Color.Black;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            btn.MouseEnter += (s, e) => { if (btn != selectedButton) btn.BackColor = HoverColor; };
            btn.MouseLeave += (s, e) => { if (btn != selectedButton) btn.BackColor = AccentColor; };

            btn.Click += (s, e) =>
            {
                if (selectedButton != null && selectedButton != btn)
                    selectedButton.BackColor = AccentColor;

                selectedButton = btn;
                selectedButton.BackColor = SelectedColor;

                Form newChildForm = (Form)Activator.CreateInstance(childFormType);
                OpenChildForm(newChildForm);
            };

            Panel container = new Panel();
            container.Height = 55;
            container.Dock = DockStyle.Top;
            container.Controls.Add(btn);

            Panel line = new Panel();
            line.Height = 1;
            line.Dock = DockStyle.Bottom;
            line.BackColor = Color.Black;
            container.Controls.Add(line);

            return container;
        }

        private void OpenChildForm(Form childForm)
        {
            if (activeForm != null)
                activeForm.Close();

            activeForm = childForm;
            childForm.TopLevel = false;
            childForm.FormBorderStyle = FormBorderStyle.None;
            childForm.AutoSize = true;
            childForm.Anchor = AnchorStyles.None;

            panelContent.Controls.Clear();
            panelContent.Controls.Add(childForm);

            childForm.Show();
            CenterChildForm();

            panelContent.Resize += (s, e) => CenterChildForm();
        }




        private void CenterChildForm()
        {
            if (activeForm != null)
            {
                activeForm.Location = new Point(
                    (panelContent.Width - activeForm.Width) / 2,
                    (panelContent.Height - activeForm.Height) / 2
                );
            }
        }
    




// Resize event handler
private void PanelContent_Resize(object sender, EventArgs e)
        {
            CenterChildForm();
        }




        private void Form1_Load(object sender, EventArgs e)
        {
        }

        private void Form1_Load_1(object sender, EventArgs e)
        {

        }
    }
}