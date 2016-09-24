namespace Finanse.DataAccessLayer {
    using Elements;
    using SQLite.Net;
    using SQLite.Net.Platform.WinRT;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.ApplicationModel;
    using Windows.Storage;

    internal static class Dal {
        private static string dbPath = string.Empty;
        private static string DbPath {
            get {
                if (string.IsNullOrEmpty(dbPath)) {
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");
                }

                return dbPath;
            }
        }

        private static SQLiteConnection DbConnection {
            get {
                return new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
            }
        }

        public static void CreateDB() {
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                db.CreateTable<MoneyAccount>();
                db.CreateTable<Operation>();
                db.CreateTable<OperationPattern>();
                db.CreateTable<OperationCategory>();
                db.CreateTable<OperationSubCategory>();
                db.CreateTable<Settings>();

                if (!db.Table<Settings>().Any())
                    db.Insert(new Settings {
                        CultureInfoName = "en-US"
                    });

                if (!db.Table<MoneyAccount>().Any()) {
                    db.Insert(new MoneyAccount {
                        Name = "Gotówka"
                    });
                    db.Insert(new MoneyAccount {
                        Name = "Karta VISA"
                    });
                }
            }
        }
        public static async Task CreateDatabase() {
            // Create a new connection
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Create the table if it does not exist
                var c = db.CreateTable<Operation>();
                var info = db.GetMapping(typeof(Operation));

            }
        }

        public static void DeletePattern(OperationPattern person) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM OperationPattern WHERE Id = ?", person.Id);
            }
        }

        public static void DeleteOperation(Operation person) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM Operation WHERE Id = ?", person.Id);
            }
        }

        public static void DeletePerson(Operation person) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM Person WHERE Id = ?", person.Id);
            }
        }
        /*
        public static Settings GetSettings() {
            Settings settings;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                settings = db.Table<Settings>().ElementAt(0);
            }

            return settings;
        }
        */
        /* GET ALL */

        public static List<OperationCategory> GetAllCategories() {
            List<OperationCategory> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = (from p in db.Table<OperationCategory>()
                          orderby p.Name
                          select p).ToList();
            }

            return models;
        }

        public static List<Operation> GetAllPersons() {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = (from p in db.Table<Operation>()
                           select p).ToList();
            }

            return models;
        }

        public static List<OperationPattern> GetAllPatterns() {
            List<OperationPattern> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = (from p in db.Table<OperationPattern>()
                          select p).ToList();
            }

            return models;
        }

        public static List<MoneyAccount> GetAllMoneyAccounts() {
            List<MoneyAccount> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = (from p in db.Table<MoneyAccount>()
                          select p).ToList();
            }

            return models;
        }

        /* GET BY ID */
    
        public static Operation GetOperationById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                Operation m = (from p in db.Table<Operation>()
                               where p.Id == Id
                               select p).FirstOrDefault();
                return m;
            }
        }

        public static MoneyAccount GetMoneyAccountById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                MoneyAccount m = (from p in db.Table<MoneyAccount>()
                                          where p.Id == Id
                                          select p).FirstOrDefault();
                return m;
            }
        }

        public static OperationSubCategory GetOperationSubCategoryById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                OperationSubCategory m = (from p in db.Table<OperationSubCategory>()
                                       where p.OperationCategoryId == Id
                                       select p).FirstOrDefault();
                return m;
            }
        }

        public static List<OperationSubCategory> GetOperationSubCategoryByBossId(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                List<OperationSubCategory> m = (from p in db.Table<OperationSubCategory>()
                                                where p.BossCategoryId == Id
                                                orderby p.Name
                                                select p).ToList();
                return m;
            }
        }

        public static OperationCategory GetOperationCategoryById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                OperationCategory m = (from p in db.Table<OperationCategory>()
                               where p.Id == Id
                               select p).FirstOrDefault();
                return m;
            }
        }

        public static Operation GetPersonById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                Operation m = (from p in db.Table<Operation>()
                               where p.Id == Id
                               select p).FirstOrDefault();
                return m;
            }
        }

        /* SAVE */

        public static void SaveOperation(Operation operation) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (operation.Id == 0) {
                    // New
                    db.Insert(operation);
                }
                else {
                    // Update
                    db.Update(operation);
                }
            }
        }

        public static void SaveOperationPattern(OperationPattern operationPattern) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (operationPattern.Id == 0) {
                    // New
                    db.Insert(operationPattern);
                }
                else {
                    // Update
                    db.Update(operationPattern);
                }
            }
        }

        public static void SavePerson(Operation person) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (person.Id == 0) {
                    // New
                    db.Insert(person);
                }
                else {
                    // Update
                    db.Update(person);
                }
            }
        }
    }
}