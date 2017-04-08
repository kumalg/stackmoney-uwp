using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Finanse.Models;
using Finanse.Models.DateTimeExtensions;
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

                var operationCategory =
                    db.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE name='OperationCategory'");
                if (!string.IsNullOrEmpty(operationCategory))
                    db.Execute("ALTER TABLE OperationCategory RENAME TO Category");

                var operationSubCategory =
                    db.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE name='OperationSubCategory'");
                if (!string.IsNullOrEmpty(operationSubCategory))
                    db.Execute("ALTER TABLE OperationSubCategory RENAME TO SubCategory");

                db.CreateTable<Operation>();
                db.CreateTable<OperationPattern>();
                db.CreateTable<Category>();
                db.CreateTable<SubCategory>();
                db.CreateTable<MAccount>();

                try {
                    db.Execute("ALTER TABLE MAccount ADD COLUMN BossAccountGlobalId varchar");
                }
                catch {}
            }
        }

        public static void AddItemsIfEmpty() {
            using (var db = DbConnection) {
                if (db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'Category'") == 1) {
                    db.Insert(new Category { Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true, GlobalId = "2" });
                    db.Insert(new Category { Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "3" });
                    db.Insert(new Category { Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true, GlobalId = "4" });
                    db.Insert(new Category { Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true, GlobalId = "5" });
                    db.Insert(new Category { Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false, GlobalId = "6" });

                    db.Insert(new SubCategory { Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = "4", VisibleInIncomes = false, VisibleInExpenses = true });
                    db.Insert(new SubCategory { Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = "3", VisibleInIncomes = false, VisibleInExpenses = true });
                }

                if (db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'MAccount'") == 0) {
                    MAccountsDal.AddAccount(new MAccount { Name = "Gotówka", ColorKey = "01", GlobalId = "1" });
                    MAccountsDal.AddAccount(new MAccount { Name = "Konto bankowe", ColorKey = "02", GlobalId = "2" });
                    MAccountsDal.AddAccount(new SubMAccount { Name = "Karta", ColorKey = "03", BossAccountGlobalId = "2" });
                }
            }
        }

        public static async void CreateBackup() {
            using (var db = DbConnection) {
                DateTime now = DateTime.UtcNow.AddDays(1 - Settings.BackupFrequency);
                var yco = await ListOfBackupDates();
                if (yco == null || !yco.Any(i => DateTime.ParseExact(i, "yyyy-MM-dd_HH-mm-ss", null) > now))
                    await db.BackupDatabase(new SQLitePlatformWinRT(), "Backup");
            }
        }

        public static void RepairDb() {
            using (var db = DbConnection) {

                db.Execute("UPDATE Category SET GlobalId = Id WHERE GlobalId ISNULL AND Id < 7");
                db.Execute("UPDATE SubCategory SET GlobalId = Id WHERE GlobalId ISNULL AND Id < 3");
                db.Execute("UPDATE MAccount SET GlobalId = Id WHERE GlobalId ISNULL AND Id < 4");

                CheckSyncColumns(db, "Operation");
                CheckSyncColumns(db, "OperationPattern");
                CheckSyncColumns(db, "Category");
                CheckSyncColumns(db, "SubCategory");
                CheckSyncColumns(db, "MAccount");

                db.Execute("UPDATE Category SET CantDelete = 1, GlobalId = '1' WHERE Id = 1");
                db.Execute("UPDATE Category SET CantDelete = 0 WHERE CantDelete ISNULL");
                db.Execute("UPDATE SubCategory SET CantDelete = 0 WHERE CantDelete ISNULL");

                if (db.ExecuteScalar<int>("SELECT COUNT(*) FROM Category WHERE Id = 1 LIMIT 1") == 0) {
                    //    db.Execute("INSERT INTO Category (Id, Name, ColorKey, IconKey, VisibleInIncomes, VisibleInExpenses, CantDelete, GlobalId, IsDeleted) VALUES (1, 'Inne', '14', 'FontIcon_2', 1, 1, 1, '1', 0)");
                    db.Execute("INSERT INTO Category (Id) VALUES (1)");
                    db.Update(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true, CantDelete = true, GlobalId = "1" });
                }

               // db.Execute("INSERT INTO sqlite_sequence (name, seq) SELECT 'Account', 0 WHERE NOT EXISTS(SELECT 1 FROM sqlite_sequence WHERE name = 'Account')");

                db.Execute("UPDATE Operation SET LastModifed = SUBSTR(LastModifed,1,11) || REPLACE(SUBSTR(LastModifed,12),'.',':') WHERE LastModifed IS NOT NULL AND SUBSTR(LastModifed,14,1) = '.'");
                db.Execute("UPDATE Category SET IsDeleted = 1 WHERE Id != 1 AND Name = 'Inne' AND CantDelete = 1");
            }
        }

        public static void ConvertLocalIdReferenceToGlobal() {
            using (var db = DbConnection) {
                var subMoAccounts = db.Query<SubMAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId IS NOT NULL AND IsDeleted = 0");

                foreach (var subMoAccount in subMoAccounts) {
                    var bossMoAccount =
                        db.Query<MAccount>("SELECT * FROM MAccount WHERE Id = ? LIMIT 1", subMoAccount.BossAccountGlobalId)
                            .FirstOrDefault();

                    if (bossMoAccount != null)
                        db.Execute("UPDATE MAccount SET BossAccountGlobalId = ? WHERE Id = ?", bossMoAccount.GlobalId, subMoAccount.Id);
                }

                var subCategories = db.Query<SubCategory>("SELECT * FROM SubCategory WHERE IsDeleted = 0");

                foreach (var subCategory in subCategories) {
                    var bossCategory =
                        db.Query<Category>("SELECT * FROM Category WHERE Id = ? LIMIT 1", subCategory.BossCategoryId)
                            .FirstOrDefault();

                    if (bossCategory != null)
                        db.Execute("UPDATE SubCategory SET BossCategoryId = ? WHERE Id = ?", bossCategory.GlobalId, subCategory.Id);
                }


                var categories = db.Query<Category>("SELECT * FROM Category WHERE IsDeleted = 0");
                foreach (var category in categories) {
                    db.Execute("UPDATE Operation SET CategoryId = ? WHERE CategoryId = ?", category.GlobalId, category.Id);
                    db.Execute("UPDATE OperationPattern SET CategoryId = ? WHERE CategoryId = ?", category.GlobalId, category.Id);
                }

                subCategories = db.Query<SubCategory>("SELECT * FROM SubCategory WHERE IsDeleted = 0");
                foreach (var subCategory in subCategories) {
                    db.Execute("UPDATE Operation SET SubCategoryId = ? WHERE SubCategoryId = ?", subCategory.GlobalId, subCategory.Id);
                    db.Execute("UPDATE OperationPattern SET SubCategoryId = ? WHERE SubCategoryId = ?", subCategory.GlobalId, subCategory.Id);
                }

                var mAccounts = db.Query<MAccount>("SELECT * FROM MAccount");
                foreach (var mAccount in mAccounts) {
                    db.Execute("UPDATE Operation SET MoneyAccountId = ? WHERE MoneyAccountId = ?", mAccount.GlobalId, mAccount.Id);
                    db.Execute("UPDATE OperationPattern SET MoneyAccountId = ? WHERE MoneyAccountId = ?", mAccount.GlobalId, mAccount.Id);
                }
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

        private static void CheckSyncColumns(SQLiteConnection db, string tableName) {
                db.Execute("UPDATE " + tableName + " SET GlobalId = ? || '_' || Id || '_' || ? WHERE GlobalId ISNULL", Informations.DeviceId, DateTime.UtcNow.GetTimestamp());
                db.Execute("UPDATE " + tableName + " SET LastModifed = ? WHERE LastModifed ISNULL", DateTimeHelper.DateTimeUtcNowString);
                db.Execute("UPDATE " + tableName + " SET IsDeleted = 0 WHERE IsDeleted ISNULL");
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