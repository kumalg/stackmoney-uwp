using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using Windows.Storage;
using Windows.System.UserProfile;
using Windows.UI.Xaml;
using Finanse.Models.Helpers;
using DayOfWeek = Windows.Globalization.DayOfWeek;

namespace Finanse.Models {

    public class Settings {

        private static readonly ApplicationDataContainer LocalSettings = ApplicationData.Current.LocalSettings;
        private static readonly ApplicationDataContainer RoamingSettings = ApplicationData.Current.RoamingSettings;

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

        
        public static void SetSettings() {

            if (LocalSettings.Values["syncSettings"] == null)
                LocalSettings.Values["syncSettings"] = true;

            if (LocalSettings.Values["syncData"] == null)
                LocalSettings.Values["syncData"] = false;

            var actualTypeOfSettings = WhichSettings;

            if (actualTypeOfSettings.Values["Theme"] == null)
                actualTypeOfSettings.Values["Theme"] = "Dark";

            if (actualTypeOfSettings.Values["maxFutureMonths"] == null)
                actualTypeOfSettings.Values["maxFutureMonths"] = 6;
            
            if (actualTypeOfSettings.Values["BackupFrequency"] == null)
                actualTypeOfSettings.Values["BackupFrequency"] = 7;

            if (actualTypeOfSettings.Values["categoryNameVisibility"] == null)
                actualTypeOfSettings.Values["categoryNameVisibility"] = false;

            if (actualTypeOfSettings.Values["accountEllipseVisibility"] == null)
                actualTypeOfSettings.Values["accountEllipseVisibility"] = true;

            if (actualTypeOfSettings.Values["whichCurrency"] == null)
                actualTypeOfSettings.Values["whichCurrency"] = CultureInfo.CurrentCulture.Name;
            
            if (LocalSettings.Values["LastAppVersion"] == null)
                LocalSettings.Values["LastAppVersion"] = Informations.AppVersion;
        }

        private static void SetFromRoamingToLocal() {
            LocalSettings.Values["iconStyle"] = RoamingSettings.Values["iconStyle"];
            LocalSettings.Values["Theme"] = RoamingSettings.Values["Theme"];
            LocalSettings.Values["maxFutureMonths"] = RoamingSettings.Values["maxFutureMonths"];
            LocalSettings.Values["categoryNameVisibility"] = RoamingSettings.Values["categoryNameVisibility"];
            LocalSettings.Values["accountEllipseVisibility"] = RoamingSettings.Values["accountEllipseVisibility"];
            LocalSettings.Values["whichCurrency"] = RoamingSettings.Values["whichCurrency"];
        }

        public static bool SyncSettings {
            get { return (bool)LocalSettings.Values["syncSettings"]; }
            set {
                if ((bool) LocalSettings.Values["syncSettings"] && !value)
                    SetFromRoamingToLocal();
                LocalSettings.Values["syncSettings"] = value;
            }
        }

        public static bool SyncData {
            get { return (bool)LocalSettings.Values["syncData"]; }
            set { LocalSettings.Values["syncData"] = value; }
        }

        private static ApplicationDataContainer WhichSettings => (bool)LocalSettings.Values["syncSettings"]
            ? ApplicationData.Current.RoamingSettings
            : ApplicationData.Current.LocalSettings;

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

        public static DayOfWeek FirstDayOfWeek
            => GlobalizationPreferences.WeekStartsOn;

        public static int MaxFutureMonths {
            get { return (int)WhichSettings.Values["maxFutureMonths"]; }
            set { WhichSettings.Values["maxFutureMonths"] = value; }
        }

        public static bool CategoryNameVisibility {
            get { return (bool)WhichSettings.Values["categoryNameVisibility"]; }
            set { WhichSettings.Values["categoryNameVisibility"] = value; }
        }

        public static bool AccountEllipseVisibility {
            get { return (bool)WhichSettings.Values["accountEllipseVisibility"]; }
            set { WhichSettings.Values["accountEllipseVisibility"] = value; }
        }

        public static string LastAppVersion {
            get { return LocalSettings.Values["LastAppVersion"].ToString(); }
            set { LocalSettings.Values["LastAppVersion"] = value; }
        }

        public static int BackupFrequency {
            get { return (int)LocalSettings.Values["BackupFrequency"]; }
            set { LocalSettings.Values["BackupFrequency"] = value; }
        }
    }
}
