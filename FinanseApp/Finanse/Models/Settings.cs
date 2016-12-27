using System;
using System.Collections.Generic;
using System.Globalization;
using Windows.UI.Xaml;

namespace Finanse.Models {

    public class Settings {

        //Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
        public string CultureInfoName { get; set; }

        public static void SetSettings() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataContainer roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            if (localSettings.Values["whichCurrency"] == null)
                localSettings.Values["whichCurrency"] = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;

            if (localSettings.Values["iconStyle"] == null)
                localSettings.Values["iconStyle"] = "Segoe UI Symbol";

            if (localSettings.Values["Theme"] == null)
                localSettings.Values["Theme"] = 1;

            if (localSettings.Values["maxFutureMonths"] == null)
                localSettings.Values["maxFutureMonths"] = 6;

            if (localSettings.Values["categoryNameVisibility"] == null)
                localSettings.Values["categoryNameVisibility"] = false;

            if (localSettings.Values["accountEllipseVisibility"] == null)
                localSettings.Values["accountEllipseVisibility"] = true;

            if (roamingSettings.Values["whichCurrency"] == null)
                roamingSettings.Values["whichCurrency"] = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
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

        public static ApplicationTheme GetTheme() {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            ApplicationTheme actualTheme;

            if ((int)localSettings.Values["Theme"] == 1)
                actualTheme = ApplicationTheme.Dark;
            else
                actualTheme = ApplicationTheme.Light;

            return actualTheme;
        }
        public static void SetTheme(int i) {
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["Theme"] = i;
        }

        public static CultureInfo GetActualCurrency() {

            //   Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;
            
            CultureInfo value = new CultureInfo(localSettings.Values["whichCurrency"].ToString());
            return value;
        }
        public static void SetActualCurrency(string currency) {

            //   Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            localSettings.Values["whichCurrency"] = currency;
        }

        public static Windows.Globalization.DayOfWeek GetFirstDayOfWeek() {

            return Windows.System.UserProfile.GlobalizationPreferences.WeekStartsOn;
        }

        public static string GetActualIconStyle() {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            string value = localSettings.Values["iconStyle"].ToString();
            return value;
        }
        public static void SetActualIconStyle(string iconStyle) {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["iconStyle"] = iconStyle;
        }

        public static int GetMaxFutureMonths() {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            int value = (int)localSettings.Values["maxFutureMonths"];
            return value;
        }

        public static DateTime GetMaxDate() {
            return new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month)).AddMonths(Settings.GetMaxFutureMonths());
        }
        public static void SetMaxFutureMonths(int months) {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["maxFutureMonths"] = months;
        }

        public static bool GetCategoryNameVisibility() {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            bool value = (bool)localSettings.Values["categoryNameVisibility"];

            return value;
        }
        public static void SetCategoryNameVisibility(bool value) {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["categoryNameVisibility"] = value;
        }

        public static bool GetAccountEllipseVisibility() {

            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            bool value = (bool)localSettings.Values["accountEllipseVisibility"];

            return value;
        }
        public static void SetAccountEllipseVisibility(bool value) {
            
            Windows.Storage.ApplicationDataContainer localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            localSettings.Values["accountEllipseVisibility"] = value;
        }
    }
}
