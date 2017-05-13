using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Newtonsoft.Json;

namespace Finanse.Models.WhatsNew {
    public class WhatsNewHelper {
        public static async Task<WhatsNew[]> GetWhatsNewItemsAsync() {
            const string filepath = @"Assets\WhatsNew.json";
            var folder = Package.Current.InstalledLocation;
            try {
                var file = await folder.GetFileAsync(filepath);
                var lines = await FileIO.ReadTextAsync(file);
                return JsonConvert.DeserializeObject<WhatsNew[]>(lines);
            }
            catch (Exception e) {
                Debug.WriteLine("Can't get \"what's new\" from file\n" + e.Message);
                return new WhatsNew[]{};
            }
        }

        public static async Task<string> GetJsonStringAsync() {
            var stringBuilder = new StringBuilder();
            WhatsNew[] whatsNews = await GetWhatsNewItemsAsync();
            foreach (var item in whatsNews)
                stringBuilder.AppendLine(item.Formatted);
            return stringBuilder.ToString();
        }
    }
}
