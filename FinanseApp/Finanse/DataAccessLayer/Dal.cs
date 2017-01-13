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

                // eldest = db.Table<Operation>().Aggregate((c1, c2) => Convert.ToDateTime(c1.Date) < Convert.ToDateTime(c2.Date) ? c1 : c2);
                // to jest spoko, ale może być super kłopotliwe przy duzych bazach.

                Operation eldest = (from p in db.Table<Operation>().ToList()
                          where !string.IsNullOrEmpty(p.Date)
                          orderby p.Date
                          select p).ToList().ElementAt(0);

                return eldest;
            }
        }

        //TODO
        public static DateTime getEldestDateInOperations() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();


               // if (db.Table<Operation>().Count() == 0)
                    //return null;

                // eldest = db.Table<Operation>().Aggregate((c1, c2) => Convert.ToDateTime(c1.Date) < Convert.ToDateTime(c2.Date) ? c1 : c2);
                // to jest spoko, ale może być super kłopotliwe przy duzych bazach.

                Operation eldest = (from p in db.Table<Operation>().ToList()
                                    where !String.IsNullOrEmpty(p.Date)
                                    orderby p.Date
                                    select p).ToList().ElementAt(0);

                return Convert.ToDateTime(eldest.Date);
            }
        }

        public static decimal[] getBalanceFromSingleAccountToDate(DateTime date, int moneyAccountId) {
            decimal[] values = new decimal[2]; // initial, final

            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                values[0] = (from p in db.Table<Operation>().ToList()
                             where p.MoneyAccountId == moneyAccountId && !String.IsNullOrEmpty(p.Date) && DateTime.Parse(p.Date) < date
                             select p.isExpense ? -p.Cost : p.Cost).Sum();
                
                values[1] = (from p in db.Table<Operation>().ToList()
                             where p.MoneyAccountId == moneyAccountId && !String.IsNullOrEmpty(p.Date) && DateTime.Parse(p.Date) < date.AddMonths(1)
                             select p.isExpense ? -p.Cost : p.Cost).Sum();
            }

            return values;
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

                models = (from p in db.Table<OperationCategory>()
                          orderby p.Name
                          select p).ToList();
            }

            return models;
        }

        public static List<List<OperationSubCategory>> getAllSubCategoriesInExpensesGroupedByBoss() {

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<List<OperationSubCategory>> models = new List<List<OperationSubCategory>>();


                var query = (from p in db.Table<OperationSubCategory>()
                          where p.VisibleInExpenses orderby p.Name
                          group p by p.BossCategoryId into g
                          select new {
                              GroupName = g.Key,
                              Items = g
                          }).ToList();

                foreach (var g in query)
                    models.Add(g.Items.ToList());

                return models;
            }
        }

        public static List<List<OperationSubCategory>> getAllSubCategoriesInIncomesGroupedByBoss() {

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<List<OperationSubCategory>> models = new List<List<OperationSubCategory>>();


                var query = (from p in db.Table<OperationSubCategory>()
                             where p.VisibleInIncomes
                             orderby p.Name
                             group p by p.BossCategoryId into g
                             select new {
                                 GroupName = g.Key,
                                 Items = g
                             }).ToList();

                foreach (var g in query)
                    models.Add(g.Items.ToList());

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
                    models = (from p in db.Table<Operation>().ToList()
                              where p.MoneyAccountId == account.Id
                              select p).ToList();
            }

            return models;
        }

        public static List<Operation> getAllOperationsOfThisMoneyAccount(MoneyAccount account) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                models = (from p in db.Table<Operation>().ToList()
                          where p.MoneyAccountId == account.Id
                          select p).ToList();
            }

            return models;
        }

        public static List<Operation> getAllOperations(int month, int year) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                string date = /*year.ToString() + "." + month.ToString("00");*/ String.Format("{yyyy}.{MM}", year, month);

                models = (from p in db.Table<Operation>().ToList()
                          where p.Date.Substring(0,7) == date.ToString() 
                             && Convert.ToDateTime(p.Date) <= DateTime.Today
                          select p).ToList();
            }

            return models;
        }

        public static List<Operation> getAllOperations(int month, int year, HashSet<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                string settedYearAndMonth = year.ToString() + "." + month.ToString("00");

                if (visiblePayFormList != null) {
                    models = (from p in db.Table<Operation>().ToList()
                              where !string.IsNullOrEmpty(p.Date)
                                 && p.Date.Substring(0, 7) == settedYearAndMonth.ToString()
                                 && Convert.ToDateTime(p.Date) <= DateTime.Today
                                 && visiblePayFormList.Any(iteme => iteme == p.MoneyAccountId) == true
                              select p).ToList();
                }
                else {
                    models = (from p in db.Table<Operation>().ToList()
                              where p.Date != null && p.Date != ""
                                 && p.Date.Substring(0, 7) == settedYearAndMonth.ToString()
                                 && Convert.ToDateTime(p.Date) <= DateTime.Today
                              select p).ToList();
                }
            }

            return models;
        }

        public static List<Operation> getAllFutureOperations() {
            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                List<Operation> models = (from p in db.Table<Operation>().ToList()
                          where string.IsNullOrEmpty(p.Date)
                             || Convert.ToDateTime(p.Date) > DateTime.Today
                          select p).ToList();

                return models;
            }
        }

        public static List<Operation> getAllFutureOperations(HashSet<int> visiblePayFormList) {
            List<Operation> models;

            // Create a new connection
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                // Activate Tracing
                db.TraceListener = new DebugTraceListener();

                if (visiblePayFormList != null) {
                    models = (from p in db.Table<Operation>().ToList()
                              where (string.IsNullOrEmpty(p.Date)
                                  || Convert.ToDateTime(p.Date) > DateTime.Today)
                                  && visiblePayFormList.Any(iteme => iteme == p.MoneyAccountId)
                              select p).ToList();
                }
                else {
                    models = (from p in db.Table<Operation>().ToList()
                              where string.IsNullOrEmpty(p.Date)
                                 || Convert.ToDateTime(p.Date) > DateTime.Today
                              select p).ToList();
                }
            }

            return models;
        }

        public static List<OperationPattern> getAllPatterns() {
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

        public static List<MoneyAccount> getAllMoneyAccounts() {
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
    
        public static Operation getOperationById(int Id) {
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

        public static MoneyAccount getMoneyAccountById(int Id) {
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

        public static OperationSubCategory getOperationSubCategoryById(int Id) {
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

        public static List<OperationSubCategory> getOperationSubCategoriesByBossId(int Id) {
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

        public static OperationCategory getOperationCategoryById(int Id) {
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

        public static void deleteCategory(OperationCategory operationCategory) {
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

        public static void deleteSubCategory(OperationSubCategory operationSubCategory) {
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