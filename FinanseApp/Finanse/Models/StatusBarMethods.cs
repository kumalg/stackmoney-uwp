using System;
using Windows.Foundation.Metadata;
using Windows.Graphics.Display;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Finanse.Models {
    public class StatusBarMethods {
        public static void WhichOrientation() {
            var info = DisplayInformation.GetForCurrentView();

            if (info.CurrentOrientation == DisplayOrientations.Landscape || info.CurrentOrientation == DisplayOrientations.LandscapeFlipped) {
                SetStatusBarColors("Background", "Text-1");
                HideStatusBar();
            }
            else {
                SetStatusBarColors("AccentColor", "AccentText");
                ShowStatusBar();
            }
        }
        private static async void ShowStatusBar() {
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                return;

            var statusBar = StatusBar.GetForCurrentView();
            await statusBar.ShowAsync();
        }

        private static async void HideStatusBar() {
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                return;

            var statusBar = StatusBar.GetForCurrentView();
            await statusBar.HideAsync();
        }

        private static void SetStatusBarColors(string statusBarBackgroundColor, string statusBarForegroundColor) {
            //PC customization
            /*
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView")) {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null) {
                    titleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["AccentColor"] as SolidColorBrush).Color;
                    titleBar.ButtonForegroundColor = Colors.White;

                    titleBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["AccentColor"] as SolidColorBrush).Color;
                    titleBar.ForegroundColor = Colors.White;
                }
            }
            */
            //Mobile customization
            if (!ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar"))
                return;

            var statusBar = StatusBar.GetForCurrentView();
            if (statusBar == null)
                return;

            statusBar.BackgroundOpacity = 1;
            statusBar.BackgroundColor = ( (SolidColorBrush)Application.Current.Resources[statusBarBackgroundColor] ).Color;

            statusBar.ForegroundColor = statusBarForegroundColor == "White" ?
                Colors.White :
                ((SolidColorBrush)Application.Current.Resources[statusBarForegroundColor]).Color;
        }
    }
}
