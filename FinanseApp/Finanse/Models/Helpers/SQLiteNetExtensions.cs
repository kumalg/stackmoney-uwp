﻿using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Finanse.Annotations;
using SQLite.Net;
using SQLite.Net.Interop;

namespace Finanse.Models.Helpers {
    public static class SQLiteNetExtensions {
        public static async Task<string> BackupDatabase(this SQLiteConnection conn, ISQLitePlatform platform, string folderName = null) {
            ISQLiteApiExt sqliteApi = platform.SQLiteApi as ISQLiteApiExt;

            if (sqliteApi == null)
                return null;

            string additionalPath = string.Empty;
            if (!string.IsNullOrEmpty(folderName)) {
                ApplicationData.Current?.LocalFolder.CreateFolderAsync(folderName, CreationCollisionOption.FailIfExists);
              //  if (asyncOperation != null)
              //      await asyncOperation;
                additionalPath = "\\" + folderName;
            }

            string destDBPath = ApplicationData.Current?.LocalFolder.Path + additionalPath + "\\db." + DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");

            IDbHandle destDB;
            byte[] databasePathAsBytes = GetNullTerminatedUtf8(destDBPath);
            Result r = sqliteApi.Open(databasePathAsBytes, out destDB,
                (int)( SQLiteOpenFlags.Create | SQLiteOpenFlags.ReadWrite ), IntPtr.Zero);

            if (r != Result.OK) {
                throw SQLiteException.New(r, String.Format("Could not open backup database file: {0} ({1})", destDBPath, r));
            }

            /* Open the backup object used to accomplish the transfer */
            IDbBackupHandle bHandle = sqliteApi.BackupInit(destDB, "main", conn.Handle, "main");
            

            if (bHandle == null) {
                // Close the database connection 
                sqliteApi.Close(destDB);

                throw SQLiteException.New(r, String.Format("Could not initiate backup process: {0}", destDBPath));
            }

            /* Each iteration of this loop copies 5 database pages from database
            ** pDb to the backup database. If the return value of backup_step()
            ** indicates that there are still further pages to copy, sleep for
            ** 250 ms before repeating. */
            do {
                r = sqliteApi.BackupStep(bHandle, 5);

                if (r == Result.OK || r == Result.Busy || r == Result.Locked) {
                    sqliteApi.Sleep(250);
                }
            } while (r == Result.OK || r == Result.Busy || r == Result.Locked);

            /* Release resources allocated by backup_init(). */
            r = sqliteApi.BackupFinish(bHandle);

            if (r != Result.OK) {
                // Close the database connection 
                sqliteApi.Close(destDB);

                throw SQLiteException.New(r, String.Format("Could not finish backup process: {0} ({1})", destDBPath, r));
            }

            // Close the database connection 
            sqliteApi.Close(destDB);

            return destDBPath;
        }

        [NotNull]
        private static byte[] GetNullTerminatedUtf8(string s) {
            var utf8Length = Encoding.UTF8.GetByteCount(s);
            var bytes = new byte[utf8Length + 1];
            Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
            return bytes;
        }
    }
}
