using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Finanse.Models.Categories;
using Finanse.Models.Helpers;
using Finanse.Models.MoneyAccounts;
using Finanse.Models.Operations;
using SQLite.Net;
using SQLite.Net.Platform.WinRT;

namespace Finanse.DataAccessLayer {
    public class SyncDal : DalBase {

        private static void InsertSyncOperation(OperationPattern operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(operation);
            }
        }
        private static void UpdateSyncOperation(OperationPattern operation) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(operation);
            }
        }

        private static void InsertSyncCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(category);
            }
        }
        private static void UpdateSyncCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(category);
            }
        }

        private static void InsertSyncAccount(Account account) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Insert(account);
            }
        }
        private static void UpdateSyncAccount(Account account) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                db.Update(account);
            }
        }


        public static void UpdateBase(string localFileName) {
            IEnumerable<Operation> localOperations, oneDriveOperations;
            IEnumerable<OperationPattern> localOperationPatterns, oneDriveOperationPatterns;
            IEnumerable<Category> localCategories, oneDriveCategories;
            IEnumerable<SubCategory> localSubCategories, oneDriveSubCategories;
            //IEnumerable<BankAccount> localBankAccounts, oneDriveBankAccounts;
            //IEnumerable<CashAccount> localCashAccounts, oneDriveCashAccounts;
            //IEnumerable<CardAccount> localCardAccounts, oneDriveCardAccounts;

            using (var db = DbConnection) {
                localOperations = db.Table<Operation>().ToList();
                localOperationPatterns = db.Table<OperationPattern>().ToList();
                localCategories = db.Table<Category>().ToList();
                localSubCategories = db.Table<SubCategory>().ToList();
                //localBankAccounts = db.Table<BankAccount>().ToList();
                //localCashAccounts = db.Table<CashAccount>().ToList();
                //localCardAccounts = db.Table<CardAccount>().ToList();
            }
            using (var dbOneDrive = new SQLiteConnection(new SQLitePlatformWinRT(), DbPathLocalFromFileName(localFileName))) {
                oneDriveOperations = dbOneDrive.Table<Operation>().ToList();
                oneDriveOperationPatterns = dbOneDrive.Table<OperationPattern>().ToList();
                oneDriveCategories = dbOneDrive.Table<Category>().ToList();
                oneDriveSubCategories = dbOneDrive.Table<SubCategory>().ToList();
                //oneDriveBankAccounts = dbOneDrive.Table<BankAccount>().ToList();
                //oneDriveCashAccounts = dbOneDrive.Table<CashAccount>().ToList();
                //oneDriveCardAccounts = dbOneDrive.Table<CardAccount>().ToList();
            }

            UpdateOperations(localOperations, oneDriveOperations);
            UpdateOperations(localOperationPatterns, oneDriveOperationPatterns);

            UpdateCategories(localCategories, oneDriveCategories);
            UpdateCategories(localSubCategories, oneDriveSubCategories);
        }


        private static void UpdateOperations(IEnumerable<OperationPattern> localOperations, IEnumerable<OperationPattern> oneDriveOperations) {
            foreach (var oneDriveOperation in oneDriveOperations) {
                if (string.IsNullOrEmpty(oneDriveOperation.GlobalId))
                    continue;

                if (localOperations != null) {
                    var localOperation = localOperations.FirstOrDefault(
                        item => item.GlobalId == oneDriveOperation.GlobalId);

                    if (localOperation == null) {
                        InsertSyncOperation(oneDriveOperation);
                        continue;
                    }

                    if (localOperation.LastModifed != null &&
                        DateTimeOffset.Parse(localOperation.LastModifed) >= DateTimeOffset.Parse(oneDriveOperation.LastModifed))
                        continue;
                }

                if (oneDriveOperation.LastModifed == null)
                    oneDriveOperation.LastModifed = DateTimeOffsetHelper.DateTimeOffsetNowString;

                UpdateSyncOperation(oneDriveOperation);
            }
        }
        private static void UpdateCategories(IEnumerable<Category> localCategories, IEnumerable<Category> oneDriveCategories) {
            foreach (var oneDriveCategory in oneDriveCategories) {
                if (string.IsNullOrEmpty(oneDriveCategory.GlobalId))
                    continue;

                if (localCategories != null) {
                    var localCategory = localCategories.FirstOrDefault(
                        item => item.GlobalId == oneDriveCategory.GlobalId);

                    if (localCategory == null) {
                        InsertSyncCategory(oneDriveCategory);
                        continue;
                    }

                    if (localCategory.LastModifed != null &&
                        DateTimeOffset.Parse(localCategory.LastModifed) >= DateTimeOffset.Parse(oneDriveCategory.LastModifed))
                        continue;
                }

                if (oneDriveCategory.LastModifed == null)
                    oneDriveCategory.LastModifed = DateTimeOffsetHelper.DateTimeOffsetNowString;

                UpdateSyncCategory(oneDriveCategory);
            }
        }
    }
}
