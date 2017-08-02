using System;
using Finanse.Models;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;
using SQLite.Net.Async;
using SQLite.Net.Interop;

namespace Finanse.DataAccessLayer {
    using Models.Categories;
    using Models.Helpers;
    using SQLite.Net;
    using SQLite.Net.Platform.WinRT;
    using System.IO;
    using System.Linq;
    using Windows.Storage;

    public class DalBase {
        public static string DbOneDriveName = "dbOneDrive.sqlite";
        private static string _dbPath = string.Empty;
        public static string DbPath {
            get {
                if (string.IsNullOrEmpty(_dbPath))
                    _dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");
                return _dbPath;
            }
        }

        public static string DbPathLocalFromFileName(string fileName) => Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName);

        protected static SQLiteConnection DbConnection => new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);

        public static SQLiteAsyncConnection GetConnection(string path, ISQLitePlatform sqlitePlatform) {
            var connectionFactory = new Func<SQLiteConnectionWithLock>(() => new SQLiteConnectionWithLock(sqlitePlatform, new SQLiteConnectionString(path, storeDateTimeAsTicks: false)));
            return new SQLiteAsyncConnection(connectionFactory);
        }

        public static SQLiteAsyncConnection DbAsyncConnection {
            get {
                var connectionFactory = new Func<SQLiteConnectionWithLock>(() => new SQLiteConnectionWithLock(new SQLitePlatformWinRT(), new SQLiteConnectionString(DbPath, storeDateTimeAsTicks: false)));
                return new SQLiteAsyncConnection(connectionFactory);
            }
        }

        protected static SQLiteConnection DbConnectionFromPath(string path) => new SQLiteConnection(new SQLitePlatformWinRT(), path);

        public static void CreateDb() {

            using (var db = DbConnection) {
                // Activate Tracing
                db.Execute("PRAGMA foreign_keys = ON");
                db.TraceListener = new DebugTraceListener();

                db.CreateTable<Operation>();
                db.CreateTable<OperationPattern>();
                db.CreateTable<MAccount>();

                db.Execute(CategoriesDal.CreateCategoriesTableQuery);
                
                var columnExists = db.ExecuteScalar<string>("SELECT sql FROM sqlite_master where name = 'MAccount' and sql like('%BossAccountGlobalId%')");
                if (string.IsNullOrEmpty(columnExists))
                    db.Execute("ALTER TABLE MAccount ADD COLUMN BossAccountGlobalId varchar");
            }
        }

        public static void AddItemsIfEmpty() {
            using (var db = DbConnection) {
                if (db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'Categories'") == 0) {
                    CategoriesDal.AddCategory(new Category { Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", LastModifed = "2017.01.01 00:00:00", VisibleInIncomes = true, VisibleInExpenses = true, CantDelete = true, GlobalId = "1"});
                    CategoriesDal.AddCategory(new Category { Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", LastModifed = "2017.01.01 00:00:00", VisibleInExpenses = true, VisibleInIncomes = true, GlobalId = "2" });
                    CategoriesDal.AddCategory(new Category { Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", LastModifed = "2017.01.01 00:00:00", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "3" });
                    CategoriesDal.AddCategory(new Category { Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", LastModifed = "2017.01.01 00:00:00", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "4" });
                    CategoriesDal.AddCategory(new Category { Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", LastModifed = "2017.01.01 00:00:00", VisibleInIncomes = true, VisibleInExpenses = true, GlobalId = "5" });
                    CategoriesDal.AddCategory(new Category { Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", LastModifed = "2017.01.01 00:00:00", VisibleInIncomes = true, VisibleInExpenses = false, GlobalId = "6" });

                    CategoriesDal.AddCategory(new SubCategory { Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", LastModifed = "2017.01.01 00:00:00", BossCategoryId = "4", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "7" });
                    CategoriesDal.AddCategory(new SubCategory { Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", LastModifed = "2017.01.01 00:00:00", BossCategoryId = "3", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "8" });
                }

                if (db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'MAccount'") == 0) {
                    MAccountsDal.AddAccount(new MAccount { Name = "Gotówka", ColorKey = "01", LastModifed = "2017.01.01 00:00:00", GlobalId = "1" });
                    MAccountsDal.AddAccount(new MAccount { Name = "Konto bankowe", ColorKey = "02", LastModifed = "2017.01.01 00:00:00", GlobalId = "2" });
                    MAccountsDal.AddAccount(new SubMAccount { Name = "Karta", ColorKey = "03", LastModifed = "2017.01.01 00:00:00", GlobalId = "3", BossAccountGlobalId = "2" });
                }
            }
        }

        public static async void CreateBackup() {
            using (var db = DbConnection) {
                DateTime now = DateTime.UtcNow.AddDays( - Settings.BackupFrequency);
                var yco = await BackupHelper.ListOfBackupDates();
                if (yco == null || !yco.Any(i => DateTime.ParseExact(i, "yyyy-MM-dd_HH-mm-ss", null) > now))
                    db.BackupDatabase(new SQLitePlatformWinRT(), "Backup");
            }
        }

        public static void ConvertCategories() {
            using (var db = DbConnection) {
                var columnExists = db.ExecuteScalar<string>("SELECT sql FROM sqlite_master where name = 'Operation' and sql like('%SubCategoryId%')");
                if (string.IsNullOrEmpty(columnExists))
                    return;

                db.Execute(
                    "UPDATE Operation SET SubCategoryId = SubCategoryId || 'xxx' WHERE SubCategoryId IN(SELECT GlobalId FROM Category)");
                db.Execute(
                    "UPDATE OperationPattern SET SubCategoryId = SubCategoryId || 'xxx' WHERE SubCategoryId IN(SELECT GlobalId FROM Category)");
                db.Execute(
                    "UPDATE SubCategory SET GlobalId = GlobalId || 'xxx' WHERE GlobalId IN(SELECT GlobalId FROM Category)");

                db.Execute(
                    "UPDATE Operation SET CategoryGlobalId = (CASE WHEN SubCategoryId IN('-1','') THEN CategoryId ELSE SubCategoryId END) WHERE CategoryGlobalId ISNULL OR CategoryGlobalId = ''");
                db.Execute(
                    "UPDATE OperationPattern SET CategoryGlobalId = (CASE WHEN SubCategoryId IN('-1','') THEN CategoryId ELSE SubCategoryId END) WHERE CategoryGlobalId ISNULL OR CategoryGlobalId = ''");

                db.Execute(
                    "INSERT INTO Categories(Name, VisibleInIncomes, VisibleInExpenses, ColorKey, IconKey, CantDelete, LastModifed, IsDeleted, GlobalId) " +
                    "SELECT Name, VisibleInIncomes, VisibleInExpenses, ColorKey, IconKey, CantDelete, LastModifed, IsDeleted, GlobalId " +
                    "FROM Category WHERE GlobalId NOT IN(SELECT GlobalId FROM Categories)");

                db.Execute(
                    "INSERT INTO Categories(Name, BossCategoryId, VisibleInIncomes, VisibleInExpenses, ColorKey, IconKey, CantDelete, LastModifed, IsDeleted, GlobalId) " +
                    "SELECT Name, BossCategoryId, VisibleInIncomes, VisibleInExpenses, ColorKey, IconKey, CantDelete, LastModifed, IsDeleted, GlobalId " +
                    "FROM SubCategory WHERE GlobalId NOT IN(SELECT GlobalId FROM Categories)");
            }
        }

        public static void CheckDefaultCategory() {
            using (var db = DbConnection) {
                db.Execute("DELETE FROM Categories WHERE GlobalId = '1' AND Id != (SELECT Id FROM Categories WHERE GlobalId = '1' LIMIT 1)");

                db.Execute("INSERT INTO Categories (GlobalId) " +
                           "SELECT '1' WHERE NOT EXISTS (SELECT 1 FROM Categories WHERE GlobalId = '1' LIMIT 1)");

                db.Execute("UPDATE Categories " +
                           "SET Name = 'Inne', ColorKey = '14', IconKey = 'FontIcon_2', LastModifed = '2017.01.01 00:00:00', VisibleInIncomes = 1, VisibleInExpenses = 1, CantDelete = 1, IsDeleted = 0 " +
                           "WHERE GlobalId = '1'");
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
    }
}