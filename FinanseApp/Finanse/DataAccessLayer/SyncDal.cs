using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Finanse.Models.Categories;
using Finanse.Models.Helpers;
using Finanse.Models.MAccounts;
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

                object[] parameters = {
                    category.Name,
                    category.VisibleInIncomes,
                    category.VisibleInExpenses,
                    category.ColorKey,
                    category.IconKey,
                    category.CantDelete,
                    category.LastModifed ?? DateTimeHelper.DateTimeUtcNowString,
                    category.IsDeleted,
                    ( category as SubCategory )?.BossCategoryId,
                    category.GlobalId
                };

                db.Execute(
                    "INSERT INTO Categories (Name, VisibleInIncomes, VisibleInExpenses, ColorKey, IconKey, CantDelete, LastModifed, IsDeleted, BossCategoryId, GlobalId) " +
                    "VALUES(?,?,?,?,?,?,?,?,?,?)"
                    , parameters);
            }
        }
        private static void UpdateSyncCategory(Category category) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                object[] parameters = {
                    category.Name,
                    category.VisibleInIncomes,
                    category.VisibleInExpenses,
                    category.ColorKey,
                    category.IconKey,
                    category.CantDelete,
                    category.LastModifed ?? DateTimeHelper.DateTimeUtcNowString,
                    category.IsDeleted,
                    ( category as SubCategory )?.BossCategoryId,
                    category.GlobalId
                };

                db.Execute(
                    "UPDATE Categories " +
                    "SET Name = ?, VisibleInIncomes = ?, VisibleInExpenses = ?, ColorKey = ?, IconKey = ?, CantDelete = ?, LastModifed = ?, IsDeleted = ?, BossCategoryId = ? " +
                    "WHERE GlobalId = ?"
                    , parameters);
            }
        }

        private static void InsertSyncAccount(MAccount account) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();
                // db.Insert(account);
                object[] parameters = {
                    account.Name,
                    account.ColorKey,
                    account.LastModifed ?? DateTimeHelper.DateTimeUtcNowString,
                    account.IsDeleted,
                    account.GlobalId,
                    ( account as SubMAccount )?.BossAccountGlobalId
                };

                db.Execute(
                    "INSERT INTO " + typeof(MAccount).Name +
                    " (Name, ColorKey, LastModifed, IsDeleted, GlobalId, BossAccountGlobalId) " +
                    "VALUES(?,?,?,?,?,?)"
                        , parameters);
            }
        }
        private static void UpdateSyncAccount(MAccount account) {
            using (var db = DbConnection) {
                db.TraceListener = new DebugTraceListener();

                object[] parameters = {
                    account.Name,
                    account.ColorKey,
                    account.LastModifed ?? DateTimeHelper.DateTimeUtcNowString,
                    account.IsDeleted,
                    (account as SubMAccount)?.BossAccountGlobalId,
                    account.GlobalId
                };

                db.Execute(
                    "UPDATE " + typeof(MAccount).Name + " " +
                    "SET Name = ?, ColorKey = ?, LastModifed = ?, IsDeleted = ?, BossAccountGlobalId = ? WHERE GlobalId = ?"
                    , parameters);
            }
        }


        public static void UpdateBase(string localFileName) {
            IEnumerable<Operation> localOperations, oneDriveOperations;
            IEnumerable<OperationPattern> localOperationPatterns, oneDriveOperationPatterns;
            IEnumerable<Category> localCategories, oneDriveCategories;
            IEnumerable<MAccount> localAccounts, oneDriveAccounts;

            using (var db = DbConnection) {
                localOperations = db.Table<Operation>().ToList();
                localOperationPatterns = db.Table<OperationPattern>().ToList();

                var localCat = db.Query<Category>("SELECT * FROM Categories WHERE BossCategoryId ISNULL");
                localCat.AddRange(db.Query<SubCategory>("SELECT * FROM Categories WHERE BossCategoryId IS NOT NULL"));
                localCategories = localCat;


                var localAcc = db.Query<MAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId ISNULL");
                localAcc.AddRange(db.Query<SubMAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId IS NOT NULL"));
                localAccounts = localAcc;
            }
            using (var dbOneDrive = new SQLiteConnection(new SQLitePlatformWinRT(), DbPathLocalFromFileName(localFileName))) {
                oneDriveOperations = dbOneDrive.Table<Operation>().ToList();
                oneDriveOperationPatterns = dbOneDrive.Table<OperationPattern>().ToList();

                var oneDriveCat = dbOneDrive.Query<Category>("SELECT * FROM Categories WHERE BossCategoryId ISNULL");
                oneDriveCat.AddRange(dbOneDrive.Query<SubCategory>("SELECT * FROM Categories WHERE BossCategoryId IS NOT NULL"));
                oneDriveCategories = oneDriveCat;

                var oneDriveAcc = dbOneDrive.Query<MAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId ISNULL");
                oneDriveAcc.AddRange(dbOneDrive.Query<SubMAccount>("SELECT * FROM MAccount WHERE BossAccountGlobalId IS NOT NULL"));
                oneDriveAccounts = oneDriveAcc;
            }

            UpdateOperations(localOperations, oneDriveOperations);
            UpdateOperations(localOperationPatterns, oneDriveOperationPatterns);

            UpdateCategories(localCategories, oneDriveCategories);

            UpdateAccounts(localAccounts, oneDriveAccounts);
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
                        DateTime.Parse(localOperation.LastModifed) >= DateTime.Parse(oneDriveOperation.LastModifed))
                        continue;

                    oneDriveOperation.Id = localOperation.Id;
                }

                if (oneDriveOperation.LastModifed == null)
                    oneDriveOperation.LastModifed = DateTimeHelper.DateTimeUtcNowString;

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
                        DateTime.Parse(localCategory.LastModifed) >= DateTime.Parse(oneDriveCategory.LastModifed))
                        continue;

                    oneDriveCategory.Id = localCategory.Id;
                }

                if (oneDriveCategory.LastModifed == null)
                    oneDriveCategory.LastModifed = DateTimeHelper.DateTimeUtcNowString;

                UpdateSyncCategory(oneDriveCategory);
            }
        }
        private static void UpdateAccounts(IEnumerable<MAccount> localAccounts, IEnumerable<MAccount> oneDriveAccounts) {
            foreach (var oneDriveAccount in oneDriveAccounts) {
                if (string.IsNullOrEmpty(oneDriveAccount.GlobalId))
                    continue;

                if (localAccounts != null) {
                    var localAccount = localAccounts.FirstOrDefault(
                        item => item.GlobalId == oneDriveAccount.GlobalId);

                    if (localAccount == null) {
                        InsertSyncAccount(oneDriveAccount);
                        continue;
                    }

                    if (localAccount.LastModifed != null &&
                        DateTime.Parse(localAccount.LastModifed) >= DateTime.Parse(oneDriveAccount.LastModifed))
                        continue;

                    oneDriveAccount.Id = localAccount.Id;
                }

                if (oneDriveAccount.LastModifed == null)
                    oneDriveAccount.LastModifed = DateTimeHelper.DateTimeUtcNowString;

                UpdateSyncAccount(oneDriveAccount);
            }
        }
    }
}
