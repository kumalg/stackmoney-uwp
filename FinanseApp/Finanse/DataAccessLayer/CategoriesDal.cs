using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Finanse.Models.Categories;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using Finanse.Models.Extensions;
using Finanse.Models.Helpers;

namespace Finanse.DataAccessLayer {
    public class CategoriesDal : DalBase {

        public static string CreateCategoriesTableQuery = "CREATE TABLE IF NOT EXISTS \"Categories\" ( `Id` integer NOT NULL PRIMARY KEY AUTOINCREMENT, " +
                                                                                        "`Name` varchar, " +
                                                                                        "`BossCategoryId` varchar, " +
                                                                                        "`VisibleInIncomes` integer, " +
                                                                                        "`VisibleInExpenses` integer, " +
                                                                                        "`ColorKey` varchar, " +
                                                                                        "`IconKey` varchar, " +
                                                                                        "`CantDelete` integer, " +
                                                                                        "`LastModifed` varchar, " +
                                                                                        "`IsDeleted` integer, " +
                                                                                        "`GlobalId` varchar )";

        public static IEnumerable<Category> GetAllCategories() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<Category>("SELECT * FROM Categories WHERE BossCategoryId ISNULL AND IsDeleted = 0").OrderBy(i => i.Name);
            }
        }

        public static IEnumerable<SubCategory> GetAllSubCategories() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM Categories WHERE BossCategoryId IS NOT NULL AND IsDeleted = 0").OrderBy(i => i.Name);
            }
        }

        public static IEnumerable<Category> GetAllCategoriesAndSubCategories() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                var accounts = db.Query<Category>("SELECT * FROM Categories WHERE BossCategoryId ISNULL AND IsDeleted = 0");
                accounts.AddRange(db.Query<SubCategory>("SELECT * FROM Categories WHERE BossCategoryId IS NOT NULL AND IsDeleted = 0"));
                return accounts.OrderBy(i => i.Name);
            }
        }

        public static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategoriesInExpenses()
            => GetCategoriesWithSubCategories("VisibleInExpenses");

        public static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategoriesInIncomes()
            => GetCategoriesWithSubCategories("VisibleInIncomes");

        private static IEnumerable<CategoryWithSubCategories> GetCategoriesWithSubCategories(string visibleIn) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var subCategoriesGroups = from subCategory in db.Query<SubCategory>("SELECT * FROM Categories WHERE " + visibleIn + " AND IsDeleted = 0 AND BossCategoryId IS NOT NULL ORDER BY Name")
                    group subCategory by subCategory.BossCategoryId into g
                    select new {
                        BossCategoryId = g.Key,
                        subCategories = g.AsEnumerable()
                    };

                return from category in db.Query<Category>("SELECT * FROM Categories WHERE " + visibleIn + " AND IsDeleted = 0 AND BossCategoryId ISNULL ORDER BY Name")
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

        public static IEnumerable<SubCategory> GetSubCategoriesByBossId(string globalId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubCategory>("SELECT * FROM Categories WHERE BossCategoryId == ? AND IsDeleted = 0 ORDER BY Name", globalId);
            }
        }



        public static Category GetDefaultCategory() {
            using (var db = DbConnection) {
                return db.Query<Category>("SELECT * FROM Categories WHERE CantDelete LIMIT 1").FirstOrDefault();
            }
        }

        public static Category GetCategoryByGlobalId(string globalId) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                var item = db.Query<SubCategory>("SELECT * FROM Categories WHERE GlobalId = ? AND IsDeleted = 0 LIMIT 1", globalId).FirstOrDefault();

                if (item != null && string.IsNullOrEmpty(item.BossCategoryId))
                    return item.ToCategory();
                return item;
            }
        }

        public static Category GetCategoryByGlobalId(string globalId, SQLiteConnection db) {
          //  using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                var item = db.Query<SubCategory>("SELECT * FROM Categories WHERE GlobalId = ? AND IsDeleted = 0 LIMIT 1", globalId).FirstOrDefault();

                if (item != null && string.IsNullOrEmpty(item.BossCategoryId))
                    return item.ToCategory();
                return item;
          //  }
        }



        public static bool CategoryExistInBaseByName(string name) {
            using (var db = DbConnection) {
                return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM Categories WHERE LOWER(TRIM(Name)) = ? AND IsDeleted = 0", name.Trim().ToLower());
            }
        }

        public static bool SubCategoryExistInBaseByName(string name, string bossCategoryGlobalId) {
            using (var db = DbConnection) {
                return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM Categories WHERE LOWER(Name) = ? AND BossCategoryId = ? AND IsDeleted = 0", name.ToLower(), bossCategoryGlobalId);
            }
        }

        

        public static void AddCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                object[] parameters = {
                    category.Name,
                    category.VisibleInIncomes,
                    category.VisibleInExpenses,
                    category.ColorKey,
                    category.IconKey,
                    false,
                    DateTimeHelper.DateTimeUtcNowString,
                    false,
                    category.GlobalId ?? (Dal.GetMaxRowId("Categories") + 1).NewGlobalIdFromLocal(),
                    ( category as SubCategory )?.BossCategoryId
                };

                db.Execute(
                    "INSERT INTO Categories (Name, VisibleInIncomes, VisibleInExpenses, ColorKey, IconKey, CantDelete, LastModifed, IsDeleted, GlobalId, BossCategoryId) VALUES(?,?,?,?,?,?,?,?,?,?)"
                        , parameters);

                category.LastModifed = parameters[6] as string;
                category.GlobalId = parameters[8] as string;
                if (category is SubCategory)
                    ((SubCategory)category).BossCategoryId = parameters[9] as string;
            }
        }
        
        public static void UpdateCategory(Category category) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                object[] parameters = {
                    category.Name,
                    category.VisibleInIncomes,
                    category.VisibleInExpenses,
                    category.ColorKey,
                    category.IconKey,
                    DateTimeHelper.DateTimeUtcNowString,
                    ( category as SubCategory )?.BossCategoryId,
                    category.CantDelete,
                    category.IsDeleted,
                    category.GlobalId
                };

                db.Execute(
                    "UPDATE Categories SET Name = ?, VisibleInIncomes = ?, VisibleInExpenses = ?, ColorKey = ?, IconKey = ?, LastModifed = ?, BossCategoryId = ?, CantDelete = ?, IsDeleted = ? WHERE GlobalId = ?"
                        , parameters);
            }
        }

        public static void RemoveSubCategoryAndSetOperationsToParentCategory(SubCategory subCategory) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                db.Execute(
                    "UPDATE Operation SET CategoryGlobalId = ?, LastModifed = ? WHERE CategoryGlobalId = ?"
                        , subCategory.BossCategoryId
                        , DateTimeHelper.DateTimeUtcNowString
                        , subCategory.GlobalId);

                db.Execute(
                    "UPDATE Categories SET IsDeleted = 1, LastModifed = ? WHERE GlobalId = ?"
                        , DateTimeHelper.DateTimeUtcNowString
                        , subCategory.GlobalId);
            }
        }

        public static void RemoveCategoryWithSubCategories(Category category) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                db.Execute(
                    "UPDATE Categories SET IsDeleted = 1, LastModifed = ? WHERE ? IN(GlobalId, BossCategoryId)"
                        , DateTimeHelper.DateTimeUtcNowString
                        , category.GlobalId);
            }
        }
    }
}
