using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Finanse.Models;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;

namespace Finanse.DataAccessLayer {
    using Models.Categories;
    using Models.Helpers;
    using Models.MoneyAccounts;
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
                if (db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'Categories'") == 1) {
                    CategoriesDal.AddCategory(new Category { Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true, GlobalId = "2" });
                    CategoriesDal.AddCategory(new Category { Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "3" });
                    CategoriesDal.AddCategory(new Category { Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "4" });
                    CategoriesDal.AddCategory(new Category { Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true, GlobalId = "5" });
                    CategoriesDal.AddCategory(new Category { Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false, GlobalId = "6" });

                    CategoriesDal.AddCategory(new SubCategory { Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = "4", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "7" });
                    CategoriesDal.AddCategory(new SubCategory { Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = "3", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "8" });
                }

                if (db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'MAccount'") == 0) {
                    MAccountsDal.AddAccount(new MAccount { Name = "Gotówka", ColorKey = "01", GlobalId = "1" });
                    MAccountsDal.AddAccount(new MAccount { Name = "Konto bankowe", ColorKey = "02", GlobalId = "2" });
                    MAccountsDal.AddAccount(new SubMAccount { Name = "Karta", ColorKey = "03", GlobalId = "3", BossAccountGlobalId = "2" });
                }
            }
        }

        public static async void CreateBackup() {
            using (var db = DbConnection) {
                DateTime now = DateTime.UtcNow.AddDays( - Settings.BackupFrequency);
                var yco = await BackupHelper.ListOfBackupDates();//ListOfBackupDates();
                if (yco == null || !yco.Any(i => DateTime.ParseExact(i, "yyyy-MM-dd_HH-mm-ss", null) > now))
                    await db.BackupDatabase(new SQLitePlatformWinRT(), "Backup");
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

        public static void RepairDb() {
            using (var db = DbConnection) {
                
                db.Execute("UPDATE Categories SET CantDelete = 1, GlobalId = '1' WHERE Id = 1");

                if (db.ExecuteScalar<int>("SELECT COUNT(*) FROM Categories WHERE Id = 1 LIMIT 1") == 0) {
                    db.Execute("INSERT INTO Categories (Id, GlobalId) VALUES (1, 1)");
                    CategoriesDal.UpdateCategory(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true, CantDelete = true, GlobalId = "1", IsDeleted = false});
                }

                db.Execute("UPDATE Operation SET LastModifed = SUBSTR(LastModifed,1,11) || REPLACE(SUBSTR(LastModifed,12),'.',':') WHERE LastModifed IS NOT NULL AND SUBSTR(LastModifed,14,1) = '.'");
                db.Execute("UPDATE Categories SET IsDeleted = 1 WHERE Id != 1 AND Name = 'Inne' AND CantDelete = 1");
            }
        }

        private static async Task<IEnumerable<string>> ListOfBackupDates() {
            ApplicationData.Current?.LocalFolder.CreateFolderAsync("Backup", CreationCollisionOption.FailIfExists);
            StorageFolder backupFolder = await ApplicationData.Current?.LocalFolder.GetFolderAsync("Backup");

            if (backupFolder == null)
                return null;

            var backupFiles = await backupFolder.GetFilesAsync();

            return backupFiles?
                .Select(i => i.Name.Substring(i.Name.IndexOf('.') + 1))
                .AsEnumerable();
        }

        public static void ConvertAccounts() {
            using (var db = DbConnection) {

                var bankAccountsTable =
                    db.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE name='BankAccount'");
                if (!string.IsNullOrEmpty(bankAccountsTable))
                    foreach (var bankAccount in db.Table<BankAccount>()) {
                        Account account =
                            db.Query<BankAccount>("SELECT * FROM MAccount WHERE Id = ? LIMIT 1", bankAccount.Id)
                                .FirstOrDefault();
                        if (account == null)
                            db.Execute("INSERT INTO MAccount (Id, Name, ColorKey, IsDeleted) VALUES(?, ?, ?, 0)",
                                bankAccount.Id, bankAccount.Name, bankAccount.ColorKey);
                    }

                var cashAccountsTable =
                    db.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE name='BankAccount'");
                if (!string.IsNullOrEmpty(cashAccountsTable))
                    foreach (var cashAccount in db.Table<CashAccount>()) {
                        Account account =
                            db.Query<CashAccount>("SELECT * FROM MAccount WHERE Id = ? LIMIT 1", cashAccount.Id)
                                .FirstOrDefault();
                        if (account == null)
                            db.Execute("INSERT INTO MAccount (Id, Name, ColorKey, IsDeleted) VALUES(?, ?, ?, 0)",
                                cashAccount.Id, cashAccount.Name, cashAccount.ColorKey);
                    }

                var cardAccountsTable =
                    db.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE name='BankAccount'");
                if (!string.IsNullOrEmpty(cardAccountsTable))
                    foreach (var cardAccount in db.Table<CardAccount>()) {
                        Account account =
                            db.Query<CardAccount>("SELECT * FROM MAccount WHERE Id = ? LIMIT 1", cardAccount.Id)
                                .FirstOrDefault();
                        if (account == null)
                            db.Execute(
                                "INSERT INTO MAccount (Id, Name, ColorKey, BossAccountGlobalId, IsDeleted) VALUES(?, ?, ?, ?, 0)",
                                cardAccount.Id, cardAccount.Name, cardAccount.ColorKey, cardAccount.BankAccountId);
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
    }
}