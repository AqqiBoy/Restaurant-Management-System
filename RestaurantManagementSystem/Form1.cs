using System;
using System.Drawing;
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class Form1 : Form
    {
        // Layout Panels
        private Panel panelSidebar;
        private Panel panelHeader;
        private Panel panelContent;
        private Panel panelLogo; // Top-left corner box

        // To track the current form open inside the panel
        private Form activeForm = null;

        public Form1()
        {
            // Main Window Settings
            this.Text = "Restaurant Management System";
            this.WindowState = FormWindowState.Maximized; // Full Screen
            this.FormBorderStyle = FormBorderStyle.None;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.White;

            // Build the Layout
            InitializeDashboardLayout();
        }

        private void InitializeDashboardLayout()
        {
            panelSidebar = new Panel();
            panelSidebar.Dock = DockStyle.Left;
            panelSidebar.Width = 260; // Narrow, as requested
            panelSidebar.BackColor = Color.WhiteSmoke;
            this.Controls.Add(panelSidebar);

            // 2. Logo Panel (Top Left inside Sidebar)
            panelLogo = new Panel();
            panelLogo.Dock = DockStyle.Top;
            panelLogo.Height = 100;
            panelLogo.BackColor = Color.WhiteSmoke;
            // Add a Label for App Name
            Label lblName = new Label();
            lblName.Text = "RESTAURANT\nMANAGER"; // \n for new line
            lblName.ForeColor = Color.Black;
            lblName.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblName.TextAlign = ContentAlignment.MiddleCenter;
            lblName.Dock = DockStyle.Fill;
            panelLogo.Controls.Add(lblName);
            panelSidebar.Controls.Add(panelLogo);

            // 3. Header Panel (Top) - Light/Color Theme
            panelHeader = new Panel();
            panelHeader.Dock = DockStyle.Top;
            panelHeader.Height = 80;
            panelHeader.BackColor = Color.WhiteSmoke;
            this.Controls.Add(panelHeader);

            // 4. Content Panel (The middle empty space)
            panelContent = new Panel();
            panelContent.Dock = DockStyle.Fill;
            panelContent.BackColor = Color.White;
            // Background Image or Welcome Text could go here
            Label lblWelcome = new Label();
            lblWelcome.Text = "Welcome to Dashboard";
            lblWelcome.Font = new Font("Segoe UI", 24, FontStyle.Regular);
            lblWelcome.ForeColor = Color.LightGray;
            lblWelcome.AutoSize = true;
            lblWelcome.Location = new Point(50, 50);
            panelContent.Controls.Add(lblWelcome);

            this.Controls.Add(panelContent);
            panelContent.BringToFront(); // Ensure it sits correctly between side and top

            // --- ADD BUTTONS ---
            AddSidebarButtons();
            AddHeaderButtons();
        }

        // --- SECTION A: SIDEBAR BUTTONS (Setup & Admin) ---
        private void AddSidebarButtons()
        {
            
            // 1. Units
            CreateSidebarButton("Units Setup", (s, e) => OpenChildForm(new UnitsForm()));
            // 2. Ingredients
            CreateSidebarButton("Ingredients List", (s, e) => OpenChildForm(new IngredientsForm()));
            // 3. Purchases
            CreateSidebarButton("Record Purchase", (s, e) => OpenChildForm(new PurchasesForm()));
            // 4. Production
            CreateSidebarButton("Kitchen Production", (s, e) => OpenChildForm(new ProductionForm()));
            // 5. Categories
            CreateSidebarButton("Food Categories", (s, e) => OpenChildForm(new CategoriesForm()));
            // 6. Menu Items
            CreateSidebarButton("Menu Items", (s, e) => OpenChildForm(new MenuForm()));
            // 7. Recipes
            CreateSidebarButton("Manage Recipes", (s, e) => OpenChildForm(new RecipesForm()));
            // 8. Wastage
            CreateSidebarButton("Stock Wastage", (s, e) => OpenChildForm(new WastageForm()));

            // Label for Section
            Label lblSetup = new Label() { Text = "  BACK OFFICE SETUP", ForeColor = Color.Gray, Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.BottomLeft, Font = new Font("Segoe UI", 8, FontStyle.Bold) };
            panelSidebar.Controls.Add(lblSetup);
        }

        // --- SECTION B: HEADER BUTTONS (Operations) ---
        private void AddHeaderButtons()
        {
            // We flow these from Left to Right
            int btnWidth = 120;
            int btnHeight = 60;
            int spacing = 10;
            int startX = 20;
            int startY = 10;

            // 1. Dashboard (Home)
            CreateHeaderButton("Dashboard", Color.DarkSlateBlue, startX, startY, (s, e) => {
                if (activeForm != null) activeForm.Close(); // Close any open form
            });

            // 2. Dine In (POS)
            startX += btnWidth + spacing;
            CreateHeaderButton("Dine In", Color.Crimson, startX, startY, (s, e) => MessageBox.Show("Dine In Module Coming Soon"));

            // 3. Take Away
            startX += btnWidth + spacing;
            CreateHeaderButton("Take Away", Color.Orange, startX, startY, (s, e) => MessageBox.Show("Take Away Module Coming Soon"));

            // 4. Delivery
            startX += btnWidth + spacing;
            CreateHeaderButton("Delivery", Color.Teal, startX, startY, (s, e) => MessageBox.Show("Delivery Module Coming Soon"));

            // 5. Reports
            startX += btnWidth + spacing;
            CreateHeaderButton("Reports", Color.SeaGreen, startX, startY, (s, e) => MessageBox.Show("Reports Coming Soon"));

            // Exit Button (Far Right)
            Button btnExit = new Button();
            btnExit.Text = "X";
            btnExit.Size = new Size(40, 40);
            btnExit.Location = new Point(Screen.PrimaryScreen.Bounds.Width - 320, 20); // Anchor right
            btnExit.FlatStyle = FlatStyle.Flat;
            btnExit.FlatAppearance.BorderSize = 0;
            btnExit.BackColor = Color.IndianRed;
            btnExit.ForeColor = Color.White;
            btnExit.Click += (s, e) => Application.Exit();
            panelHeader.Controls.Add(btnExit);
        }

        // --- HELPER: Sidebar Button Creator ---
        private void CreateSidebarButton(string text, EventHandler action)
        {
            Button btn = new Button();
            btn.Text = "  " + text;
            btn.Dock = DockStyle.Top;
            btn.Height = 45;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;

            // 1. CHANGE: Match the sidebar background (WhiteSmoke)
            btn.BackColor = Color.WhiteSmoke;

            // 2. CHANGE: Text color should be Dark (Black/DarkGray) to be visible
            btn.ForeColor = Color.FromArgb(64, 64, 64);

            btn.TextAlign = ContentAlignment.MiddleLeft;
            btn.Font = new Font("Segoe UI", 15, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            // 3. CHANGE: Update Hover Effects for Light Theme
            btn.MouseEnter += (s, e) => {
                btn.BackColor = Color.Gainsboro; // Light Gray when hovering
                btn.ForeColor = Color.Black;
            };

            btn.MouseLeave += (s, e) => {
                btn.BackColor = Color.WhiteSmoke; // Back to normal
                btn.ForeColor = Color.FromArgb(64, 64, 64);
            };

            btn.Click += action;
            panelSidebar.Controls.Add(btn);
            panelSidebar.Controls.SetChildIndex(btn, 0);
        }

        // --- HELPER: Header Button Creator ---
        private void CreateHeaderButton(string text, Color color, int x, int y, EventHandler action)
        {
            Button btn = new Button();
            btn.Text = text;
            btn.Location = new Point(x, y);
            btn.Size = new Size(120, 60);
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.BackColor = color;
            btn.ForeColor = Color.White;
            btn.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btn.Cursor = Cursors.Hand;

            // Rounded corners logic is complex in pure code, keeping it square/flat for now
            // or we could draw it, but flat is cleaner for this stage.

            btn.Click += action;
            panelHeader.Controls.Add(btn);
        }

        // --- CORE LOGIC: Loading Forms inside the Panel ---
        private void OpenChildForm(Form childForm)
        {
            // 1. Close the current form if open
            if (activeForm != null)
                activeForm.Close();

            // 2. Setup the new form
            activeForm = childForm;
            childForm.TopLevel = false; // Important: Make it behave like a control
            childForm.FormBorderStyle = FormBorderStyle.None; // Remove border/title bar
            childForm.Dock = DockStyle.Fill; // Fill the empty space

            // 3. Add to panel and show
            panelContent.Controls.Add(childForm);
            panelContent.Tag = childForm;
            childForm.BringToFront();
            childForm.Show();
        }
    }
}