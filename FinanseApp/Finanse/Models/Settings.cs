using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;

namespace Finanse.Models {

    public class Settings {

        public static string[] AllCultures = {
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

        public static void SetSettings() {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["syncSettings"] == null)
                localSettings.Values["syncSettings"] = true;

            var actualTypeOfSettings = WhichSettings();

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

        public static List<CultureInfo> GetAllCurrencies() => AllCultures.Select(s => new CultureInfo(s)).ToList();

        public static ApplicationTheme GetTheme() {
            return (int)WhichSettings().Values["Theme"] == 1 ?
                ApplicationTheme.Dark :
                ApplicationTheme.Light;
        }
        public static void SetTheme(int i) {
            WhichSettings().Values["Theme"] = i;
        }

        public static CultureInfo GetActualCultureInfo() {
            return new CultureInfo(WhichSettings().Values["whichCurrency"].ToString());
        }

        private static void SetFromRoamingToLocal() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            localSettings.Values["iconStyle"] = roamingSettings.Values["iconStyle"];
            localSettings.Values["Theme"] = roamingSettings.Values["Theme"];
            localSettings.Values["maxFutureMonths"] = roamingSettings.Values["maxFutureMonths"];
            localSettings.Values["categoryNameVisibility"] = roamingSettings.Values["categoryNameVisibility"];
            localSettings.Values["accountEllipseVisibility"] = roamingSettings.Values["accountEllipseVisibility"];
            localSettings.Values["whichCurrency"] = roamingSettings.Values["whichCurrency"];
        }
        public static void SetSyncSettings(bool syncSettings) {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if ((bool)localSettings.Values["syncSettings"] && !syncSettings)
                SetFromRoamingToLocal();

            localSettings.Values["syncSettings"] = syncSettings;
        }
        public static bool GetSyncSettings() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return (bool)localSettings.Values["syncSettings"];
        }

        private static Windows.Storage.ApplicationDataContainer WhichSettings() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            return (bool)localSettings.Values["syncSettings"] ?
                Windows.Storage.ApplicationData.Current.RoamingSettings :
                Windows.Storage.ApplicationData.Current.LocalSettings;
        }
        public static void SetActualCultureInfo(string culture) {
            WhichSettings().Values["whichCurrency"] = culture;
        }

        public static Windows.Globalization.DayOfWeek GetFirstDayOfWeek() {
            return Windows.System.UserProfile.GlobalizationPreferences.WeekStartsOn;
        }

        public static string GetActualIconStyle() {
            return WhichSettings().Values["iconStyle"].ToString();
        }

        public static int GetMaxFutureMonths() {
            return (int)WhichSettings().Values["maxFutureMonths"];
        }

        public static DateTime GetMaxDate() {
            return new DateTime(
                DateTime.Today.Year, 
                DateTime.Today.Month, 
                DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))
                    .AddMonths(Settings.GetMaxFutureMonths());
        }
        public static void SetMaxFutureMonths(int months) {
            WhichSettings().Values["maxFutureMonths"] = months;
        }
        public static DateTime GetMinDate() {
            return new DateTime(2000,1,1);
        }

        public static bool GetCategoryNameVisibility() {
            return (bool)WhichSettings().Values["categoryNameVisibility"];
        }
        public static void SetCategoryNameVisibility(bool value) {
            WhichSettings().Values["categoryNameVisibility"] = value;
        }

        public static bool GetAccountEllipseVisibility() {
            return (bool)WhichSettings().Values["accountEllipseVisibility"];
        }
        public static void SetAccountEllipseVisibility(bool value) {
            WhichSettings().Values["accountEllipseVisibility"] = value;
        }
    }
}
