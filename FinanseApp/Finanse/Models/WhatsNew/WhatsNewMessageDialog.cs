using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Finanse.Models.Helpers;
using Finanse.Models.WhatsNew;

namespace Finanse.Models.WhatsNew {
    public class WhatsNewMessageDialog {
        public static async void ShowWhatsNewDialog(string appVersion) {
            var whatsNewTable = await WhatsNewHelper.GetWhatsNewItemsAsync();
            var whatsNewList = new List<WhatsNew>(whatsNewTable);
            var whatsNewItem = whatsNewList.SingleOrDefault(item => item.AppVersion == appVersion);
            var description = string.Empty;

            if (whatsNewItem != null)
                description = whatsNewItem.FormattedDescription;

            MessageDialog messageDialog = new MessageDialog(description, "Co nowego w wersji " + appVersion);
            await messageDialog.ShowAsync();
        }

        public static void CheckForNewerAppVersion() {
            if (Settings.LastAppVersion == Informations.AppVersion)
                return;

            ShowWhatsNewDialog(Informations.AppVersion);
            Settings.LastAppVersion = Informations.AppVersion;
        }
    }
}
