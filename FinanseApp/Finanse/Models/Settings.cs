using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Windows.Security.ExchangeActiveSyncProvisioning;
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

        public static List<CultureInfo> AllCurrencies => AllCultures.Select(s => new CultureInfo(s)).ToList();
        
        public static DateTime MaxDate => new DateTime(
                DateTime.Today.Year,
                DateTime.Today.Month,
                DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month))
            .AddMonths(MaxFutureMonths);

        public static DateTime MinDate => new DateTime(2000, 1, 1);

        public static string DeviceId {
            get {
                var deviceInformation = new EasClientDeviceInformation();
                return deviceInformation.Id.ToString();
            }
        }
        
        public static void SetSettings() {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

            if (localSettings.Values["syncSettings"] == null)
                localSettings.Values["syncSettings"] = true;

            if (localSettings.Values["syncData"] == null)
                localSettings.Values["syncData"] = false;

            var actualTypeOfSettings = WhichSettings;

            if (actualTypeOfSettings.Values["Theme"] == null)
                actualTypeOfSettings.Values["Theme"] = "Dark";

            if (actualTypeOfSettings.Values["maxFutureMonths"] == null)
                actualTypeOfSettings.Values["maxFutureMonths"] = 6;

            if (actualTypeOfSettings.Values["categoryNameVisibility"] == null)
                actualTypeOfSettings.Values["categoryNameVisibility"] = false;

            if (actualTypeOfSettings.Values["accountEllipseVisibility"] == null)
                actualTypeOfSettings.Values["accountEllipseVisibility"] = true;

            if (actualTypeOfSettings.Values["whichCurrency"] == null)
                actualTypeOfSettings.Values["whichCurrency"] = CultureInfo.CurrentCulture.Name;
        }

        private static void SetFromRoamingToLocal() {
            var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            var roamingSettings = Windows.Storage.ApplicationData.Current.RoamingSettings;

            localSettings.Values["iconStyle"] = roamingSettings.Values["iconStyle"];
            localSettings.Values["Theme"] = roamingSettings.Values["Theme"];
            localSettings.Values["maxFutureMonths"] = roamingSettings.Values["maxFutureMonths"];
            localSettings.Values["categoryNameVisibility"] = roamingSettings.Values["categoryNameVisibility"];
            localSettings.Values["accountEllipseVisibility"] = roamingSettings.Values["accountEllipseVisibility"];
            localSettings.Values["whichCurrency"] = roamingSettings.Values["whichCurrency"];
        }

        public static bool SyncSettings {
            get {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                return (bool) localSettings.Values["syncSettings"];
            }
            set {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;

                if ((bool) localSettings.Values["syncSettings"] && !value)
                    SetFromRoamingToLocal();

                localSettings.Values["syncSettings"] = value;
            }
        }

        public static bool SyncData {
            get {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                return (bool)localSettings.Values["syncData"];
            }
            set {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["syncData"] = value;
            }
        }

        private static Windows.Storage.ApplicationDataContainer WhichSettings {
            get {
                var localSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
                return (bool)localSettings.Values["syncSettings"]
                    ? Windows.Storage.ApplicationData.Current.RoamingSettings
                    : Windows.Storage.ApplicationData.Current.LocalSettings;
            }
        }

        public static ApplicationTheme Theme {
            get {
                return WhichSettings.Values["Theme"].ToString() == ApplicationTheme.Light.ToString()
                    ? ApplicationTheme.Light
                    : ApplicationTheme.Dark;
            }
            set { WhichSettings.Values["Theme"] = value.ToString(); }
        }

        public static CultureInfo ActualCultureInfo {
            get { return new CultureInfo(WhichSettings.Values["whichCurrency"].ToString()); }
            set { WhichSettings.Values["whichCurrency"] = value.Name; }
        }

        public static Windows.Globalization.DayOfWeek FirstDayOfWeek
            => Windows.System.UserProfile.GlobalizationPreferences.WeekStartsOn;

        public static int MaxFutureMonths {
            get {
                return (int)WhichSettings.Values["maxFutureMonths"];
            }
            set {
                WhichSettings.Values["maxFutureMonths"] = value;
            }
        }

        public static bool CategoryNameVisibility {
            get {
                return (bool)WhichSettings.Values["categoryNameVisibility"];
            }
            set {
                WhichSettings.Values["categoryNameVisibility"] = value;
            }
        }

        public static bool AccountEllipseVisibility {
            get {
                return (bool)WhichSettings.Values["accountEllipseVisibility"];
            }
            set {
                WhichSettings.Values["accountEllipseVisibility"] = value;
            }
        }
    }
}
