using Finanse.Models.MoneyAccounts;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Finanse.Models.Helpers;
using Finanse.Models.Operations;

namespace Finanse.DataAccessLayer {
    internal class AccountsDal : DalBase {


        /* GET BY ID */

        internal static int GetHighestIdOfAccounts() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<int>("SELECT seq FROM sqlite_sequence WHERE name = 'Account' LIMIT 1");
            }
        }

        public static decimal BankAccountBalanceById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>("SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) " +
                                                 "FROM Operation " +
                                                 "WHERE MoneyAccountId = ? AND Date IS NOT NULL AND Date IS NOT '' AND Date <= ? AND IsDeleted = 0", 
                                                 id, DateTime.Today.Date.ToString("yyyy.MM.dd"));
            }
        }

        public static decimal BankAccountWithCardsBalanceById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>("SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) " +
                                                 "FROM Operation " +
                                                 "WHERE MoneyAccountId IN (?, (SELECT DISTINCT Id FROM CardAccount Where BankAccountId = ?)) AND Date IS NOT NULL AND Date IS NOT '' AND Date <= ? AND IsDeleted = 0",
                                                 id, id, DateTime.Today.Date.ToString("yyyy.MM.dd"));
            }
        }

        public static decimal CashAccountBalanceById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>("SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) " +
                                                 "FROM Operation " +
                                                 "WHERE MoneyAccountId = ? AND Date IS NOT NULL AND Date IS NOT '' AND Date <= ? AND IsDeleted = 0", 
                                                 id, DateTime.Today.Date.ToString("yyyy.MM.dd"));
            }
        }

        public static decimal CardAccountExpensesById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>("SELECT TOTAL(-Cost) " +
                                                 "FROM Operation " +
                                                 "WHERE MoneyAccountId = ? AND isExpense AND Date IS NOT NULL AND Date IS NOT '' AND Date <= ? AND IsDeleted = 0", 
                                                 id, DateTime.Today.Date.ToString("yyyy.MM.dd"));
            }
        }

        public static Account GetAccountById(int id) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                var cashAccount = db.Query<CashAccount>("SELECT * FROM CashAccount WHERE Id == ? LIMIT 1", id).FirstOrDefault();
                if (cashAccount != null)
                    return cashAccount;

                var bankAccount = db.Query<BankAccount>("SELECT * FROM BankAccount WHERE Id == ? LIMIT 1", id).FirstOrDefault();
                if (bankAccount != null)
                    return bankAccount;

                var cardAccount = db.Query<CardAccount>("SELECT * FROM CardAccount WHERE Id == ? LIMIT 1", id).FirstOrDefault();
                return cardAccount;
            }
        }


        /* GET ALL */

        public static List<Account> GetAllAccounts() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var cashAccounts = db.Query<CashAccount>("SELECT * FROM CashAccount");
                var bankAccounts = db.Query<BankAccount>("SELECT * FROM BankAccount");
                var cardAccounts = db.Query<CardAccount>("SELECT * FROM CardAccount");
                var cardAccountsGrouped = from cardAccount in cardAccounts
                                          group cardAccount by cardAccount.BankAccountId into g
                                          select new {
                                              cards = g,
                                              BankAccountId = g.Key
                                          };

                List<Account> accounts = cashAccounts.Cast<Account>().ToList();

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

        public static List<BankAccount> GetAllBankAccounts() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<BankAccount>("SELECT * FROM BankAccount");
            }
        }

        internal static List<Account> GetAccountsWithoutCards() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                var cashAccounts = db.Query<CashAccount>("SELECT * FROM CashAccount");
                var bankAccounts = db.Query<BankAccount>("SELECT * FROM BankAccount");

                List<Account> list = cashAccounts.Cast<Account>().ToList();
                list.AddRange(bankAccounts);

                return list.OrderBy(i => i.Name).ToList();
            }
        }

        public static List<Account> GetAllMoneyAccounts() {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                var cashAccounts = db.Query<CashAccount>("SELECT * FROM CashAccount");
                var bankAccounts = db.Query<BankAccount>("SELECT * FROM BankAccount");
                var cardAccounts = db.Query<CardAccount>("SELECT * FROM CardAccount");

                List<Account> list = cashAccounts.Cast<Account>().ToList();
                list.AddRange(bankAccounts);
                list.AddRange(cardAccounts);

                return list.OrderBy(i => i.Name).ToList();
            }
        }

        public static List<MoneyAccountBalance> ListOfMoneyAccountBalances(DateTime date) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                var query = from p in db.Query<Operation>("SELECT * FROM Operation WHERE IsDeleted = 0")
                            group p by p.MoneyAccountId into g
                            select new {
                                account = GetAccountById(g.Key),
                                initialValue = GetInitialValue(g, date),
                                finalValue = GetFinalValue(g, date)
                            };

                List<MoneyAccountBalance> list = query
                    .Where(i => !(i.account == null || i.account is CardAccount))
                    .Select(item => new MoneyAccountBalance(item.account, item.initialValue, item.finalValue))
                    .ToList();

                foreach (var item in query.Where(i => i.account is CardAccount)) {
                    MoneyAccountBalance moneyAccountBalance = list.SingleOrDefault(i => i.Account.Id == ((CardAccount)item.account).BankAccountId);

                    if (moneyAccountBalance != null)
                        moneyAccountBalance.JoinBalance(item.initialValue, item.finalValue);
                    else
                        list.Add(new MoneyAccountBalance(GetAccountById(((CardAccount)item.account).BankAccountId), item.initialValue, item.finalValue));
                }
                return list.OrderBy(i => i.Account.Name).ToList();
            }
        }


        /* ADD UPDATE REMOVE */

        public static void AddAccount(Account account) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                account.Id = GetHighestIdOfAccounts() + 1;
                db.Insert(account);
             //   db.Execute("UPDATE sqlite_sequence SET seq = seq + 1 WHERE name = 'Account'");
            }
        }

        public static void UpdateAccount(Account account) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                if (account is BankAccountWithCards)
                    db.Update(new BankAccount {
                        Id = account.Id,
                        ColorKey = account.ColorKey,
                        Name = account.Name,
                    });
                else
                    db.Update(account);
            }
        }

        public static void RemoveSingleAccountWithOperations(int accountId) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("UPDATE Operation SET IsDeleted = 1, LastModifed = ? WHERE MoneyAccountId = ?", DateHelper.ActualTimeString, accountId);

                db.Execute("DELETE FROM CashAccount WHERE Id = ?", accountId);
                db.Execute("DELETE FROM BankAccount WHERE Id = ?", accountId);
                db.Execute("DELETE FROM CardAccount WHERE Id = ?", accountId);
            }
        }

        public static void RemoveBankAccountWithCards(int bankAccountId) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Execute("UPDATE Operation " +
                           "SET IsDeleted = 1, LastModifed = ? " +
                           "WHERE MoneyAccountId IN (?, (SELECT DISTINCT Id FROM CardAccount Where BankAccountId = ?))", DateHelper.ActualTimeString, bankAccountId, bankAccountId);

                db.Execute("DELETE FROM BankAccount WHERE Id = ?", bankAccountId);
                db.Execute("DELETE FROM CardAccount WHERE BankAccountId = ?", bankAccountId);
            }
        }


        public static int CountBankAccouns() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<int>("SELECT COUNT(*) FROM BankAccount");
            }
        }

        public static int CountCashAccouns() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<int>("SELECT COUNT(*) FROM CashAccount");
            }
        }

        /* HELPERS */

        private static DateTime MaxDateInFinalValue(DateTime date) {
            return (date.Month == DateTime.Today.Month && date.Year == DateTime.Today.Year) ?
                DateTime.Today.AddDays(1) :
                date.AddMonths(1);
        }

        private static DateTime MaxDateInInitialValue(DateTime date) {
            return (date > DateTime.Today) ?
                DateTime.Today.AddDays(1) :
                date;
        }

        private static decimal GetFinalValue(IEnumerable<Operation> operations, DateTime date) {
            if (date.Date > DateTime.Today.Date)
                return operations.Sum(i => i.isExpense ? -i.Cost : i.Cost);
            return operations.Where(i => !string.IsNullOrEmpty(i.Date) && DateTime.Parse(i.Date) < MaxDateInFinalValue(date))
                .Sum(i => i.isExpense ? -i.Cost : i.Cost);
        }

        private static decimal GetInitialValue(IEnumerable<Operation> operations, DateTime date) {
            return operations.Where(i => !string.IsNullOrEmpty(i.Date) && DateTime.Parse(i.Date) < MaxDateInInitialValue(date))
                                .Sum(i => i.isExpense ? -i.Cost : i.Cost);
        }
    }
}
