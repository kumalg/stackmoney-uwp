using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI.Xaml;

namespace Finanse.Models {

    public class Settings {

        //Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        //public string CultureInfoName { get; set; }
        public static string[] allCultures = {
            "en-US",
            "pl-PL",
            "en-GB",
            "fr-FR",
            "ja-JP",
            "af-ZA",
            "sq-AL",
            "ar-DZ",
            "ar-BH",
            "en-CA"
        };

        public static void setSettings() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["syncSettings"] == null)
                localSettings.Values["syncSettings"] = true;

            Windows.Storage.ApplicationDataContainer actualTypeOfSettings = whichSettings();

            if (actualTypeOfSettings.Values["iconStyle"] == null)
                actualTypeOfSettings.Values["iconStyle"] = "Segoe UI Symbol";

            if (actualTypeOfSettings.Values["Theme"] == null)
                actualTypeOfSettings.Values["Theme"] = 1;

            if (actualTypeOfSettings.Values["maxFutureMonths"] == null)
                actualTypeOfSettings.Values["maxFutureMonths"] = 6;

            if (actualTypeOfSettings.Values["categoryNameVisibility"] == null)
                actualTypeOfSettings.Values["categoryNameVisibility"] = false;

            if (actualTypeOfSettings.Values["accountEllipseVisibility"] == null)
                actualTypeOfSettings.Values["accountEllipseVisibility"] = true;

            if (actualTypeOfSettings.Values["whichCurrency"] == null)
                actualTypeOfSettings.Values["whichCurrency"] = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
        }

        public static List<CultureInfo> getAllCurrencies() {
            List<CultureInfo> lista = new List<CultureInfo>();
            /*
            {
                new CultureInfo("en-US"),
                new CultureInfo("en-GB"),
                new CultureInfo("fr-FR"),
                new CultureInfo("ja-JP"),
                new CultureInfo("pl-PL"),
            };*/
            foreach (string s in allCultures)
                lista.Add(new CultureInfo(s));

            return lista;
        }

        public static ApplicationTheme getTheme() {
            return (int)whichSettings().Values["Theme"] == 1 ?
                ApplicationTheme.Dark :
                ApplicationTheme.Light;
        }
        public static void setTheme(int i) {
            whichSettings().Values["Theme"] = i;
        }

        public static CultureInfo getActualCultureInfo() {
            return new CultureInfo(whichSettings().Values["whichCurrency"].ToString());
        }

        private static void setFromRoamingToLocal() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            localSettings.Values["iconStyle"] = roamingSettings.Values["iconStyle"];
            localSettings.Values["Theme"] = roamingSettings.Values["Theme"];
            localSettings.Values["maxFutureMonths"] = roamingSettings.Values["maxFutureMonths"];
            localSettings.Values["categoryNameVisibility"] = roamingSettings.Values["categoryNameVisibility"];
            localSettings.Values["accountEllipseVisibility"] = roamingSettings.Values["accountEllipseVisibility"];
            localSettings.Values["whichCurrency"] = roamingSettings.Values["whichCurrency"];
        }
        public static void setSyncSettings(bool syncSettings) {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if ((bool)localSettings.Values["syncSettings"] && !syncSettings)
                setFromRoamingToLocal();

            localSettings.Values["syncSettings"] = syncSettings;
        }
        public static bool getSyncSettings() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return (bool)localSettings.Values["syncSettings"];
        }

        private static Windows.Storage.ApplicationDataContainer whichSettings() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return (bool)localSettings.Values["syncSettings"] ?
                Windows.Storage.ApplicationData.Current.RoamingSettings :
                Windows.Storage.ApplicationData.Current.LocalSettings;
        }
        public static void setActualCultureInfo(string culture) {
            whichSettings().Values["whichCurrency"] = culture;
        }

        public static Windows.Globalization.DayOfWeek getFirstDayOfWeek() {
            return Windows.System.UserProfile.GlobalizationPreferences.WeekStartsOn;
        }

        public static string getActualIconStyle() {
            return whichSettings().Values["iconStyle"].ToString();
        }

        public static int getMaxFutureMonths() {
            return (int)whichSettings().Values["maxFutureMonths"];
        }

        public static DateTime getMaxDate() {
            return new DateTime(
                DateTime.Today.Year, 
                DateTime.Today.Month, 
                DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))
                .AddMonths(Settings.getMaxFutureMonths());
        }
        public static void setMaxFutureMonths(int months) {
            whichSettings().Values["maxFutureMonths"] = months;
        }
        public static DateTime getMinDate() {
            return new DateTime(2000,1,1);
        }

        public static bool getCategoryNameVisibility() {
            return (bool)whichSettings().Values["categoryNameVisibility"];
        }
        public static void setCategoryNameVisibility(bool value) {
            whichSettings().Values["categoryNameVisibility"] = value;
        }

        public static bool getAccountEllipseVisibility() {
            return (bool)whichSettings().Values["accountEllipseVisibility"];
        }
        public static void setAccountEllipseVisibility(bool value) {
            whichSettings().Values["accountEllipseVisibility"] = value;
        }
    }
}
