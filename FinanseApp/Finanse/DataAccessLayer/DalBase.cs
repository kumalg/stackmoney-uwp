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

    internal class DalBase {
        private static string dbPath = string.Empty;
        protected static string DbPath {
            get {
                if (string.IsNullOrEmpty(dbPath))
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");
                return dbPath;
            }
        }

        protected static SQLiteConnection DbConnection => new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);

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

          //      db.Execute("ALTER TABLE OperationCategory RENAME TO Category");
            //    db.Execute("ALTER TABLE OperationSubCategory RENAME TO SubCategory");

                db.CreateTable<Operation>();
                db.CreateTable<OperationPattern>();
                db.CreateTable<Category>();
                db.CreateTable<SubCategory>();
                db.CreateTable<CashAccount>();
                db.CreateTable<CardAccount>();
                db.CreateTable<BankAccount>();

                db.Execute("INSERT INTO sqlite_sequence (name, seq) SELECT 'Account', 0 WHERE NOT EXISTS(SELECT 1 FROM sqlite_sequence WHERE name = 'Account')");

                if (!(db.Table<Category>().Any())) {
                    db.Insert(new Category { Id = 1, Name = "Inne", ColorKey = "14", IconKey = "FontIcon_2", VisibleInIncomes = true, VisibleInExpenses = true });
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