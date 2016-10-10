namespace Finanse.DataAccessLayer {
    using Elements;
    using Models;
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
                /*
                List<OperationSubCategory> subCatList = new List<OperationSubCategory>();
                foreach (var item in db.Table<OperationSubCategory>()) {
                    subCatList.Add(new OperationSubCategory {
                        BossCategoryId = item.BossCategoryId,
                        Color = item.Color,
                        Icon = item.Icon,
                        Name = item.Name,
                        VisibleInExpenses = item.VisibleInExpenses,
                        VisibleInIncomes = item.VisibleInIncomes,
                    });
                }
                db.DropTable<OperationSubCategory>();
                db.CreateTable<OperationSubCategory>();
                foreach (var item in subCatList) {
                    db.Insert(item);
                }*/
/*
                db.DeleteAll<MoneyAccount>();
                db.Insert(new MoneyAccount {
                    Id = 1,
                    Name = "Gotówka",
                    Color = "#FFcfd8dc",
                });
                db.Insert(new MoneyAccount {
                    Id = 2,
                    Name = "Karta VISA",
                    Color = "#FF00e676",
                });
                */
                if (!db.Table<Settings>().Any())
                    db.Insert(new Settings {
                        CultureInfoName = "en-US"
                    });

                if (!db.Table<MoneyAccount>().Any()) {
                    db.Insert(new MoneyAccount {
                        Name = "Gotówka",
                        Color = "#FFcfd8dc"
                    });
                    db.Insert(new MoneyAccount {
                        Name = "Karta VISA",
                        Color = "#FF00e676",
                    });
                    db.Insert(new MoneyAccount {
                        Name = "Student (Karta VISA)",
                        Color = "#FFe04967"
                    });
                    db.Insert(new MoneyAccount {
                        Name = "Student (Gotówka)",
                        Color = "#FFe7c64a"
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

        public static Operation GetEldestOperation() {
            Operation eldest;

            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                eldest = db.Table<Operation>().Aggregate((c1, c2) => Convert.ToDateTime(c1.Date) < Convert.ToDateTime(c2.Date) ? c1 : c2);
            }

            return eldest;
        }

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

        public static List<Operation> GetAllOperations(int month, int year) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                string date = year.ToString() + "." + month.ToString("00");

                models = (from p in db.Table<Operation>().ToList()
                          where p.Date.Substring(0,7) == date.ToString() 
                             && Convert.ToDateTime(p.Date) <= DateTime.Today
                          select p).ToList();
            }

            return models;
        }

        public static List<Operation> GetAllOperations(int month, int year, List<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                string date = year.ToString() + "." + month.ToString("00");

                if (visiblePayFormList != null) {
                    models = (from p in db.Table<Operation>().ToList()
                              where p.Date.Substring(0, 7) == date.ToString()
                                 && Convert.ToDateTime(p.Date) <= DateTime.Today
                                 && visiblePayFormList.Any(iteme => iteme == p.MoneyAccountId) == true
                              select p).ToList();
                }
                else {
                    models = (from p in db.Table<Operation>().ToList()
                              where p.Date.Substring(0, 7) == date.ToString()
                                 && Convert.ToDateTime(p.Date) <= DateTime.Today
                              select p).ToList();
                }
            }

            return models;
        }

        public static List<Operation> GetAllFutureOperations() {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = (from p in db.Table<Operation>().ToList()
                          where Convert.ToDateTime(p.Date) > DateTime.Today
                          select p).ToList();
            }

            return models;
        }

        public static List<Operation> GetAllFutureOperations(List<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (visiblePayFormList != null) {
                    models = (from p in db.Table<Operation>().ToList()
                              where Convert.ToDateTime(p.Date) > DateTime.Today
                                 && visiblePayFormList.Any(iteme => iteme == p.MoneyAccountId) == true
                              select p).ToList();
                }
                else {
                    models = (from p in db.Table<Operation>().ToList()
                              where Convert.ToDateTime(p.Date) > DateTime.Today
                              select p).ToList();
                }
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
                                       where p.Id == Id
                                       select p).FirstOrDefault();

                return m;
            }
        }

        public static List<OperationSubCategory> GetOperationSubCategoriesByBossId(int Id) {
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

        public static void SaveOperationCategory(OperationCategory operationCategory) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (operationCategory.Id == 0) {
                    // New
                    db.Insert(operationCategory);
                }
                else {
                    // Update
                    db.Update(operationCategory);
                }
            }
        }

        public static void SaveOperationSubCategory(OperationSubCategory operationSubCategory) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (operationSubCategory.Id == 0) {
                    // New
                    db.Insert(operationSubCategory);
                }
                else {
                    // Update
                    db.Update(operationSubCategory);
                }
            }
        }
        /* DELETE */

        public static void DeletePattern(OperationPattern operationPattern) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM OperationPattern WHERE Id = ?", operationPattern.Id);
            }
        }

        public static void DeleteOperation(Operation operation) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM Operation WHERE Id = ?", operation.Id);
            }
        }

        public static void DeleteCategory(OperationCategory operationCategory) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM OperationCategory WHERE Id = ?", operationCategory.Id);
                db.Execute("DELETE FROM OperationSubCategory WHERE BossCategoryId = ?", operationCategory.Id);
            }
        }

        public static void DeleteSubCategory(OperationSubCategory operationSubCategory) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM OperationSubCategory WHERE Id = ?", operationSubCategory.Id);
            }
        }
    }
}