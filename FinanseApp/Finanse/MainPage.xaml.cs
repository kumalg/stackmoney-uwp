using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Finanse.Elements;
using Finanse.Pages;
using System.Collections.ObjectModel;
using Windows.Graphics.Display;
using Finanse.Models;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Finanse {
    public sealed partial class MainPage : Page {

        public MainPage() {
            this.InitializeComponent();
            //AktualnaStrona_Frame.Navigate(typeof(Konta));
            Strona_glowna_ListBoxItem.IsChecked = true;

            DisplayInformation info = DisplayInformation.GetForCurrentView();

            whichOrientation(info);
            //StatusBarAndTitleBar("GreyColorStyle");
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            DisplayInformation.GetForCurrentView().OrientationChanged += MainPage_OrientationChanged;
        }

        private void whichOrientation(DisplayInformation info) {
            if (info.CurrentOrientation == DisplayOrientations.Landscape || info.CurrentOrientation == DisplayOrientations.LandscapeFlipped)
                StatusBarAndTitleBar("GreyColorStyle", "White");
            else
                StatusBarAndTitleBar("AccentDarkColorStyle", "AccentTextColorStyle");
        }

        private void MainPage_OrientationChanged(DisplayInformation info, object args) {
            whichOrientation(info);
        }

        private void StatusBarAndTitleBar(string statusBarBackgroundColor, string statusBarForegroundColor) {
            //PC customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView")) {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null) {
                    titleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["AccentDarkColorStyle"] as SolidColorBrush).Color;
                    titleBar.ButtonForegroundColor = Colors.White;

                    titleBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["AccentDarkColorStyle"] as SolidColorBrush).Color;
                    titleBar.ForegroundColor = Colors.White;
                }
            }

            //Mobile customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) {

                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null) {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources[statusBarBackgroundColor] as SolidColorBrush).Color;
                    if (statusBarForegroundColor == "White")
                        statusBar.ForegroundColor = Colors.White;
                    else
                        statusBar.ForegroundColor = ((SolidColorBrush)Application.Current.Resources[statusBarForegroundColor] as SolidColorBrush).Color;
                }
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {

            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
            if (MySplitView.IsPaneOpen) {
                PageFillWhenPaneIsOpen.Visibility = Visibility.Visible;
            }
        }

        private void Strona_glowna_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Strona_glowna));
            ClosingPane();
        }

        private void Kategorie_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Kategorie));
            ClosingPane();
        }

        private void Szablony_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Szablony));
            ClosingPane();
        }

        private void ZleceniaStale_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(ZleceniaStale));
            ClosingPane();
        }

        private void PlanowaneWydatki_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(PlanowaneWydatki));
            ClosingPane();
        }

        private void Ustawienia_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Ustawienia));
            ClosingPane();
        }

        private void Konta_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Konta));
            ClosingPane();
        }

        private void ClosingPane() {
            if (MySplitView.IsPaneOpen)
                MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void Strona_glowna_ListBoxItem_Click(object sender, RoutedEventArgs e) {
            ClosingPane();
        }

        private void MySplitView_PaneClosing(SplitView sender, SplitViewPaneClosingEventArgs args) {
            PageFillWhenPaneIsOpen.Visibility = Visibility.Collapsed;
        }

        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e) {
            if (ThemeToggle.IsOn) {
                (Frame as ThemeAwareFrame).AppTheme = ElementTheme.Dark;
                Settings.SetTheme(1);
            }
            else {
                (Frame as ThemeAwareFrame).AppTheme = ElementTheme.Light;
                Settings.SetTheme(0);
            }
        }
    }
}
