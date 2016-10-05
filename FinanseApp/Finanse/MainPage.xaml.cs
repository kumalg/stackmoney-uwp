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
using Finanse.DataAccessLayer;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Finanse {
    public sealed partial class MainPage : Page {

        public MainPage() {
            this.InitializeComponent();
            //AktualnaStrona_Frame.Navigate(typeof(Konta));
            //Strona_glowna_ListBoxItem.IsChecked = true;
            Dal.CreateDB();
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
                StatusBarAndTitleBar("Background", "Text-1");
            else
                StatusBarAndTitleBar("AccentColor-1", "AccentText");
        }

        private void MainPage_OrientationChanged(DisplayInformation info, object args) {
            whichOrientation(info);
        }

        private void StatusBarAndTitleBar(string statusBarBackgroundColor, string statusBarForegroundColor) {
            //PC customization
            if (ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView")) {
                var titleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (titleBar != null) {
                    titleBar.ButtonBackgroundColor = ((SolidColorBrush)Application.Current.Resources["AccentColor-1"] as SolidColorBrush).Color;
                    titleBar.ButtonForegroundColor = Colors.White;

                    titleBar.BackgroundColor = ((SolidColorBrush)Application.Current.Resources["AccentColor-1"] as SolidColorBrush).Color;
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

        private void OperationsAppBarRadioButton_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Strona_glowna));
        }

        private void CategoriesAppBarRadioButton_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Kategorie));
        }

        private void AddNewOperationAppBarRadioButton_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Nowa_Operacja));
        }

        private void StatisticsAppBarRadioButton_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Statystyki));
        }
        private void RadioButton_Click(object sender, RoutedEventArgs e) {
            MoreAppBarRadioButton.IsChecked = false;
            CommandBar.IsOpen = !CommandBar.IsOpen;
        }

        private void SzablonyAppBarButton_Click(object sender, RoutedEventArgs e) {
            UncheckAllMenuButtons();
            AktualnaStrona_Frame.Navigate(typeof(Szablony));
        }

        private void ZleceniaStaleAppBarButton_Click(object sender, RoutedEventArgs e) {
            UncheckAllMenuButtons();
            AktualnaStrona_Frame.Navigate(typeof(ZleceniaStale));
        }

        private void KontaAppBarButton_Click(object sender, RoutedEventArgs e) {
            UncheckAllMenuButtons();
            AktualnaStrona_Frame.Navigate(typeof(Konta));
        }

        private void UstawieniaAppBarButton_Click(object sender, RoutedEventArgs e) {
            UncheckAllMenuButtons();
            AktualnaStrona_Frame.Navigate(typeof(Ustawienia));
        }

        private void CommandBar_Opening(object sender, object e) {
            OperationsAppBarRadioButton.Height = Double.NaN;
            CategoriesAppBarRadioButton.Height = Double.NaN;
            AddNewOperationAppBarRadioButton.Height = Double.NaN;
            StatisticsAppBarRadioButton.Height = Double.NaN;
        }

        private void CommandBar_Closing(object sender, object e) {
            OperationsAppBarRadioButton.Height = 40;
            CategoriesAppBarRadioButton.Height = 40;
            AddNewOperationAppBarRadioButton.Height = 40;
            StatisticsAppBarRadioButton.Height = 40;
        }

        private void UncheckAllMenuButtons() {
            OperationsAppBarRadioButton.IsChecked = false;
            CategoriesAppBarRadioButton.IsChecked = false;
            AddNewOperationAppBarRadioButton.IsChecked = false;
            StatisticsAppBarRadioButton.IsChecked = false;
        }

        private void AktualnaStrona_Frame_Navigated(object sender, NavigationEventArgs e) {
            if (((Frame)sender).SourcePageType == typeof(Strona_glowna)) {
                OperationsAppBarRadioButton.IsChecked = true;
            }
        }
    }
}
