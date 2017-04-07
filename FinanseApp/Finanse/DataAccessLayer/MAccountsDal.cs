using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Finanse.Models;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;
using Finanse.Models.DateTimeExtensions;
using Finanse.Models.Helpers;

namespace Finanse.DataAccessLayer {
    public class MAccountsDal : DalBase{

        public static IEnumerable<MAccount> GetAllAccounts() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<MAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId ISNULL AND IsDeleted = 0").OrderBy(i => i.Name);
            }
        }

        public static IEnumerable<SubMAccount> GetAllSubAccounts() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.Query<SubMAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId IS NOT NULL AND IsDeleted = 0").OrderBy(i => i.Name);
            }
        }
        
        public static IEnumerable<MAccount> GetAllAccountsAndSubAccounts() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                var accounts = db.Query<MAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId ISNULL AND IsDeleted = 0");
                accounts.AddRange(db.Query<SubMAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId IS NOT NULL AND IsDeleted = 0"));
                return accounts.OrderBy(i => i.Name);
            }
        }

        public static IEnumerable<MAccountWithSubMAccounts> GetAllAccountsWithSubAccounts() { 
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                var subAccountGroups =
                    from s in db.Query<SubMAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId IS NOT NULL AND IsDeleted = 0")
                    orderby s.Name
                    group s by s.BossAccountGlobalId into g
                    select new {
                        BossAccountGlobalId = g.Key,
                        SubAccountGroup = g
                    };

                return from account in db.Query<MAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId ISNULL AND IsDeleted = 0") orderby account.Name
                       join subAccounts in subAccountGroups on account.GlobalId equals subAccounts.BossAccountGlobalId into gj
                       from secondSubAccounts in gj.DefaultIfEmpty()
                       select new MAccountWithSubMAccounts {
                           MAccount = account,
                           SubMAccounts =
                               secondSubAccounts == null
                                   ? new ObservableCollection<SubMAccount>()
                                   : new ObservableCollection<SubMAccount>(secondSubAccounts.SubAccountGroup)
                       };
            }
        }


        
        public static IEnumerable<MoneyMAccountBalance> ListOfMoneyAccountBalances(DateTime date) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                
                var query = from p in db.Query<Operation>("SELECT * FROM Operation WHERE IsDeleted = 0")
                            group p by p.MoneyAccountId into g
                            select new {
                                account = GetAccountByGlobalId(g.Key), //TODO tu ma nie być ToString
                                initialValue = GetInitialValue(g, date),
                                finalValue = GetFinalValue(g, date)
                            };

                List<MoneyMAccountBalance> list = query
                    .Where(i => !( i.account == null || i.account is SubMAccount ))
                    .Select(item => new MoneyMAccountBalance(item.account, item.initialValue, item.finalValue))
                    .ToList();

                foreach (var item in query.Where(i => i.account is SubMAccount)) {
                    MoneyMAccountBalance moneyAccountBalance = list.SingleOrDefault(i => i.Account.GlobalId == ( (SubMAccount)item.account ).BossAccountGlobalId);

                    if (moneyAccountBalance != null)
                        moneyAccountBalance.JoinBalance(item.initialValue, item.finalValue);
                    else
                        list.Add(new MoneyMAccountBalance(GetAccountByGlobalId(( (SubMAccount)item.account ).BossAccountGlobalId), item.initialValue, item.finalValue));
                }
                return list.OrderBy(i => i.Account.Name);
            }
        }



        public static bool AccountExistInBaseByName(string name) {
            using (var db = DbConnection) {
                return db.ExecuteScalar<bool>("SELECT COUNT(*) FROM MAccount WHERE LOWER(Name) = ?", name.ToLower());
            }
        }


        public static decimal AccountWithSubAccountsBalanceByGlobalId(string globalId) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<decimal>(
                    "SELECT TOTAL(CASE WHEN isExpense THEN -Cost ELSE Cost END) " +
                    "FROM Operation " +
                    "WHERE MoneyAccountId IN (?, (SELECT DISTINCT GlobalId FROM MAccount Where BossAccountGlobalId = ?)) AND Date IS NOT NULL AND Date IS NOT '' AND Date <= ? AND IsDeleted = 0"
                        ,globalId
                        ,globalId
                        ,DateTime.Today.Date.ToString("yyyy.MM.dd"));
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

        public static MAccount GetAccountById(int id) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                var item = db.Query<MAccount>("SELECT * FROM MAccount WHERE Id = ? AND BossAccountGlobalId ISNULL AND IsDeleted = 0 LIMIT 1", id)
                    .FirstOrDefault();

                if (item != null)
                    return item;

                return db.Query<SubMAccount>("SELECT * FROM MAccount WHERE Id = ? AND BossAccountGlobalId IS NOT NULL AND IsDeleted = 0 LIMIT 1", id)
                    .FirstOrDefault();
            }
        }

        public static MAccount GetAccountByGlobalId(string globalId) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                var item = db.Query<MAccount>("SELECT * FROM MAccount WHERE GlobalId = ? AND BossAccountGlobalId ISNULL AND IsDeleted = 0 LIMIT 1", globalId)
                    .FirstOrDefault();

                if (item != null)
                    return item;

                return db.Query<SubMAccount>("SELECT * FROM MAccount WHERE GlobalId = ? AND BossAccountGlobalId IS NOT NULL AND IsDeleted = 0 LIMIT 1", globalId)
                    .FirstOrDefault();
            }
        }




        public static MAccount AddAccount(MAccount account) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                object[] parameters = {
                    account.Name,
                    account.ColorKey,
                    DateTimeHelper.DateTimeUtcNowString,
                    false,
                    account.GlobalId ?? (Dal.GetMaxRowId(typeof(MAccount)) + 1).NewGlobalIdFromLocal(),//SyncProperties.GetGlobalId(typeof(MAccount)),
                    ( account as SubMAccount )?.BossAccountGlobalId
                };

                db.Execute(
                    "INSERT INTO " + typeof(MAccount).Name + 
                    " (Name, ColorKey, LastModifed, IsDeleted, GlobalId, BossAccountGlobalId) VALUES(?,?,?,?,?,?)"
                        , parameters);

                account.LastModifed = parameters[2] as string;
                account.GlobalId = parameters[4] as string;
                if (account is SubMAccount)
                    ((SubMAccount)account).BossAccountGlobalId = parameters[5] as string;

                return account;
            }
        }

        
        public static void UpdateAccount(MAccount account) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                object[] parameters = {
                    account.Name,
                    account.ColorKey,
                    DateTimeHelper.DateTimeUtcNowString,
                    ( account as SubMAccount )?.BossAccountGlobalId,
                    account.GlobalId
                };

                db.Execute(
                    "UPDATE " + typeof(MAccount).Name + " SET Name = ?, ColorKey = ?, LastModifed = ?, BossAccountGlobalId = ? WHERE GlobalId = ?"
                        , parameters);
            }
        }

        public static void RemoveSubAccountWithOperations(MAccount mAccount) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();

                db.Execute(
                    "UPDATE Operation SET IsDeleted = 1, LastModifed = ? WHERE MoneyAccountId = ?"
                        , DateTimeHelper.DateTimeUtcNowString
                        , mAccount.GlobalId);

                db.Execute(
                    "UPDATE MAccount SET IsDeleted = 1, LastModifed = ? WHERE GlobalId = ?"
                        , DateTimeHelper.DateTimeUtcNowString
                        , mAccount.GlobalId);
            }
        }

        public static void RemoveAccountWithSubAccountsAndOperations(MAccount mAccount) {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                db.Execute(
                    "UPDATE Operation " +
                    "SET IsDeleted = 1, LastModifed = ? " +
                    "WHERE MoneyAccountId IN (?, (SELECT DISTINCT GlobalId FROM MAccount Where BossAccountGlobalId = ?))"
                        , DateTimeHelper.DateTimeUtcNowString
                        , mAccount.GlobalId
                        , mAccount.GlobalId);

                db.Execute(
                    "UPDATE MAccount SET IsDeleted = 1, LastModifed = ? WHERE GlobalId = ?"
                        , DateTimeHelper.DateTimeUtcNowString
                        , mAccount.GlobalId);

                db.Execute(
                    "UPDATE MAccount SET IsDeleted = 1, LastModifed = ? WHERE BossAccountGlobalId = ?"
                        , DateTimeHelper.DateTimeUtcNowString
                        , mAccount.GlobalId);
            }
        }
        


        public static int CountAccounts() {
            using (var db = new SQLiteConnection(new SQLitePlatformWinRT(), DbPath)) {
                db.TraceListener = new DebugTraceListener();
                return db.ExecuteScalar<int>("SELECT COUNT(*) FROM MAccount WHERE BossAccountGlobalId ISNULL AND IsDeleted = 0");
            }
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

        private static DateTime MaxDateInInitialValue(DateTime date) => date > DateTime.Today 
            ? DateTime.Today.AddDays(1)
            : date;

        private static DateTime MaxDateInFinalValue(DateTime date) => date.Month == DateTime.Today.Month && date.Year == DateTime.Today.Year
            ? DateTime.Today.AddDays(1)
            : date.AddMonths(1);
    }
}
