namespace Finanse.DataAccessLayer {
    using Models;
    using Models.Categories;
    using Models.Helpers;
    using Models.MoneyAccounts;
    using SQLite.Net;
    using SQLite.Net.Platform.WinRT;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using Windows.Storage;

    internal static class Dal {
        private static string dbPath = string.Empty;
        private static string DbPath {
            get {
                if (string.IsNullOrEmpty(dbPath))
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");
                return dbPath;
            }
        }

        private static SQLiteConnection DbConnection {
            get {
                return new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
            }
        }

        public static void createDB() {
            using (var db = DbConnection) {
                // Activate Tracing
                db.Execute("PRAGMA foreign_keys = ON");
                db.TraceListener = new DebugTraceListener();

                /*
                db.Execute("CREATE TABLE IF NOT EXISTS images ( "
                    + "nameRed VARCHAR(20) NOT NULL PRIMARY KEY,"
                    + "patientID INT,"
                    + "FOREIGN KEY(patientID) REFERENCES patients(id) ) ");*/

               // db.CreateTable<MoneyAccount>();
                db.CreateTable<Operation>();
                db.CreateTable<OperationPattern>();
                db.CreateTable<OperationCategory>();
                db.CreateTable<OperationSubCategory>();
                db.CreateTable<CashAccount>();
                db.CreateTable<CardAccount>();
                db.CreateTable<BankAccount>();

                db.Execute("INSERT INTO sqlite_sequence (name, seq) SELECT 'Account', 0 WHERE NOT EXISTS(SELECT 1 FROM sqlite_sequence WHERE name = 'Account')");

                if (!(db.Table<OperationCategory>().Any())) {
                    addOperationCategory(new OperationCategory { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true });
                    addOperationCategory(new OperationCategory { Id = 2, Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true });
                    addOperationCategory(new OperationCategory { Id = 3, Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true });
                    addOperationCategory(new OperationCategory { Id = 4, Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true });
                    addOperationCategory(new OperationCategory { Id = 5, Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true });
                    addOperationCategory(new OperationCategory { Id = 6, Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false});

                    addOperationSubCategory(new OperationSubCategory { Id = 1, Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = 4, VisibleInIncomes = false, VisibleInExpenses = true });
                    addOperationSubCategory(new OperationSubCategory { Id = 2, Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = 3, VisibleInIncomes = false, VisibleInExpenses = true });
                }

                if (!(db.Table<CashAccount>().Any() || db.Table<BankAccount>().Any())) {
                    AccountsDal.addAccount(new CashAccount { Name = "Gotówka", ColorKey = "01" });
                    AccountsDal.addAccount(new BankAccount { Name = "Konto bankowe", ColorKey = "02", });
                    AccountsDal.addAccount(new CardAccount { Name = "Karta", ColorKey = "03", BankAccountId = db.ExecuteScalar<int>("SELECT Id FROM BankAccount LIMIT 1")});
                }
            }
        }


        /*
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
*/

        public static void DeleteAll() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.DeleteAll<Operation>();
                db.DeleteAll<OperationPattern>();
                db.DeleteAll<OperationCategory>();
                db.DeleteAll<OperationSubCategory>();
                db.DeleteAll<CashAccount>();
                db.DeleteAll<CardAccount>();
                db.DeleteAll<BankAccount>();
                db.Execute("DELETE FROM sqlite_sequence");
            }
        }

        public static void AddInitialElements() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                db.Execute("INSERT INTO sqlite_sequence (name, seq) SELECT 'Account', 0 WHERE NOT EXISTS(SELECT 1 FROM sqlite_sequence WHERE name = 'Account')");

                addOperationCategory(new OperationCategory { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true });
                addOperationCategory(new OperationCategory { Id = 2, Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true });
                addOperationCategory(new OperationCategory { Id = 3, Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true });
                addOperationCategory(new OperationCategory { Id = 4, Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true });
                addOperationCategory(new OperationCategory { Id = 5, Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true });
                addOperationCategory(new OperationCategory { Id = 6, Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false });

                addOperationSubCategory(new OperationSubCategory { Id = 1, Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = 4, VisibleInIncomes = false, VisibleInExpenses = true });
                addOperationSubCategory(new OperationSubCategory { Id = 2, Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = 3, VisibleInIncomes = false, VisibleInExpenses = true });

                AccountsDal.addAccount(new CashAccount { Name = "Gotówka", ColorKey = "01" });
                AccountsDal.addAccount(new BankAccount { Name = "Konto bankowe", ColorKey = "02", });
                AccountsDal.addAccount(new CardAccount { Name = "Karta", ColorKey = "03", BankAccountId = db.ExecuteScalar<int>("SELECT Id FROM BankAccount LIMIT 1") });
            }
        }

        public static Operation getEldestOperation() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE Date IS NOT NULL AND Date != '' ORDER BY Date LIMIT 1").FirstOrDefault();
            }
        }

        internal static List<Operation> getAllOperationsFromRangeToStatistics(DateTime minDate, DateTime maxDate) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE (VisibleInStatistics OR VisibleInStatistics ISNULL) AND Date >= ? AND Date <= ?", minDate.ToString("yyyy.MM.dd"), maxDate.ToString("yyyy.MM.dd"));
            }
        }


        /* GET ALL */

        public static List<OperationCategory> getAllCategories() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<OperationCategory>("SELECT * FROM OperationCategory ORDER BY Name");
            }
        }

        public static List<OperationCategory> getAllCategoriesInExpenses() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInExpenses ORDER BY Name");
            }
        }
        public static List<OperationCategory> getAllCategoriesInIncomes() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInIncomes ORDER BY Name");
            }
        }

        public static HashSet<CategoryWithSubCategories> getOperationCategoriesWithSubCategoriesInExpenses() {
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                /*
                var test = from category in db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInExpenses ORDER BY Name")
                           join subCategory in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInExpenses ORDER BY Name")
                           on category.Id equals subCategory.BossCategoryId
                           select new {
                               category,
                               subCategory
                           };
                           */

                List<OperationCategory> categories = db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInExpenses ORDER BY Name");
                var subCategoriesGroups = from subCategory in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInExpenses ORDER BY Name")
                                          group subCategory by subCategory.BossCategoryId into g
                                          select new {
                                              BossCategoryId = g.Key,
                                              subCategories = g.ToList()
                                          };


                HashSet<CategoryWithSubCategories> categoriesWithSubCategories = new HashSet<CategoryWithSubCategories>();

                foreach (var item in categories) {
                    CategoryWithSubCategories categoryWithSubCategories = new CategoryWithSubCategories {
                        Category = item,
                    };

                    var yco = subCategoriesGroups.FirstOrDefault(i => i.BossCategoryId == item.Id);//.subCategories;

                    if (yco != null)
                        categoryWithSubCategories.SubCategories = new ObservableCollection<OperationSubCategory>(yco.subCategories);

                    categoriesWithSubCategories.Add(categoryWithSubCategories);
                }

                return categoriesWithSubCategories;
            }
        }

        public static HashSet<CategoryWithSubCategories> getOperationCategoriesWithSubCategoriesInIncomes() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                List<OperationCategory> categories = db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInIncomes ORDER BY Name");
                var subCategoriesGroups = from subCategory in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInIncomes ORDER BY Name")
                                          group subCategory by subCategory.BossCategoryId into g
                                          select new {
                                              BossCategoryId = g.Key,
                                              subCategories = g.ToList()
                                          };


                HashSet<CategoryWithSubCategories> categoriesWithSubCategories = new HashSet<CategoryWithSubCategories>();

                foreach (var item in categories) {
                    CategoryWithSubCategories categoryWithSubCategories = new CategoryWithSubCategories {
                        Category = item,
                    };

                    var yco = subCategoriesGroups.FirstOrDefault(i => i.BossCategoryId == item.Id);

                    if (yco != null)
                        categoryWithSubCategories.SubCategories = new ObservableCollection<OperationSubCategory>(yco.subCategories);

                    categoriesWithSubCategories.Add(categoryWithSubCategories);
                }

                return categoriesWithSubCategories;
            }
        }

        public static List<Operation> getAllOperationsOfThisMoneyAccount(MoneyAccount account) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE MoneyAccountId == ?", account.Id);
            }
        }

        public static decimal getBalanceOfCertainDay(DateTime dateTime) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>("SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) FROM Operation WHERE Date <= ? AND Date IS NOT NULL AND Date IS NOT ''", dateTime.ToString("yyyy.MM.dd"));
            }
        }

        public static List<Operation> getAllOperations(DateTime actualMonth, HashSet<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                
                List<Operation> list = db.Query<Operation>("SELECT * FROM Operation WHERE Date GLOB ? AND Date <= ?", actualMonth.ToString("yyyy.MM*"), DateTime.Today.ToString("yyyy.MM.dd"));

                if (visiblePayFormList != null) {
                    models = (from p in list
                              where visiblePayFormList.Any(iteme => iteme == p.MoneyAccountId) == true
                              select p).ToList();
                }
                else
                    models = list;
            }

            return models;
        }

        public static List<Operation> getAllFutureOperations() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE Date > ? OR Date IS NULL OR Date == ''", DateTime.Today.ToString("yyyy.MM.dd"));
            }
        }

        public static List<Operation> getAllFutureOperations(HashSet<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<Operation> list = db.Query<Operation>("SELECT * FROM Operation WHERE Date > ? OR Date IS NULL OR Date == ''", DateTime.Today.ToString("yyyy.MM.dd"));

                if (visiblePayFormList != null) {
                    models = (from p in list
                              where visiblePayFormList.Any(iteme => iteme == p.MoneyAccountId)
                              select p).ToList();
                }
                else
                    models = list;
            }

            return models;
        }

        public static List<OperationPattern> getAllPatterns() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Table<OperationPattern>().ToList();
            }
        }
        

        /* GET BY ID */
    
        public static OperationSubCategory getOperationSubCategoryById(int Id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE Id == ? LIMIT 1", Id).FirstOrDefault();
            }
        }

        public static List<OperationSubCategory> getOperationSubCategoriesByBossId(int Id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE BossCategoryId == ? ORDER BY Name", Id);
            }
        }

        public static OperationCategory getOperationCategoryById(int Id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE Id == ? LIMIT 1", Id).FirstOrDefault();
            }
        }


        /* SAVE */

        public static void saveOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                if (operation.Id == 0)
                    db.Insert(operation);
                else
                    db.Update(operation);
            }
        }

        public static void saveOperationPattern(OperationPattern operationPattern) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                if (operationPattern.Id == 0)
                    db.Insert(operationPattern);
                else
                    db.Update(operationPattern);
            }
        }

        public static void updateOperationCategory(OperationCategory operationCategory) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(operationCategory);
            }
        }

        public static void addOperationCategory(OperationCategory operationCategory) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(operationCategory);
            }
        }

        public static void updateOperationSubCategory(OperationSubCategory operationSubCategory) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(operationSubCategory);
            }
        }

        public static void addOperationSubCategory(OperationSubCategory operationSubCategory) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(operationSubCategory);
            }
        }
        

        /* DELETE */

        public static void deletePattern(OperationPattern operationPattern) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM OperationPattern WHERE Id = ?", operationPattern.Id);
            }
        }

        public static void deleteOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM Operation WHERE Id = ?", operation.Id);
            }
        }

        public static void deleteCategoryWithSubCategories(int categoryId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM OperationCategory WHERE Id = ?", categoryId);
                db.Execute("DELETE FROM OperationSubCategory WHERE BossCategoryId = ?", categoryId);
            }
        }

        public static void deleteSubCategory(int subCategoryId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM OperationSubCategory WHERE Id = ?", subCategoryId);
            }
        }
    }
}