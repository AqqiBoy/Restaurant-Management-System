using System;
using System.Drawing; // For Size, Point, Color, Font
using System.Windows.Forms;

namespace RestaurantManagementSystem
{
    public partial class Form1 : Form
    {
        // Define controls
        private Label lblTitle;
        private Button btnUnits;
        private Button btnIngredients;
        private Button btnExit;
        private Button btnPurchases;
        private Button btnMenu;
        private Button btnCategories;

        public Form1()
        {
            // 1. Setup the Main Window properties
            this.Text = "Restaurant Management System";
            this.Size = new Size(600, 600);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.WhiteSmoke; // Light gray background

            // 2. Build the UI
            InitializeDashboard();
        }

        private void InitializeDashboard()
        {
            // --- Title Label ---
            lblTitle = new Label();
            lblTitle.Text = "Restaurant Dashboard";
            lblTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold); // Big bold text
            lblTitle.AutoSize = true;
            lblTitle.Location = new Point(147, 40); // Centered-ish
            lblTitle.ForeColor = Color.DarkSlateGray;
            this.Controls.Add(lblTitle);

            // --- Button 1: Manage Units ---
            btnUnits = new Button();
            btnUnits.Text = "Manage Units";
            btnUnits.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            btnUnits.Location = new Point(150, 120);
            btnUnits.Size = new Size(300, 50); // Wide button
            btnUnits.BackColor = Color.White;
            btnUnits.Click += new EventHandler(OpenUnitsForm); // Link to function below
            this.Controls.Add(btnUnits);

            // --- Button 2: Manage Categories ---
            Button btnCategories = new Button();
            btnCategories.Text = "Manage Categories";
            btnCategories.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            btnCategories.Location = new Point(150, 330); // Put this below Menu Button
            btnCategories.Size = new Size(300, 50);
            btnCategories.BackColor = Color.White;
            btnCategories.Click += new EventHandler(OpenCategoriesForm);
            this.Controls.Add(btnCategories);

            // --- Button 3: Manage Ingredients (Placeholder for now) ---
            // We will enable this fully when you paste the IngredientsForm code later
            btnIngredients = new Button();
            btnIngredients.Text = "Manage Ingredients";
            btnIngredients.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            btnIngredients.Location = new Point(150, 190); // 70px lower
            btnIngredients.Size = new Size(300, 50);
            btnIngredients.BackColor = Color.White;
            btnIngredients.Click += new EventHandler(OpenIngredientsForm);
            this.Controls.Add(btnIngredients);

            // --- Button 4: Record Purchase ---
            btnPurchases = new Button();
            btnPurchases.Text = "Record Expense (Purchase)";
            btnPurchases.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            btnPurchases.Location = new Point(150, 260); // Lower down
            btnPurchases.Size = new Size(300, 50);
            btnPurchases.BackColor = Color.White;
            btnPurchases.Click += new EventHandler(OpenPurchasesForm);
            this.Controls.Add(btnPurchases);

            // --- Button 5: Manage Menu ---
            btnMenu = new Button();
            btnMenu.Text = "Manage Menu Items";
            btnMenu.Font = new Font("Segoe UI", 12, FontStyle.Regular);
            btnMenu.Location = new Point(150, 400); // Lower down
            btnMenu.Size = new Size(300, 50);
            btnMenu.BackColor = Color.White;
            btnMenu.Click += new EventHandler(OpenMenuForm);
            this.Controls.Add(btnMenu);

            // --- Button 6: Exit ---
            btnExit = new Button();
            btnExit.Text = "Exit System";
            btnExit.Font = new Font("Segoe UI", 10, FontStyle.Regular);
            btnExit.Location = new Point(150, 470);
            btnExit.Size = new Size(300, 40);
            btnExit.BackColor = Color.IndianRed; // Red color for exit
            btnExit.ForeColor = Color.White;
            btnExit.Click += (s, e) => { Application.Exit(); };
            this.Controls.Add(btnExit);
        }

        // --- Event Handlers (Button Actions) ---

        private void OpenUnitsForm(object sender, EventArgs e)
        {
            // This creates the UnitsForm and opens it as a popup
            UnitsForm frm = new UnitsForm();
            frm.ShowDialog();
        }

        private void OpenCategoriesForm(object sender, EventArgs e)
        {
            // This creates the UnitsForm and opens it as a popup
            CategoriesForm frm = new CategoriesForm();
            frm.ShowDialog();
        }

        private void OpenIngredientsForm(object sender, EventArgs e)
        {
            // If you haven't created IngredientsForm yet, this might crash.
            // But since we are building it next, I'll leave the connection here.
            // If it shows an error "IngredientsForm could not be found", 
            // comment out the lines inside this bracket for now.

            IngredientsForm frm = new IngredientsForm();
            frm.ShowDialog();
        }

        private void OpenPurchasesForm(object sender, EventArgs e)
        {
            PurchasesForm frm = new PurchasesForm();
            frm.ShowDialog();
        }

        private void OpenMenuForm(object sender, EventArgs e)
        {
            MenuForm frm = new MenuForm();
            frm.ShowDialog();
        }
    }
}