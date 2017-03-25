using System;
using System.Collections.Generic;
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
            using (var dbOneDrive = new SQLiteConnection(new SQLitePlatformWinRT(), DBPathLocalFromFileName(localFileName))) {
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
                if (string.IsNullOrEmpty(oneDriveOperation.DeviceId))
                    continue;

                var localOperation =
                                    localOperations.FirstOrDefault(
                                        item =>
                                            item.RemoteId == oneDriveOperation.RemoteId &&
                                            item.DeviceId == oneDriveOperation.DeviceId);

                if (localOperation == null) {
                    InsertSyncOperation(oneDriveOperation);
                    continue;
                }

                if (localOperation.LastModifed != null &&
                    DateTime.Parse(localOperation.LastModifed) >= DateTime.Parse(oneDriveOperation.LastModifed))
                    continue;

                if (oneDriveOperation.LastModifed == null)
                    oneDriveOperation.LastModifed = DateHelper.ActualTimeString;

                UpdateSyncOperation(oneDriveOperation);
            }
        }
        private static void UpdateCategories(IEnumerable<Category> localCategories, IEnumerable<Category> oneDriveCategories) {
            foreach (var oneDriveCategory in oneDriveCategories) {
                if (string.IsNullOrEmpty(oneDriveCategory.DeviceId))
                    continue;

                var localCategory = localCategories.FirstOrDefault(
                                        item =>
                                            item.RemoteId == oneDriveCategory.RemoteId &&
                                            item.DeviceId == oneDriveCategory.DeviceId);

                if (localCategory == null) {
                    InsertSyncCategory(oneDriveCategory);
                    continue;
                }

                if (localCategory.LastModifed != null &&
                    DateTime.Parse(localCategory.LastModifed) >= DateTime.Parse(oneDriveCategory.LastModifed))
                    continue;

                if (oneDriveCategory.LastModifed == null)
                    oneDriveCategory.LastModifed = DateHelper.ActualTimeString;

                UpdateSyncCategory(oneDriveCategory);
            }
        }
    }
}
