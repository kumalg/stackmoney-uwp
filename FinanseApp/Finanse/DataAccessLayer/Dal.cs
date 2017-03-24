using System.Threading.Tasks;
using Finanse.Dialogs;
using Finanse.Models.Operations;
using Microsoft.Toolkit.Uwp.Services.OneDrive;

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

    public class Dal : DalBase{

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
                return db.Query<Operation>("SELECT * FROM Operation WHERE Date IS NOT NULL AND Date != '' AND IsDeleted = 0 ORDER BY Date LIMIT 1").FirstOrDefault();
            }
        }

        internal static List<Operation> GetAllOperationsFromRangeToStatistics(DateTime minDate, DateTime maxDate) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Operation>("SELECT * FROM Operation WHERE (VisibleInStatistics OR VisibleInStatistics ISNULL) AND Date >= ? AND Date <= ? AND IsDeleted = 0", minDate.ToString("yyyy.MM.dd"), maxDate.ToString("yyyy.MM.dd"));
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



        public static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategoriesInExpenses()
            => GetCategoriesWithSubCategories("VisibleInExpenses");

        public static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategoriesInIncomes()
            => GetCategoriesWithSubCategories("VisibleInIncomes");

        private static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategories(string visibleIn) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var subCategoriesGroups = from subCategory in db.Query<SubCategory>("SELECT * FROM SubCategory WHERE " + visibleIn  + " ORDER BY Name")
                                          group subCategory by subCategory.BossCategoryId into g
                                          select new {
                                              BossCategoryId = g.Key,
                                              subCategories = g.ToList()
                                          };

                return from category in db.Query<Category>("SELECT * FROM Category WHERE " + visibleIn + " ORDER BY Name")
                       join subCategories in subCategoriesGroups on category.Id equals subCategories.BossCategoryId into gj
                       from secondSubCategories in gj.DefaultIfEmpty()
                       select new CategoryWithSubCategories {
                           Category = category,
                           SubCategories =
                               secondSubCategories == null
                                   ? null
                                   : new ObservableCollection<SubCategory>(secondSubCategories.subCategories)
                       };
            }
        }



        public static decimal GetBalanceOfCertainDay(DateTime dateTime) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>(
                    "SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) FROM Operation " +
                    "WHERE Date <= ? AND Date IS NOT NULL AND Date IS NOT '' AND IsDeleted = 0", 
                    dateTime.ToString("yyyy.MM.dd"));
            }
        }

        public static List<Operation> GetAllOperations(DateTime actualMonth, HashSet<int> visiblePayFormList) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                
                var list = db.Query<Operation>(
                    "SELECT * FROM Operation WHERE Date GLOB ? AND Date <= ? AND IsDeleted = 0", 
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
                    "SELECT * FROM Operation WHERE Date > ? OR Date IS NULL OR Date == '' AND IsDeleted = 0", 
                    DateTime.Today.ToString("yyyy.MM.dd"));
            }
        }

        public static List<Operation> GetAllFutureOperations(HashSet<int> visiblePayFormList) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var list = db.Query<Operation>(
                    "SELECT * FROM Operation WHERE Date > ? OR Date IS NULL OR Date == '' AND IsDeleted = 0", 
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

        public static bool CategoryExistByName(string name) {
            using (var db = DbConnection) {
                return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM Category WHERE LOWER(Name) = ?", 
                    name.ToLower());
            }
        }

        public static bool SubCategoryExistInBaseByName(string name, int bossCategoryId) {
            using (var db = DbConnection) {
                return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM SubCategory WHERE LOWER(Name) = ? AND BossCategoryId", 
                    name.ToLower(), 
                    bossCategoryId);
            }
        }

        public static bool AccountExistInBaseByName(string name, AccountType accountType) {
            using (var db = DbConnection) {
                switch (accountType) {
                    case AccountType.BankAccount:
                        return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM BankAccount WHERE LOWER(Name) = ?", name.ToLower());
                    case AccountType.CardAccount:
                        return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM CardAccount WHERE LOWER(Name) = ?", name.ToLower());
                    case AccountType.CashAccount:
                        return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM CashAccount WHERE LOWER(Name) = ?", name.ToLower());
                    default:
                        return false;
                }
            }
        }

        /* GET BY ID */

        public static SubCategory GetSubCategoryById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM SubCategory WHERE Id == ? LIMIT 1", id).FirstOrDefault();
            }
        }

        public static List<SubCategory> GetSubCategoriesByBossId(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM SubCategory WHERE BossCategoryId == ? ORDER BY Name", id);
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
                operation.LastModifed = DateHelper.ActualTimeString;

                if (operation.DeviceId == null)
                    operation.DeviceId = Settings.DeviceId;

                if (operation.Id == 0) {
                    operation.RemoteId = db.ExecuteScalar<int>("SELECT seq " +
                                                               "FROM sqlite_sequence " +
                                                               "WHERE name = 'Operation'") + 1;
                    db.Insert(operation);
                }
                else
                    db.Update(operation);
            }
        }

        private static void InsertOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(operation);
            }
        }
        private static void UpdateOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(operation);
            }
        }

        public static void SaveOperationPattern(OperationPattern operationPattern) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                operationPattern.LastModifed = DateHelper.ActualTimeString;

                if (operationPattern.Id == 0) {
                    operationPattern.RemoteId = db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'OperationPattern'") + 1;
                    db.Insert(operationPattern);
                }
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
                db.Execute("UPDATE OperationPattern " +
                           "SET IsDeleted = 1, LastModifed = ? " +
                           "WHERE Id = ?", DateHelper.ActualTimeString, operationPattern.Id);
            }
        }

        public static void DeleteOperation(Operation operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("UPDATE Operation " +
                           "SET IsDeleted = 1, LastModifed = ? " +
                           "WHERE Id = ?", DateHelper.ActualTimeString, operation.Id);
            }
        }

        public static void DeleteCategoryWithSubCategories(int categoryId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                if (db.ExecuteScalar<bool>("SELECT CantDelete FROM Category WHERE Id = ? LIMIT 1", categoryId))
                    return;

                db.Execute("DELETE FROM Category WHERE Id = ?", categoryId);
                db.Execute("DELETE FROM SubCategory WHERE BossCategoryId = ?", categoryId);
            }
        }

        public static void DeleteSubCategory(int subCategoryId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                if (db.ExecuteScalar<bool>("SELECT CantDelete FROM SubCategory WHERE Id = ? LIMIT 1", subCategoryId))
                    return;

                db.Execute("DELETE FROM SubCategory WHERE Id = ?", subCategoryId);
            }
        }

        public static void UpdateBase(string localFileName) {
            IEnumerable<Operation> oneDriveOperations;
            IEnumerable<Operation> localOperations;

            using (var db = DbConnection) {
                localOperations = db.Table<Operation>().ToList();
            }
            using ( var dbOneDrive = new SQLiteConnection(new SQLitePlatformWinRT(), DBPathLocalFromFileName(localFileName)) ) {
                oneDriveOperations = dbOneDrive.Table<Operation>().ToList();
            }


            foreach (var oneDriveOperation in oneDriveOperations) {
                if (string.IsNullOrEmpty(oneDriveOperation.DeviceId))
                    continue;

                Operation localOperation =
                                    localOperations.FirstOrDefault(
                                        item =>
                                            item.RemoteId == oneDriveOperation.RemoteId &&
                                            item.DeviceId == oneDriveOperation.DeviceId);

                if (localOperation == null) {
                    InsertOperation(oneDriveOperation);
                    continue;
                }

                if (localOperation.LastModifed != null &&
                    DateTime.Parse(localOperation.LastModifed) >= DateTime.Parse(oneDriveOperation.LastModifed))
                    continue;

                if (oneDriveOperation.LastModifed == null)
                    oneDriveOperation.LastModifed = DateHelper.ActualTimeString;

                UpdateOperation(oneDriveOperation);
            }
        }
    }
}