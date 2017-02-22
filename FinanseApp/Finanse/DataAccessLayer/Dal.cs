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
              //  db.Execute("ALTER TABLE OperationCategory RENAME TO Category");
               // db.Execute("ALTER TABLE OperationSubCategory RENAME TO SubCategory");

                db.CreateTable<Operation>();
                db.CreateTable<OperationPattern>();
                db.CreateTable<Category>();
                db.CreateTable<SubCategory>();
                db.CreateTable<CashAccount>();
                db.CreateTable<CardAccount>();
                db.CreateTable<BankAccount>();

                db.Execute("INSERT INTO sqlite_sequence (name, seq) SELECT 'Account', 0 WHERE NOT EXISTS(SELECT 1 FROM sqlite_sequence WHERE name = 'Account')");

                if (!(db.Table<Category>().Any())) {
                    addCategory(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true });
                    addCategory(new Category { Id = 2, Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true });
                    addCategory(new Category { Id = 3, Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true });
                    addCategory(new Category { Id = 4, Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true });
                    addCategory(new Category { Id = 5, Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true });
                    addCategory(new Category { Id = 6, Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false});

                    addSubCategory(new SubCategory { Id = 1, Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = 4, VisibleInIncomes = false, VisibleInExpenses = true });
                    addSubCategory(new SubCategory { Id = 2, Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = 3, VisibleInIncomes = false, VisibleInExpenses = true });
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
                db.DeleteAll<Category>();
                db.DeleteAll<SubCategory>();
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

                addCategory(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true });
                addCategory(new Category { Id = 2, Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true });
                addCategory(new Category { Id = 3, Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true });
                addCategory(new Category { Id = 4, Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true });
                addCategory(new Category { Id = 5, Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true });
                addCategory(new Category { Id = 6, Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false });

                addSubCategory(new SubCategory { Id = 1, Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = 4, VisibleInIncomes = false, VisibleInExpenses = true });
                addSubCategory(new SubCategory { Id = 2, Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = 3, VisibleInIncomes = false, VisibleInExpenses = true });

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

        public static List<Category> getAllCategories() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category ORDER BY Name");
            }
        }

        public static List<Category> getAllCategoriesInExpenses() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE VisibleInExpenses ORDER BY Name");
            }
        }
        public static List<Category> getAllCategoriesInIncomes() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE VisibleInIncomes ORDER BY Name");
            }
        }

        public static HashSet<CategoryWithSubCategories> getCategoriesWithSubCategoriesInExpenses() {
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                /*
                var test = from category in db.Query<Category>("SELECT * FROM Category WHERE VisibleInExpenses ORDER BY Name")
                           join subCategory in db.Query<SubCategory>("SELECT * FROM SubCategory WHERE VisibleInExpenses ORDER BY Name")
                           on category.Id equals subCategory.BossCategoryId
                           select new {
                               category,
                               subCategory
                           };
                           */

                List<Category> categories = db.Query<Category>("SELECT * FROM Category WHERE VisibleInExpenses ORDER BY Name");
                var subCategoriesGroups = from subCategory in db.Query<SubCategory>("SELECT * FROM SubCategory WHERE VisibleInExpenses ORDER BY Name")
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
                        categoryWithSubCategories.SubCategories = new ObservableCollection<SubCategory>(yco.subCategories);

                    categoriesWithSubCategories.Add(categoryWithSubCategories);
                }

                return categoriesWithSubCategories;
            }
        }

        public static HashSet<CategoryWithSubCategories> getCategoriesWithSubCategoriesInIncomes() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                List<Category> categories = db.Query<Category>("SELECT * FROM Category WHERE VisibleInIncomes ORDER BY Name");
                var subCategoriesGroups = from subCategory in db.Query<SubCategory>("SELECT * FROM SubCategory WHERE VisibleInIncomes ORDER BY Name")
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
                        categoryWithSubCategories.SubCategories = new ObservableCollection<SubCategory>(yco.subCategories);

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
    
        public static SubCategory getSubCategoryById(int Id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM SubCategory WHERE Id == ? LIMIT 1", Id).FirstOrDefault();
            }
        }

        public static List<SubCategory> getSubCategoriesByBossId(int Id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM SubCategory WHERE BossCategoryId == ? ORDER BY Name", Id);
            }
        }

        public static Category getCategoryById(int Id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE Id == ? LIMIT 1", Id).FirstOrDefault();
            }
        }


        /* SAVE */

        public static void saveOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                operation.LastModifed = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss");

                if (operation.Id == 0)
                    db.Insert(operation);
                else
                    db.Update(operation);
            }
        }

        public static void saveOperationPattern(OperationPattern operationPattern) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                operationPattern.LastModifed = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss");

                if (operationPattern.Id == 0)
                    db.Insert(operationPattern);
                else
                    db.Update(operationPattern);
            }
        }

        public static void updateCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(category);
            }
        }

        public static void addCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(category);
            }
        }

        public static void updateSubCategory(SubCategory subCategory) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(subCategory);
            }
        }

        public static void addSubCategory(SubCategory subCategory) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(subCategory);
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
                db.Execute("DELETE FROM Category WHERE Id = ?", categoryId);
                db.Execute("DELETE FROM SubCategory WHERE BossCategoryId = ?", categoryId);
            }
        }

        public static void deleteSubCategory(int subCategoryId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM SubCategory WHERE Id = ?", subCategoryId);
            }
        }
    }
}