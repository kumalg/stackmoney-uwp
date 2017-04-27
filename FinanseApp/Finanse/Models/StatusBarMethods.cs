using System;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Finanse.Models.Helpers;

namespace Finanse.Models {
    public class StatusBarMethods {
        public static void WhichOrientation() {
            var info = DisplayInformation.GetForCurrentView();

            if (info.CurrentOrientation == DisplayOrientations.Landscape || info.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
                HideStatusBar();
            else
                ShowStatusBar();
        }

        private static async void ShowStatusBar() {
            var statusBar = CurrentStatusBar;
            if (statusBar != null)
                await statusBar.ShowAsync();
        }

        private static async void HideStatusBar() {
            var statusBar = CurrentStatusBar;
            if (statusBar != null)
                await statusBar.HideAsync();
        }

        public static void SetStatusBarColors() {
            var statusBar = CurrentStatusBar;
            if (statusBar == null)
                return;

            statusBar.BackgroundOpacity = 1;

            if (Settings.Theme == ApplicationTheme.Light)
                SetLightStatusBar(statusBar);
            else
                SetDarkStatusBar(statusBar);
        }
        
        private static StatusBar CurrentStatusBar => !ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")
            ? null
            : StatusBar.GetForCurrentView();
            
        public static void SetLightStatusBar() {
            var statusBar = CurrentStatusBar;
            if (statusBar != null)
                SetLightStatusBar(statusBar);
        }
        public static void SetDarkStatusBar() {
            var statusBar = CurrentStatusBar;
            if (statusBar != null)
                SetDarkStatusBar(statusBar);
        }

        private static void SetLightStatusBar(StatusBar statusBar) {
            statusBar.BackgroundColor = Functions.GetSolidColorBrush("#fff7f7f7").Color;
            statusBar.ForegroundColor = Functions.GetSolidColorBrush("#ff3e3e3e").Color;
        }

        private static void SetDarkStatusBar(StatusBar statusBar) {
            statusBar.BackgroundColor = Functions.GetSolidColorBrush("#ff2b2b2b").Color;
            statusBar.ForegroundColor = Functions.GetSolidColorBrush("#ffc8c8c8").Color;
        }
    }
}
