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

    internal class Dal : DalBase{

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

                AddCategory(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true });
                AddCategory(new Category { Id = 2, Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true });
                AddCategory(new Category { Id = 3, Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true });
                AddCategory(new Category { Id = 4, Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true });
                AddCategory(new Category { Id = 5, Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true });
                AddCategory(new Category { Id = 6, Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false });

                AddSubCategory(new SubCategory { Id = 1, Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = 4, VisibleInIncomes = false, VisibleInExpenses = true });
                AddSubCategory(new SubCategory { Id = 2, Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = 3, VisibleInIncomes = false, VisibleInExpenses = true });

                AccountsDal.AddAccount(new CashAccount { Name = "Gotówka", ColorKey = "01" });
                AccountsDal.AddAccount(new BankAccount { Name = "Konto bankowe", ColorKey = "02", });
                AccountsDal.AddAccount(new CardAccount { Name = "Karta", ColorKey = "03", BankAccountId = db.ExecuteScalar<int>("SELECT Id FROM BankAccount LIMIT 1") });
            }
        }

        public static Operation GetEldestOperation() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE Date IS NOT NULL AND Date != '' ORDER BY Date LIMIT 1").FirstOrDefault();
            }
        }

        internal static List<Operation> GetAllOperationsFromRangeToStatistics(DateTime minDate, DateTime maxDate) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE (VisibleInStatistics OR VisibleInStatistics ISNULL) AND Date >= ? AND Date <= ?", minDate.ToString("yyyy.MM.dd"), maxDate.ToString("yyyy.MM.dd"));
            }
        }


        /* GET ALL */

        public static List<Category> GetAllCategories() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category ORDER BY Name");
            }
        }

        public static List<Category> GetAllCategoriesInExpenses() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE VisibleInExpenses ORDER BY Name");
            }
        }
        public static List<Category> GetAllCategoriesInIncomes() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE VisibleInIncomes ORDER BY Name");
            }
        }

        public static HashSet<CategoryWithSubCategories> GetCategoriesWithSubCategoriesInExpenses() {
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

        public static HashSet<CategoryWithSubCategories> GetCategoriesWithSubCategoriesInIncomes() {
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

        public static List<Operation> GetAllOperationsOfThisMoneyAccount(MoneyAccount account) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE MoneyAccountId == ?", account.Id);
            }
        }

        public static decimal GetBalanceOfCertainDay(DateTime dateTime) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>(
                    "SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) FROM Operation " +
                    "WHERE Date <= ? AND Date IS NOT NULL AND Date IS NOT ''", 
                    dateTime.ToString("yyyy.MM.dd"));
            }
        }

        public static List<Operation> GetAllOperations(DateTime actualMonth, HashSet<int> visiblePayFormList) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                
                var list = db.Query<Operation>(
                    "SELECT * FROM Operation WHERE Date GLOB ? AND Date <= ?", 
                    actualMonth.ToString("yyyy.MM*"), 
                    DateTime.Today.ToString("yyyy.MM.dd"));

                return visiblePayFormList != null 
                    ? list.Where(i => visiblePayFormList.Contains(i.MoneyAccountId)).ToList() 
                    : list;
            }
        }

        public static List<Operation> GetAllFutureOperations() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>(
                    "SELECT * FROM Operation WHERE Date > ? OR Date IS NULL OR Date == ''", 
                    DateTime.Today.ToString("yyyy.MM.dd"));
            }
        }

        public static List<Operation> GetAllFutureOperations(HashSet<int> visiblePayFormList) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var list = db.Query<Operation>(
                    "SELECT * FROM Operation WHERE Date > ? OR Date IS NULL OR Date == ''", 
                    DateTime.Today.ToString("yyyy.MM.dd"));

                return visiblePayFormList != null
                    ? list.Where(i => visiblePayFormList.Contains(i.MoneyAccountId)).ToList()
                    : list;
            }
        }

        public static List<OperationPattern> GetAllPatterns() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Table<OperationPattern>().ToList();
            }
        }
        

        /* GET BY ID */
    
        public static SubCategory GetSubCategoryById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>(
                    "SELECT * FROM SubCategory WHERE Id == ? LIMIT 1", id).FirstOrDefault();
            }
        }

        public static List<SubCategory> GetSubCategoriesByBossId(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>(
                    "SELECT * FROM SubCategory WHERE BossCategoryId == ? ORDER BY Name", id);
            }
        }

        public static Category GetCategoryById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE Id == ? LIMIT 1", id)
                    .FirstOrDefault();
            }
        }


        /* SAVE */

        public static void SaveOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                operation.LastModifed = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss");

                if (operation.Id == 0)
                    db.Insert(operation);
                else
                    db.Update(operation);
            }
        }

        public static void SaveOperationPattern(OperationPattern operationPattern) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                operationPattern.LastModifed = DateTime.Now.ToString("yyyy.MM.dd HH.mm.ss");

                if (operationPattern.Id == 0)
                    db.Insert(operationPattern);
                else
                    db.Update(operationPattern);
            }
        }

        public static void UpdateCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(category);
            }
        }

        public static void AddCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(category);
            }
        }

        public static void UpdateSubCategory(SubCategory subCategory) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(subCategory);
            }
        }

        public static void AddSubCategory(SubCategory subCategory) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(subCategory);
            }
        }
        

        /* DELETE */

        public static void DeletePattern(OperationPattern operationPattern) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM OperationPattern WHERE Id = ?", operationPattern.Id);
            }
        }

        public static void DeleteOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM Operation WHERE Id = ?", operation.Id);
            }
        }

        public static void DeleteCategoryWithSubCategories(int categoryId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM Category WHERE Id = ?", categoryId);
                db.Execute("DELETE FROM SubCategory WHERE BossCategoryId = ?", categoryId);
            }
        }

        public static void DeleteSubCategory(int subCategoryId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM SubCategory WHERE Id = ?", subCategoryId);
            }
        }
    }
}