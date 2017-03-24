using System.Threading.Tasks;
using Finanse.Models.Operations;

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

    public class DalBase {
        public static string dbOneDriveName = "dbOneDrive.sqlite";
        private static string dbPath = string.Empty;
        public static string DbPath {
            get {
                if (string.IsNullOrEmpty(dbPath))
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");
                return dbPath;
            }
        }

        public static string DBPathLocalFromFileName(string fileName) => Path.Combine(ApplicationData.Current.LocalFolder.Path, fileName);

        protected static SQLiteConnection DbConnection => new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);

        protected static SQLiteConnection DbConnectionFromPath(string path) => new SQLiteConnection(new SQLitePlatformWinRT(), path);

        public static void CreateDb() {

            using (var db = DbConnection) {
                // Activate Tracing
                db.Execute("PRAGMA foreign_keys = ON");
                db.TraceListener = new DebugTraceListener();

                /*
                db.Execute("CREATE TABLE IF NOT EXISTS images ( "
                    + "nameRed VARCHAR(20) NOT NULL PRIMARY KEY,"
                    + "patientID INT,"
                    + "FOREIGN KEY(patientID) REFERENCES patients(id) ) ");*/

                // db.CreateTable<MoneyAccount>();

                var OperationCategory =
                    db.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE name='OperationCategory'");
                if (!string.IsNullOrEmpty(OperationCategory))
                    db.Execute("ALTER TABLE OperationCategory RENAME TO Category");

                var OperationSubCategory =
                    db.ExecuteScalar<string>("SELECT name FROM sqlite_master WHERE name='OperationSubCategory'");
                if (!string.IsNullOrEmpty(OperationSubCategory))
                    db.Execute("ALTER TABLE OperationSubCategory RENAME TO SubCategory");

                db.CreateTable<Operation>();
                db.CreateTable<OperationPattern>();
                db.CreateTable<Category>();
                db.CreateTable<SubCategory>();
                db.CreateTable<CashAccount>();
                db.CreateTable<CardAccount>();
                db.CreateTable<BankAccount>();

                db.Execute("UPDATE Operation SET RemoteId = Id WHERE RemoteId ISNULL");
                db.Execute("UPDATE Operation SET DeviceId = ? WHERE DeviceId ISNULL", Settings.DeviceId);
                db.Execute("UPDATE Operation SET LastModifed = ? WHERE DeviceId ISNULL", Settings.DeviceId);
                db.Execute("UPDATE Operation SET IsDeleted = 0 WHERE IsDeleted ISNULL");

                db.Execute("UPDATE Category SET CantDelete = 1 WHERE Id = 1");
                db.Execute("UPDATE Category SET CantDelete = 0 WHERE CantDelete ISNULL");
                db.Execute("UPDATE SubCategory SET CantDelete = 0 WHERE CantDelete ISNULL");

                db.Execute(AccountQueries.SeqTriggerCashAccount);
                db.Execute(AccountQueries.SeqTriggerBankAccount);
                db.Execute(AccountQueries.SeqTriggerCardAccount);

                if (!db.ExecuteScalar<bool>("SELECT * FROM Category WHERE Id = 1 LIMIT 1")) {
                    db.Execute("INSERT INTO Category (Id) VALUES (1)");
                    db.Update(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true, CantDelete = true });
                }

                db.Execute("INSERT INTO sqlite_sequence (name, seq) SELECT 'Account', 0 WHERE NOT EXISTS(SELECT 1 FROM sqlite_sequence WHERE name = 'Account')");

                db.Execute("UPDATE Operation SET LastModifed = SUBSTR(LastModifed,1,11) || REPLACE(SUBSTR(LastModifed,12),'.',':')");

                if (db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'Category'") == 1) {
               //     db.Insert(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true, CantDelete = true});
                    db.Insert(new Category { Id = 2, Name = "Jedzenie", ColorKey = "04", IconKey = "FontIcon_6", VisibleInExpenses = true, VisibleInIncomes = true });
                    db.Insert(new Category { Id = 3, Name = "Rozrywka", ColorKey = "12", IconKey = "FontIcon_20", VisibleInIncomes = false, VisibleInExpenses = true });
                    db.Insert(new Category { Id = 4, Name = "Rachunki", ColorKey = "08", IconKey = "FontIcon_21", VisibleInIncomes = false, VisibleInExpenses = true });
                    db.Insert(new Category { Id = 5, Name = "Prezenty", ColorKey = "05", IconKey = "FontIcon_13", VisibleInIncomes = true, VisibleInExpenses = true });
                    db.Insert(new Category { Id = 6, Name = "Praca", ColorKey = "14", IconKey = "FontIcon_9", VisibleInIncomes = true, VisibleInExpenses = false});

                    db.Insert(new SubCategory { Id = 1, Name = "Prąd", ColorKey = "07", IconKey = "FontIcon_19", BossCategoryId = 4, VisibleInIncomes = false, VisibleInExpenses = true });
                    db.Insert(new SubCategory { Id = 2, Name = "Imprezy", ColorKey = "11", IconKey = "FontIcon_17", BossCategoryId = 3, VisibleInIncomes = false, VisibleInExpenses = true });
                }

                if (!(db.Table<CashAccount>().Any() || db.Table<BankAccount>().Any())) {
                    AccountsDal.AddAccount(new CashAccount { Name = "Gotówka", ColorKey = "01" });
                    AccountsDal.AddAccount(new BankAccount { Name = "Konto bankowe", ColorKey = "02", });
                    AccountsDal.AddAccount(new CardAccount { Name = "Karta", ColorKey = "03", BankAccountId = db.ExecuteScalar<int>("SELECT Id FROM BankAccount LIMIT 1")});
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