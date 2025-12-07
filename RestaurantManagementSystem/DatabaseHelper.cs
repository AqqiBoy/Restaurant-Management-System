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