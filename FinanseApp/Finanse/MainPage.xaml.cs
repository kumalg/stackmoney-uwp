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
using Finanse.Views;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Finanse {
    public sealed partial class MainPage : Page {

        public MainPage() {
            this.InitializeComponent();
            AktualnaStrona_Frame.Navigate(typeof(Strona_glowna));
            StatusBarAndTitleBar();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
        }

        private void StatusBarAndTitleBar() {

            //PC customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView")) {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null) {
                    titleBar.ButtonBackgroundColor = Color.FromArgb(255, 11, 99, 199);
                    titleBar.ButtonForegroundColor = Colors.White;
                    titleBar.BackgroundColor = Color.FromArgb(255, 11, 99, 199);
                    titleBar.ForegroundColor = Colors.White;
                }
            }

            //Mobile customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar")) {

                var statusBar = StatusBar.GetForCurrentView();
                if (statusBar != null) {
                    statusBar.BackgroundOpacity = 1;
                    statusBar.BackgroundColor = Color.FromArgb(255, 43, 43, 43);
                    statusBar.ForegroundColor = Colors.White;
                }
            }
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (Strona_glowna_ListBoxItem.IsSelected) {
                AktualnaStrona_Frame.Navigate(typeof(Strona_glowna));
            }

            else if (Kategorie_ListBoxItem.IsSelected) {
                AktualnaStrona_Frame.Navigate(typeof(Kategorie));
            }

            else if (Szablony_ListBoxItem.IsSelected) {
                AktualnaStrona_Frame.Navigate(typeof(Szablony));
            }

            else if (ZleceniaStale_ListBoxItem.IsSelected) {
                AktualnaStrona_Frame.Navigate(typeof(ZleceniaStale));
            }

            else if (PlanowaneWydatki_ListBoxItem.IsSelected) {
                AktualnaStrona_Frame.Navigate(typeof(PlanowaneWydatki));
            }

            else if (Ustawienia_ListBoxItem.IsSelected) {
                AktualnaStrona_Frame.Navigate(typeof(Ustawienia));
            }

            if (MySplitView.IsPaneOpen) MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
    }
}
