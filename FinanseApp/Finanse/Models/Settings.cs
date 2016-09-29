using System.Collections.Generic;
using System.Globalization;

namespace Finanse.Models {

    public class Settings {

        //Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        public string CultureInfoName {
            get; set;
        }

        public static List<CultureInfo> GetAllCurrencies() {
            List<CultureInfo> lista = new List<CultureInfo>{

                new CultureInfo("en-US"),
                new CultureInfo("en-GB"),
                new CultureInfo("fr-FR"),
                new CultureInfo("ja-JP"),
                new CultureInfo("pl-PL"),
            };

            return lista;
        }

        public static List<string> GetAllFonts() {

            List<string> lista = new List<string>();

            lista.Add("Segoe UI");
            lista.Add("Segoe UI Symbol");

            return lista;
        }

        public static void SetSettings() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["whichCurrency"] == null)
                localSettings.Values["whichCurrency"] = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            if (localSettings.Values["iconStyle"] == null)
                localSettings.Values["iconStyle"] = "Segoe UI Symbol";
        }

        public static CultureInfo GetActualCurrency() {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            CultureInfo value = new CultureInfo(localSettings.Values["whichCurrency"].ToString());
            return value;
        }

        public static string GetActualIconStyle() {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            string value = localSettings.Values["iconStyle"].ToString();
            return value;
        }

        public static void SetActualCurrency(string currency) {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["whichCurrency"] = currency;
        }

        public static void SetActualIconStyle(string iconStyle) {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["iconStyle"] = iconStyle;
        }

        public static CultureInfo StringToCultureInfo(string s) {

            return new CultureInfo(s);
        }
    }
}
