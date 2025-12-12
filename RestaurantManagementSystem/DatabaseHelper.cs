using System;
using System.Data.SQLite; // The library we just installed
using System.IO;

namespace RestaurantManagementSystem
{
    public class DatabaseHelper
    {
        // The file name of your database. It will be created in your Debug folder.
        private const string DatabaseFileName = "restaurant_db.sqlite";
        private const string ConnectionString = "Data Source=" + DatabaseFileName + ";Version=3;";

        // Method to initialize the database (Create tables if they don't exist)
        public static void InitializeDatabase()
        {
            // Check if database file exists, if not, create it
            if (!File.Exists(DatabaseFileName))
            {
                SQLiteConnection.CreateFile(DatabaseFileName);
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                // 1. Create Units Table
                string sqlUnits = @"
                    CREATE TABLE IF NOT EXISTS Units (
                        unit_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        unit_name TEXT NOT NULL UNIQUE,
                        short_code TEXT NOT NULL,
                        allow_decimals INTEGER DEFAULT 1
                    );";
                ExecuteQuery(connection, sqlUnits);

                // 2. Categories Table (NEW!)
                string sqlCategories = @"
                    CREATE TABLE IF NOT EXISTS Categories (
                        category_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL UNIQUE
                    );";
                ExecuteQuery(connection, sqlCategories);

                // 3. Create Ingredients Table
                // Note: We removed reorder_level and last_cost as requested
                string sqlIngredients = @"
                    CREATE TABLE IF NOT EXISTS Ingredients (
                        ingredient_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL,
                        unit_id INTEGER NOT NULL,
                        current_stock REAL DEFAULT 0,
                        FOREIGN KEY (unit_id) REFERENCES Units(unit_id)
                    );";
                ExecuteQuery(connection, sqlIngredients);

                // 4. Create Purchases Table (Expenses)
                string sqlPurchases = @"
                    CREATE TABLE IF NOT EXISTS Purchases (
                        purchase_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ingredient_id INTEGER NOT NULL,
                        quantity REAL NOT NULL,
                        total_cost REAL NOT NULL,
                        purchase_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        description TEXT,
                        FOREIGN KEY (ingredient_id) REFERENCES Ingredients(ingredient_id)
                    );";
                ExecuteQuery(connection, sqlPurchases);

                // 5. Create MenuItems Table
                string sqlMenu = @"
                    CREATE TABLE IF NOT EXISTS MenuItems (
                        menu_item_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        name TEXT NOT NULL UNIQUE,
                        price REAL NOT NULL,
                        category_id INTEGER NOT NULL,
                        FOREIGN KEY(category_id) REFERENCES Categories(category_id)
                    );";
                ExecuteQuery(connection, sqlMenu);

                // 6. Create Recipes Table
                // This table links MenuItems to Ingredients
                string sqlRecipes = @"
                    CREATE TABLE IF NOT EXISTS Recipes (
                        recipe_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        menu_item_id INTEGER NOT NULL,
                        ingredient_id INTEGER NOT NULL,
                        quantity_required REAL NOT NULL,
                        FOREIGN KEY(menu_item_id) REFERENCES MenuItems(menu_item_id),
                        FOREIGN KEY(ingredient_id) REFERENCES Ingredients(ingredient_id)
                    );";
                ExecuteQuery(connection, sqlRecipes);

                // 7. Create ProductionLogs Table (For "Mince to Patty" conversion)
                string sqlProductionLogs = @"
                    CREATE TABLE IF NOT EXISTS ProductionLogs (
                        production_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        production_date DATETIME DEFAULT CURRENT_TIMESTAMP,
    
                        source_ingredient_id INTEGER NOT NULL,  -- What we used (e.g., Raw Mince)
                        used_quantity REAL NOT NULL,            -- How much we used (e.g., 5 kg)
    
                        target_ingredient_id INTEGER NOT NULL,  -- What we made (e.g., Frozen Patty)
                        produced_quantity REAL NOT NULL,        -- How much we got (e.g., 50 pcs)
    
                        FOREIGN KEY(source_ingredient_id) REFERENCES Ingredients(ingredient_id),
                        FOREIGN KEY(target_ingredient_id) REFERENCES Ingredients(ingredient_id)
                    );";
                ExecuteQuery(connection, sqlProductionLogs);

                // 8. Create StockAdjustments Table
                string sqlStockAdjustments = @"
                    CREATE TABLE IF NOT EXISTS StockAdjustments (
                        adjustment_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        ingredient_id INTEGER NOT NULL,
                        quantity_adjusted REAL NOT NULL,
                        reason TEXT,
                        adjustment_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        FOREIGN KEY(ingredient_id) REFERENCES Ingredients(ingredient_id)
                    );";
                ExecuteQuery(connection, sqlStockAdjustments);

                string sqlOrders = @"
                    CREATE TABLE IF NOT EXISTS Orders (
                        order_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        order_date DATETIME DEFAULT CURRENT_TIMESTAMP,
                        order_type TEXT,        -- 'DineIn', 'TakeAway', 'Delivery'
                        table_number TEXT,      -- For DineIn
                        customer_name TEXT,     -- For Delivery/TakeAway
                        customer_phone TEXT,    -- For Delivery/TakeAway
                        customer_address TEXT,  -- For Delivery
                        total_amount REAL DEFAULT 0
                    );";
                ExecuteQuery(connection, sqlOrders);

                string sqlOrderDetails = @"
                     CREATE TABLE IF NOT EXISTS OrderDetails (
                        detail_id INTEGER PRIMARY KEY AUTOINCREMENT,
                        order_id INTEGER NOT NULL,
                        menu_item_id INTEGER NOT NULL,
                        quantity INTEGER NOT NULL,
                        price_per_item REAL NOT NULL, -- Price at the moment of sale
                        total_price REAL NOT NULL,
                        FOREIGN KEY(order_id) REFERENCES Orders(order_id),
                        FOREIGN KEY(menu_item_id) REFERENCES MenuItems(menu_item_id)
                    );";
                ExecuteQuery(connection, sqlOrderDetails);
            }
        }

        // Helper method to execute SQL commands easily
        private static void ExecuteQuery(SQLiteConnection connection, string query)
        {
            using (var command = new SQLiteCommand(query, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        // Helper method to get the connection string (we will use this later in Forms)
        public static string GetConnectionString()
        {
            return ConnectionString;
        }
    }
}
