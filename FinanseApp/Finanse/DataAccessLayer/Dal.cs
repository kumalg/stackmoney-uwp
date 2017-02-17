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
                if (string.IsNullOrEmpty(dbPath)) {
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");
                //    dbPath = Path.Combine(ApplicationData.Current.RoamingFolder.Path, "db.sqlite");
                }

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
                db.Execute("CREATE TABLE IF NOT EXISTS images ( "
                    + "nameRed VARCHAR(20) NOT NULL PRIMARY KEY,"
                    + "patientID INT,"
                    + "FOREIGN KEY(patientID) REFERENCES patients(id) ) ");
                db.CreateTable<MoneyAccount>();
                db.CreateTable<Operation>();
                db.CreateTable<OperationPattern>();
                db.CreateTable<OperationCategory>();
                db.CreateTable<OperationSubCategory>();
                //db.CreateTable<Settings>();
                /*
                if (!db.Table<Settings>().Any())
                    db.Insert(new Settings {
                        CultureInfoName = "en-US"
                    });
                    */
                    
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

        public static List<MoneyAccountBalance> listOfMoneyAccountBalances(DateTime date) {
            List<MoneyAccountBalance> list = new List<MoneyAccountBalance>();

            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var query = from p in db.Table<Operation>().ToList()
                        group p by p.MoneyAccountId into g
                        select new {
                            moneyAccount = getMoneyAccountById(g.Key),
                            initialValue = getInitialValue(g, date),
                            finalValue = getFinalValue(g,date)
                        };

                foreach (var item in query) {
                    list.Add(new MoneyAccountBalance(item.moneyAccount, item.initialValue, item.finalValue));
                }
            }
            return list;
        }

        private static DateTime maxDateInFinalValue(DateTime date) {
            return (date.Month == DateTime.Today.Month && date.Year == DateTime.Today.Year) ?
                DateTime.Today.AddDays(1) :
                date.AddMonths(1);
        }
        private static DateTime maxDateInInitialValue(DateTime date) {
            return (date > DateTime.Today) ?
                DateTime.Today.AddDays(1) :
                date;
        }

        private static decimal getFinalValue(IGrouping<int,Operation> operations, DateTime date) {
            if (date.Date > DateTime.Today.Date)
                return operations.Sum(i => i.isExpense ? -i.Cost : i.Cost);
            else
                return operations.Where(i => !string.IsNullOrEmpty(i.Date) && DateTime.Parse(i.Date) < maxDateInFinalValue(date))
                                    .Sum(i => i.isExpense ? -i.Cost : i.Cost);
        }
        private static decimal getInitialValue(IGrouping<int, Operation> operations, DateTime date) {
                return operations.Where(i => !string.IsNullOrEmpty(i.Date) && DateTime.Parse(i.Date) < maxDateInInitialValue(date))
                                    .Sum(i => i.isExpense ? -i.Cost : i.Cost);
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
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                /*
                var test = from category in db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInIncomes ORDER BY Name")
                           join subCategory in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInIncomes ORDER BY Name")
                           on category.Id equals subCategory.BossCategoryId
                           select new {
                               category,
                               subCategory
                           };

                HashSet<CategoryWithSubCategories> categoryWithSubCategories = new HashSet<CategoryWithSubCategories>();

                foreach (var item in test) {
                    categoryWithSubCategories.Add(new CategoryWithSubCategories { Category = item.category });
                    categoryWithSubCategories.FirstOrDefault(i => i.Category == item.category).SubCategories.Add(item.subCategory);
                }

                return categoryWithSubCategories;
                */
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

        public static List<CardAccount> getListOfLinkedCardAccountToThisBankAccount(BankAccount account) {
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<CardAccount> models = (from p in db.Table<CardAccount>().ToList()
                              where p.BankAccountId == account.Id
                              select p).ToList();

                return models;
            }
        }

        public static List<Operation> getAllOperationsOfThisMoneyAccount(BankAccount account) {

            // Create a new connection
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<Operation> models = (from p in db.Table<Operation>().ToList()
                          where p.MoneyAccountId == account.Id 
                            || getListOfLinkedCardAccountToThisBankAccount(account)
                            .Any(i => i.BankAccountId == account.Id)
                          select p).ToList();

                return models;
            }
        }


        public static List<Operation> getAllOperationsOfThisMoneyAccount(Account account) {
            List<Operation> models;

            // Create a new connection
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (account is BankAccount)
                    models = (from p in db.Table<Operation>().ToList()
                              where p.MoneyAccountId == account.Id || getListOfLinkedCardAccountToThisBankAccount((BankAccount)account).Any(i=>i.BankAccountId == account.Id)
                              select p).ToList();
                else
                    models = db.Query<Operation>("SELECT * FROM Operation WHERE MoneyAccountId == ?", account.Id);
            }

            return models;
        }

        public static List<Operation> getAllOperationsOfThisMoneyAccount(MoneyAccount account) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = db.Query<Operation>("SELECT * FROM Operation WHERE MoneyAccountId == ?", account.Id);
            }

            return models;
        }

        public static decimal getBalanceOfCertainDay(DateTime dateTime) {
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // return db.Query("SELECT CASE WHEN isExpense THEN -Cost ELSE Cost END FROM Operation WHERE Date <= ? AND Date IS NOT NULL AND Date IS NOT ''", dateTime.ToString("yyyy.MM.dd")).Sum();
                return db.Query<Operation>("SELECT * FROM Operation WHERE Date <= ? AND Date IS NOT NULL AND Date IS NOT ''", dateTime.ToString("yyyy.MM.dd")).Sum(i => i.SignedCost);
            }
        }

        public static List<Operation> getAllOperations(int month, int year, HashSet<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                string settedYearAndMonth = year.ToString() + "." + month.ToString("00") + "*";

                List<Operation> list = db.Query<Operation>("SELECT * FROM Operation WHERE Date GLOB ? AND Date <= ?", settedYearAndMonth, DateTime.Today.ToString("yyyy.MM.dd"));

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

        public static List<MoneyAccount> getAllMoneyAccounts() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<MoneyAccount>("SELECT * FROM MoneyAccount");
            }
        }

        /* GET BY ID */
    
        public static Operation getOperationById(int Id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE Id == ? LIMIT 1", Id).FirstOrDefault();
            }
        }

        public static MoneyAccount getMoneyAccountById(int Id) {
            // Create a new connection
            using (var db = DbConnection) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                return db.Query<MoneyAccount>("SELECT * FROM MoneyAccount WHERE Id == ? LIMIT 1", Id).FirstOrDefault();
            }
        }

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