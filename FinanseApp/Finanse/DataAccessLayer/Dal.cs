namespace Finanse.DataAccessLayer {
    using Models;
    using Models.MoneyAccounts;
    using SQLite.Net;
    using SQLite.Net.Platform.WinRT;
    using System;
    using System.Collections.Generic;
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
            /// blokuje się gdy data jest w formacie innym niż yyyy.MM.dd , np. yyyy-MM-dd . czyli należy jeszcze sprawdzać czy stiring jest {0}.{1}.{2} czy coś

            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();


                if (db.Table<Operation>().Count() == 0)
                    return null;
                
                Operation eldest = db.Query<Operation>("SELECT * FROM Operation WHERE Date IS NOT NULL AND Date != '' ORDER BY Date LIMIT 1").FirstOrDefault();
               // string eldestS = db.Query<string>("SELECT `Date` FROM Operation WHERE `Date` IS NOT NULL AND `Date` != '' ORDER BY `Date` LIMIT 1").FirstOrDefault();

                return eldest;
            }
        }
        
        internal static List<Operation> getAllOperationsFromRange(DateTime minDate, DateTime maxDate) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = db.Query<Operation>("SELECT * FROM Operation WHERE Date >= ? AND Date <= ?", minDate.ToString("yyyy.MM.dd"), maxDate.ToString("yyyy.MM.dd"));
            }

            return models;
        }

        public static List<MoneyAccountBalance> listOfMoneyAccountBalances(DateTime date) {
            List<MoneyAccountBalance> list = new List<MoneyAccountBalance>();

            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
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
            List<OperationCategory> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = db.Query<OperationCategory>("SELECT * FROM OperationCategory ORDER BY Name");
            }

            return models;
        }

        public static List<OperationCategory> getAllCategoriesInExpenses() {
            List<OperationCategory> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInExpenses ORDER BY Name");
            }

            return models;
        }
        public static List<OperationCategory> getAllCategoriesInIncomes() {
        
            List<OperationCategory> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInIncomes ORDER BY Name");
            }

            return models;
        }

        public static List<OperationCategory> getOperationCategoriesWithSubCategoriesInExpenses() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                HashSet<OperationCategory> list = new HashSet<OperationCategory>();

                var test = from category in db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInExpenses ORDER BY Name")
                           join subCategory in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInExpenses ORDER BY Name")
                           on category.Id equals subCategory.BossCategoryId
                           select new {
                               category,
                               subCategory
                           };

                foreach (var item in test) {
                    list.Add(item.category);
                    list.FirstOrDefault(i => i == item.category).SubCategories.Add(item.subCategory);
                }

                return list.ToList();
            }
        }

        public static List<OperationCategory> getOperationCategoriesWithSubCategoriesInIncomes() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                HashSet<OperationCategory> list = new HashSet<OperationCategory>();

                var test = from category in db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInIncomes ORDER BY Name")
                           join subCategory in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInIncomes ORDER BY Name")
                           on category.Id equals subCategory.BossCategoryId
                           select new {
                               category,
                               subCategory
                           };

                foreach (var item in test) {
                    list.Add(item.category);
                    list.FirstOrDefault(i => i == item.category).SubCategories.Add(item.subCategory);
                }

                return list.ToList();
            }
        }

        public static string getJoinTest() {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                //       List<object> models = db.Query<object>("SELECT * FROM OperationCategory LEFT OUTER JOIN OperationSubCategory ON OperationCategory.Id = OperationSubCategory.BossCategoryId WHERE OperationCategory.VisibleInExpenses AND OperationSubCategory.VisibleInExpenses");

                HashSet<OperationCategory> list = new HashSet<OperationCategory>();

                var test = (from category in db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE VisibleInExpenses")
                            join subCategory in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInExpenses")
                            on category.Id equals subCategory.BossCategoryId
                            select new { category, subCategory });

                foreach (var item in test) {
                    list.Add(item.category);
                    list.FirstOrDefault(i => i == item.category).SubCategories.Add(item.subCategory);
                }

                string result = string.Empty;

                foreach (var item in list) {
                    result += item.Name + "\n";

                    foreach(var itemka in item.SubCategories)
                        result += itemka.Name + "\n";

                    result += "\n";
                }

                return result;
            }
        }

        public static List<List<OperationSubCategory>> getAllSubCategoriesInExpensesGroupedByBoss() {

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<List<OperationSubCategory>> models = (
                    from p in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInExpenses ORDER BY Name")
                    group p by p.BossCategoryId into g
                    select new List<OperationSubCategory>(g)
                             ).ToList();

                return models;
            }
        }

        public static List<List<OperationSubCategory>> getAllSubCategoriesInIncomesGroupedByBoss() {

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<List<OperationSubCategory>> models = (
                    from p in db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE VisibleInIncomes ORDER BY Name")
                    group p by p.BossCategoryId into g
                    select new List<OperationSubCategory>(g)
                             ).ToList();

                return models;
            }
        }

        public static List<CardAccount> getListOfLinkedCardAccountToThisBankAccount(BankAccount account) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
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
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
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
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
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

        public static List<Operation> getAllOperations(int month, int year, HashSet<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                string settedYearAndMonth = year.ToString() + "." + month.ToString("00") + "*";

                //      db.Query<Operation>("SELECT * FROM Operation WHERE Date GLOB '?'", settedYearAndMonth);

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

        public static List<Operation> getAllFutureOperations() {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<Operation> models = db.Query<Operation>("SELECT * FROM Operation WHERE Date > ? OR Date IS NULL OR Date == ''", DateTime.Today.ToString("yyyy.MM.dd"));

                return models;
            }
        }

        public static List<Operation> getAllFutureOperations(HashSet<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
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
            List<OperationPattern> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = db.Table<OperationPattern>().ToList();
            }

            return models;
        }

        public static List<MoneyAccount> getAllMoneyAccounts() {
            List<MoneyAccount> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                /*
                var table = db.Query<MoneyAccount>("SELECT * FROM MoneyAccount");//db.Table<MoneyAccount>();
                var test = db.Query<OperationCategory>("SELECT * FROM Settings");
                */

                models = db.Table<MoneyAccount>().ToList();
            }

            return models;
        }

        /* GET BY ID */
    
        public static Operation getOperationById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                Operation m = db.Query<Operation>("SELECT * FROM Operation WHERE Id == ? LIMIT 1", Id).FirstOrDefault();

                return m;
            }
        }

        public static MoneyAccount getMoneyAccountById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                MoneyAccount m = db.Query<MoneyAccount>("SELECT * FROM MoneyAccount WHERE Id == ? LIMIT 1", Id).FirstOrDefault();

                return m;
            }
        }

        public static OperationSubCategory getOperationSubCategoryById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                OperationSubCategory m = db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE Id == ? LIMIT 1", Id).FirstOrDefault();

                return m;
            }
        }

        public static List<OperationSubCategory> getOperationSubCategoriesByBossId(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                List<OperationSubCategory> m = db.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory WHERE BossCategoryId == ? ORDER BY Name", Id);

                return m;
            }
        }

        public static OperationCategory getOperationCategoryById(int Id) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();
                OperationCategory m = db.Query<OperationCategory>("SELECT * FROM OperationCategory WHERE Id == ? LIMIT 1", Id).FirstOrDefault();

                return m;
            }
        }

        /* SAVE */

        public static void saveOperation(Operation operation) {
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

        public static void saveOperationPattern(OperationPattern operationPattern) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (operationPattern.Id == 0) {
                    db.Insert(operationPattern);
                }
                else {
                    db.Update(operationPattern);
                }
            }
        }

        public static void saveOperationCategory(OperationCategory operationCategory) {
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

        public static void saveOperationSubCategory(OperationSubCategory operationSubCategory) {
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

        public static void deletePattern(OperationPattern operationPattern) {
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

        public static void deleteOperation(Operation operation) {
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

        public static void deleteCategoryWithSubCategories(int categoryId) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM OperationCategory WHERE Id = ?", categoryId);
                db.Execute("DELETE FROM OperationSubCategory WHERE BossCategoryId = ?", categoryId);
            }
        }

        public static void deleteSubCategory(int subCategoryId) {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                // Object model:
                //db.Delete(person);

                // SQL Syntax:
                db.Execute("DELETE FROM OperationSubCategory WHERE Id = ?", subCategoryId);
            }
        }
    }
}