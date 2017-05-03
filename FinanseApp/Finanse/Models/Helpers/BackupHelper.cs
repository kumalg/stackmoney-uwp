using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

namespace Finanse.Models.Helpers {
    public class BackupHelper {

        public static async Task<IEnumerable<string>> ListOfBackupDates() {
            ApplicationData.Current?.LocalFolder.CreateFolderAsync("Backup", CreationCollisionOption.FailIfExists);
            StorageFolder backupFolder = await ApplicationData.Current?.LocalFolder.GetFolderAsync("Backup");

            if (backupFolder == null)
                return null;

            var backupFiles = await backupFolder.GetFilesAsync();

            return backupFiles?
                .Select(i => i.Name.Substring(i.Name.IndexOf('.') + 1))
                .AsEnumerable();
        }

        public static async Task<DateTime> LastBackup() {
            var yco = await ListOfBackupDates();
            return !yco.Any() 
                ? DateTime.MinValue
                : yco.Select(i => DateTime.ParseExact(i, "yyyy-MM-dd_HH-mm-ss", null)).OrderByDescending(i => i.Date).FirstOrDefault();
        }
    }
}
