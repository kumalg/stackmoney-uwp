using Finanse.Models.DateTimeExtensions;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;

namespace Finanse.DataAccessLayer {
    using Models.Categories;
    using Models.Helpers;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Dal : DalBase{

        public static void DeleteAll() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.DeleteAll<Operation>();
                db.DeleteAll<OperationPattern>();
                db.DeleteAll<Category>();
                db.DeleteAll<SubCategory>();
                db.DeleteAll<MAccount>();
                db.Execute("DELETE FROM sqlite_sequence");
            }
        }

        public static void AddInitialElements() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                db.Execute("INSERT INTO sqlite_sequence (name, seq) SELECT 'Account', 0 WHERE NOT EXISTS(SELECT 1 FROM sqlite_sequence WHERE name = 'Account')");

                //  AddCategory(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true });
                CategoriesDal.AddCategory(new Category { Id = 2, Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true });
                CategoriesDal.AddCategory(new Category { Id = 3, Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true });
                CategoriesDal.AddCategory(new Category { Id = 4, Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true });
                CategoriesDal.AddCategory(new Category { Id = 5, Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true });
                CategoriesDal.AddCategory(new Category { Id = 6, Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false });

                CategoriesDal.AddCategory(new SubCategory { Id = 1, Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = "4", VisibleInIncomes = false, VisibleInExpenses = true });
                CategoriesDal.AddCategory(new SubCategory { Id = 2, Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = "3", VisibleInIncomes = false, VisibleInExpenses = true });

                MAccountsDal.AddAccount(new MAccount { Name = "Gotówka", ColorKey = "01" });
                MAccountsDal.AddAccount(new MAccount { Name = "Konto bankowe", ColorKey = "02", });
                MAccountsDal.AddAccount(new SubMAccount { Name = "Karta", ColorKey = "03", BossAccountGlobalId = db.ExecuteScalar<string>("SELECT GlobalId FROM MAccount WHERE Id = 2 LIMIT 1") });
            }
        }

        public static Operation GetEldestOperation() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE Date IS NOT NULL AND Date != '' AND IsDeleted = 0 ORDER BY Date LIMIT 1").FirstOrDefault();
            }
        }

        internal static List<Operation> GetAllOperationsFromRangeToStatistics(DateTime minDate, DateTime maxDate) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                var operations = db.Query<Operation>("SELECT * FROM Operation WHERE (VisibleInStatistics OR VisibleInStatistics ISNULL) AND Date >= ? AND Date <= ? AND IsDeleted = 0", minDate.ToString("yyyy.MM.dd"), maxDate.ToString("yyyy.MM.dd"));
                //operations.ForEach(i => i.GeneralCategory = CategoriesDal.GetCategoryByGlobalId(i.CategoryGlobalId, db));
                return operations;
            }
        }


        /* GET ALL */
        /*
        public static List<Category> GetAllCategories() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE IsDeleted = 0 ORDER BY Name");
            }
        }


        public static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategoriesInExpenses()
            => GetCategoriesWithSubCategories("VisibleInExpenses");

        public static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategoriesInIncomes()
            => GetCategoriesWithSubCategories("VisibleInIncomes");

        private static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategories(string visibleIn) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var subCategoriesGroups = from subCategory in db.Query<SubCategory>("SELECT * FROM SubCategory WHERE " + visibleIn  + " AND IsDeleted = 0 ORDER BY Name")
                                          group subCategory by subCategory.BossCategoryId into g
                                          select new {
                                              BossCategoryId = g.Key,
                                              subCategories = g.ToList()
                                          };

                return from category in db.Query<Category>("SELECT * FROM Category WHERE " + visibleIn + " AND IsDeleted = 0 ORDER BY Name")
                       join subCategories in subCategoriesGroups on category.GlobalId equals subCategories.BossCategoryId into gj
                       from secondSubCategories in gj.DefaultIfEmpty()
                       select new CategoryWithSubCategories {
                           Category = category,
                           SubCategories =
                               secondSubCategories == null
                                   ? new ObservableCollection<SubCategory>()
                                   : new ObservableCollection<SubCategory>(secondSubCategories.subCategories)
                       };
            }
        }
        */


        public static decimal GetBalanceOfCertainDay(DateTime dateTime) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>(
                    "SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) FROM Operation " +
                    "WHERE Date <= ? AND Date IS NOT NULL AND Date IS NOT '' AND IsDeleted = 0", 
                    dateTime.ToString("yyyy.MM.dd"));
            }
        }

        public static List<Operation> GetAllOperations(DateTime actualMonth, List<string> visiblePayFormList) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var list = db.Query<Operation>(
                    "SELECT * FROM Operation WHERE Date GLOB ? AND Date <= ? AND IsDeleted = 0",
                    actualMonth.ToString("yyyy.MM*"),
                    DateTime.Today.ToString("yyyy.MM.dd"));

                return visiblePayFormList != null 
                    ? list.Where(i => visiblePayFormList.Contains(i.MoneyAccountId.ToString())).ToList() 
                    : list;
            }
        }


        public static List<Operation> GetAllFutureOperations(List<string> visiblePayFormList) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var list = db.Query<Operation>(
                    "SELECT * FROM Operation WHERE Date > ? OR Date IS NULL OR Date == '' AND IsDeleted = 0", 
                    DateTime.Today.ToString("yyyy.MM.dd"));

                return visiblePayFormList != null
                    ? list.Where(i => visiblePayFormList.Contains(i.MoneyAccountId.ToString())).ToList()
                    : list;
            }
        }

        public static List<OperationPattern> GetAllPatterns() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<OperationPattern>("SELECT * FROM OperationPattern WHERE IsDeleted = 0");
                    //db.Table<OperationPattern>().ToList();
            }
        }
        /*
        public static bool CategoryExistByName(string name) {
            using (var db = DbConnection) {
                return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM Category WHERE LOWER(Name) = ? AND IsDeleted = 0", 
                    name.ToLower());
            }
        }

        public static bool SubCategoryExistInBaseByName(string name, string bossCategoryGlobalId) {
            using (var db = DbConnection) {
                return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM SubCategory WHERE LOWER(Name) = ? AND BossCategoryId AND IsDeleted = 0", 
                    name.ToLower(), 
                    bossCategoryGlobalId);
            }
        }
        */
        /*
        public static bool AccountExistInBaseByName(string name, AccountType accountType) {
            using (var db = DbConnection) {
                switch (accountType) {
                    case AccountType.SubAccount:
                        return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM MAccount WHERE BossAccountGlobalId IS NOT NULL AND LOWER(Name) = ?", name.ToLower());
                    case AccountType.Account:
                        return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM MAccount WHERE BossAccountGlobalId ISNULL AND LOWER(Name) = ?", name.ToLower());
                    default:
                        return false;
                }
            }
        }*/
        /*
        public static Category GetDefaultCategory() {
            using (var db = DbConnection) {
                return db.Query<Category>("SELECT * FROM Category WHERE CantDelete LIMIT 1").FirstOrDefault();
            }
        }
        */
        /* GET BY ID */
        /*
        public static SubCategory GetSubCategoryById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM SubCategory WHERE Id == ? AND IsDeleted = 0 LIMIT 1", id).FirstOrDefault();
            }
        }

        public static SubCategory GetSubCategoryByGlobalId(string globalId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM SubCategory WHERE GlobalId == ? AND IsDeleted = 0 LIMIT 1", globalId).FirstOrDefault();
            }
        }

        public static List<SubCategory> GetSubCategoriesByBossId(string globalId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM SubCategory WHERE BossCategoryId == ? AND IsDeleted = 0 ORDER BY Name", globalId);
            }
        }

        public static Category GetCategoryByGlobalId(string globalId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE GlobalId == ? AND IsDeleted = 0 LIMIT 1", globalId)
                    .FirstOrDefault();
            }
        }

        public static Category GetCategoryById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Category WHERE Id == ? AND IsDeleted = 0 LIMIT 1", id)
                    .FirstOrDefault();
            }
        }
        */

        /* SAVE */

        public static void SaveOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                operation.LastModifed = DateTimeHelper.DateTimeUtcNowString;

                //if (operation.DeviceId == null)
                //    operation.DeviceId = Informations.DeviceId;

                if (operation.Id == 0) {
                    //int id = db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'Operation'") + 1;
                    operation.GlobalId = (GetMaxRowId(typeof(Operation)) + 1 ).NewGlobalIdFromLocal();//$"{Informations.DeviceId}_{id}";
                    db.Insert(operation);
                }
                else
                    db.Update(operation);
            }
        }

        
        public static void SaveOperationPattern(OperationPattern operationPattern) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                operationPattern.LastModifed = DateTimeHelper.DateTimeUtcNowString;

                if (operationPattern.Id == 0) {
                    //int id = db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'OperationPattern'") + 1;
                    operationPattern.GlobalId = ( GetMaxRowId(typeof(OperationPattern)) + 1 ).NewGlobalIdFromLocal();//$"{Informations.DeviceId}_{id}";
                    db.Insert(operationPattern);
                }
                else
                    db.Update(operationPattern);
            }
        }
        /*
        public static void UpdateCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                category.LastModifed = DateTimeHelper.DateTimeUtcNowString;

                db.Update(category);
            }
        }

        public static void AddCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                category.LastModifed = DateTimeHelper.DateTimeUtcNowString;

                //int id = db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'Category'") + 1;
                category.GlobalId = ( GetMaxRowId(typeof(Category)) + 1 ).NewGlobalIdFromLocal();//$"{Informations.DeviceId}_{id}";
                db.Insert(category);
            }
        }

        public static void UpdateSubCategory(SubCategory subCategory) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                subCategory.LastModifed = DateTimeHelper.DateTimeUtcNowString;

                db.Update(subCategory);
            }
        }

        public static void AddSubCategory(SubCategory subCategory) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                subCategory.LastModifed = DateTimeHelper.DateTimeUtcNowString;

                //int id = db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'SubCategory'") + 1;
                subCategory.GlobalId = (GetMaxRowId(typeof(SubCategory)) + 1).NewGlobalIdFromLocal();// id.NewGlobalIdFromLocal();//$"{Informations.DeviceId}_{id}";
                db.Insert(subCategory);

                //TODO Insert jak nie ma takiego GlobalId, Update jak jest
                //TODO nie czekaj... no jest kłopot... tak nie mozna 
            }
        }
        */

        /* DELETE */

        public static void DeletePattern(OperationPattern operationPattern) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("UPDATE OperationPattern " +
                           "SET IsDeleted = 1, LastModifed = ? " +
                           "WHERE Id = ?", DateTimeHelper.DateTimeUtcNowString, operationPattern.Id);
            }
        }

        public static void DeleteOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("UPDATE Operation " +
                           "SET IsDeleted = 1, LastModifed = ? " +
                           "WHERE Id = ?", DateTimeHelper.DateTimeUtcNowString, operation.Id);
            }
        }
        /*
        public static void DeleteCategoryWithSubCategories(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                if (db.ExecuteScalar<bool>("SELECT CantDelete FROM Category WHERE Id = ? LIMIT 1", category.Id))
                    return;

                db.Execute("UPDATE Category " +
                           "SET IsDeleted = 1, LastModifed = ? " +
                           "WHERE GlobalId = ?", DateTimeHelper.DateTimeUtcNowString, category.GlobalId); //TODO nie wiem czy usuwanie po ID a nie GlobalId nie jest niebezpieczne np. gdy w tym momencie zacznie sie synchronizować baza, ale chyba nie bo ten id już jest w bazie.

                db.Execute("UPDATE SubCategory " +
                           "SET IsDeleted = 1, LastModifed = ? " +
                           "WHERE BossCategoryId = ?", DateTimeHelper.DateTimeUtcNowString, category.GlobalId);
            }
        }

        public static void DeleteSubCategory(int subCategoryId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                if (db.ExecuteScalar<bool>("SELECT CantDelete FROM SubCategory WHERE Id = ? LIMIT 1", subCategoryId))
                    return;

                db.Execute("UPDATE SubCategory " +
                           "SET IsDeleted = 1, LastModifed = ? " +
                           "WHERE Id = ?", DateTimeHelper.DateTimeUtcNowString, subCategoryId);
            }
        }
        */
        public static int GetMaxRowId(Type type) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
               return db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = ?", type.Name);
            }
        }

        public static int GetMaxRowId(string tableName) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = ?", tableName);
            }
        }
    }
}