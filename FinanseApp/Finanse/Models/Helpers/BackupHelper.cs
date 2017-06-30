using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Finanse.Models.Helpers {
    public class BackupHelper {

        public static async Task<IEnumerable<string>> ListOfBackupDates() {
            ApplicationData.Current?.LocalFolder.CreateFolderAsync("Backup", CreationCollisionOption.FailIfExists);

            var backupFolder = await ApplicationData.Current?.LocalFolder.GetFolderAsync("Backup");

            if (backupFolder == null)
                return null;

            var backupFiles = await backupFolder.GetFilesAsync();

            return backupFiles?
                .Select(i => i.Name.Split('.')[1]);
        }

        public static async Task<DateTime> LastBackup() {
            var backupDates = (await ListOfBackupDates()).ToList();
            return !backupDates.Any()
                ? DateTime.MinValue
                : backupDates
                    .Select(i => DateTime.ParseExact(i, "yyyy-MM-dd_HH-mm-ss", null))
                    .OrderByDescending(ij => ij.Date).FirstOrDefault();
        }
    }
}
