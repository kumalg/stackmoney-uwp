using Finanse.Models;
using Finanse.Models.MoneyAccounts;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Windows.Storage;

namespace Finanse.DataAccessLayer {
    internal static class AccountsDal {
        private static string dbPath = string.Empty;
        private static string DbPath {
            get {
                if (string.IsNullOrEmpty(dbPath))
                    dbPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "db.sqlite");
                return dbPath;
            }
        }
        private static SQLiteConnection DbConnection {
            get {
                return new SQLiteConnection(new SQLitePlatformWinRT(), DbPath);
            }
        }


        /* GET BY ID */

        internal static int getHighestIdOfAccounts() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'Account' LIMIT 1");
            }
        }

        public static decimal BankAccountBalanceById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>("SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) FROM Operation WHERE MoneyAccountId = ? AND Date IS NOT NULL AND Date IS NOT '' AND Date <= ?", id, DateTime.Today.Date.ToString("yyyy.MM.dd"));
            }
        }

        public static decimal CashAccountBalanceById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>("SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) FROM Operation WHERE MoneyAccountId = ? AND Date IS NOT NULL AND Date IS NOT '' AND Date <= ?", id, DateTime.Today.Date.ToString("yyyy.MM.dd"));
            }
        }

        public static decimal CardAccountExpensesById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>("SELECT TOTAL(-Cost) FROM Operation WHERE MoneyAccountId = ? AND isExpense AND Date IS NOT NULL AND Date IS NOT '' AND Date <= ?", id, DateTime.Today.Date.ToString("yyyy.MM.dd"));
            }
        }

        public static Account getAccountById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                CashAccount cashAccount = db.Query<CashAccount>("SELECT * FROM CashAccount WHERE Id == ? LIMIT 1", id).FirstOrDefault();
                BankAccount bankAccount = db.Query<BankAccount>("SELECT * FROM BankAccount WHERE Id == ? LIMIT 1", id).FirstOrDefault();
                CardAccount cardAccount = db.Query<CardAccount>("SELECT * FROM CardAccount WHERE Id == ? LIMIT 1", id).FirstOrDefault();

                if (cashAccount != null)
                    return cashAccount;
                else if (bankAccount != null)
                    return bankAccount;
                else
                    return cardAccount;
            }
        }


        /* GET ALL */

        public static List<Account> getAllAccounts() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                List<Account> accounts = new List<Account>();

                var cashAccounts = db.Query<CashAccount>("SELECT * FROM CashAccount");
                var bankAccounts = db.Query<BankAccount>("SELECT * FROM BankAccount");
                var cardAccounts = db.Query<CardAccount>("SELECT * FROM CardAccount");
                var cardAccountsGrouped = from cardAccount in cardAccounts
                                          group cardAccount by cardAccount.BankAccountId into g
                                          select new {
                                              cards = g,
                                              BankAccountId = g.Key
                                          };

                foreach (CashAccount cashAccount in cashAccounts)
                    accounts.Add(cashAccount);

                foreach (BankAccount bankAccount in bankAccounts) {
                    var cardsQuery = cardAccountsGrouped.SingleOrDefault(i => i.BankAccountId == bankAccount.Id);
                    BankAccountWithCards bankAccountWithCards = new BankAccountWithCards(bankAccount);
                    if (cardsQuery != null)
                        bankAccountWithCards.Cards = new ObservableCollection<CardAccount>(cardsQuery.cards);
                    accounts.Add(bankAccountWithCards);
                }

                return accounts;
            }
        }

        internal static List<Account> getAccountsWithoutCards() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                List<Account> list = new List<Account>();
                var cashAccounts = db.Query<CashAccount>("SELECT * FROM CashAccount");
                var bankAccounts = db.Query<BankAccount>("SELECT * FROM BankAccount");

                foreach (var item in cashAccounts)
                    list.Add(item);
                foreach (var item in bankAccounts)
                    list.Add(item);

                return list.OrderBy(i => i.Name).ToList();
            }
        }

        public static List<Account> getAllMoneyAccounts() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                List<Account> list = new List<Account>();
                var cashAccounts = db.Query<CashAccount>("SELECT * FROM CashAccount");
                var bankAccounts = db.Query<BankAccount>("SELECT * FROM BankAccount");
                var cardAccounts = db.Query<CardAccount>("SELECT * FROM CardAccount");

                foreach (var item in cashAccounts)
                    list.Add(item);
                foreach (var item in bankAccounts)
                    list.Add(item);
                foreach (var item in cardAccounts)
                    list.Add(item);

                return list.OrderBy(i => i.Name).ToList();
                //return db.Query<MoneyAccount>("SELECT * FROM MoneyAccount");
            }
        }

        public static List<MoneyAccountBalance> listOfMoneyAccountBalances(DateTime date) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                List<MoneyAccountBalance> list = new List<MoneyAccountBalance>();

                var query = from p in db.Table<Operation>().ToList()
                            group p by p.MoneyAccountId into g
                            select new {
                                account = getAccountById(g.Key),
                                initialValue = getInitialValue(g, date),
                                finalValue = getFinalValue(g, date)
                            };

                foreach (var item in query.Where(i => !(i.account is CardAccount)))
                    list.Add(new MoneyAccountBalance(item.account, item.initialValue, item.finalValue));

                foreach (var item in query.Where(i => i.account is CardAccount)) {
                    MoneyAccountBalance moneyAccountBalance = list.SingleOrDefault(i => i.Account.Id == ((CardAccount)item.account).BankAccountId);
                    if (moneyAccountBalance != null) {
                        moneyAccountBalance.JoinBalance(item.initialValue, item.finalValue);
                    }
                    else {
                        list.Add(new MoneyAccountBalance(AccountsDal.getAccountById(((CardAccount)item.account).BankAccountId), item.initialValue, item.finalValue));
                    }
                }
                return list.OrderBy(i => i.Account.Name).ToList();
            }
        }


        /* ADD UPDATE REMOVE */

        public static void addAccount(Account account) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(account);
                db.Execute("UPDATE sqlite_sequence SET seq = seq + 1 WHERE name = 'Account'");
            }
        }

        public static void removeSingleAccountWithOperations(int accountId) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM CashAccount WHERE Id = ?", accountId);
                db.Execute("DELETE FROM BankAccount WHERE Id = ?", accountId);
                db.Execute("DELETE FROM CardAccount WHERE Id = ?", accountId);
                db.Execute("DELETE FROM Operation WHERE MoneyAccountId = ?", accountId);
            }
        }

        public static void removeBankAccountWithCards(int bankAccountId) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("DELETE FROM BankAccount WHERE Id = ?", bankAccountId);
                db.Execute("DELETE FROM CardAccount WHERE BankAccountId = ?", bankAccountId);
            }
        }


        /* HELPERS */

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

        private static decimal getFinalValue(IGrouping<int, Operation> operations, DateTime date) {
            if (date.Date > DateTime.Today.Date)
                return operations.Sum(i => i.isExpense ? -i.Cost : i.Cost);
            else
                return operations.Where(i => !string.IsNullOrEmpty(i.Date) && DateTime.Parse(i.Date) < maxDateInFinalValue(date))
                                    .Sum(i => i.isExpense ? -i.Cost : i.Cost);
        }

        private static decimal getInitialValue(IGrouping<int, Operation> operations, DateTime date) {
            return operations.Where(i => !string.IsNullOrEmpty(i.Date) && DateTime.Parse(i.Date) < maxDateInInitialValue(date))
                                .Sum(i => i.isExpense ? -i.Cost : i.Cost);
        }

    }
}
