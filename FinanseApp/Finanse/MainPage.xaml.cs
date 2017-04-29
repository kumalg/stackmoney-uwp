using System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Finanse.Pages;
using Finanse.DataAccessLayer;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using Windows.UI.Core;
using Windows.Foundation;
using Windows.ApplicationModel.Core;
using Windows.Foundation.Metadata;
using Windows.Storage;
using Windows.UI.Xaml.Media.Animation;
using Finanse.Models;
using Finanse.Models.Helpers;
using Finanse.Models.WhatsNew;
using Microsoft.Toolkit.Uwp;

namespace Finanse {
    public sealed partial class MainPage {

        public string DisplayName = Informations.DisplayName;

        public MainPage() {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize() {
            bool exist = await ApplicationData.Current.LocalFolder.FileExistsAsync("db.sqlite");
            if (!exist)
                await SyncHelper.CopyDbFileFromOneDrive();

            DalBase.CreateBackup();
            DalBase.CreateDb();

            DalBase.ConvertAccounts();
            DalBase.ConvertCategories();
            DalBase.RepairDb();
            DalBase.ConvertLocalIdReferenceToGlobal();

            DalBase.AddItemsIfEmpty();

            ApplicationView.GetForCurrentView().SetPreferredMinSize(new Size(360, 530));

            SetTitleBar();

            if (OperationsAppBarRadioButton != null)
                OperationsAppBarRadioButton.IsChecked = true;

            if (Strona_glowna_ListBoxItem != null)
                Strona_glowna_ListBoxItem.IsChecked = true;

            WhatsNewMessageDialog.CheckForNewerAppVersion();
            SyncHelper.CheckSyncBase();
        }

        private void SetTitleBar() {
            CoreApplication.GetCurrentView().TitleBar.IsVisibleChanged += TitleBar_IsVisibleChanged;
            
            ApplicationViewTitleBar formattableTitleBar = ApplicationView.GetForCurrentView().TitleBar;
            formattableTitleBar.ButtonBackgroundColor = Colors.Transparent;
            CoreApplicationViewTitleBar coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = true;
            formattableTitleBar.ButtonHoverBackgroundColor = Functions.GetSolidColorBrush("#19000000").Color;

            formattableTitleBar.ButtonForegroundColor = Settings.Theme == ApplicationTheme.Dark
                ? Colors.White
                : Colors.Black;
        }
        private void TitleBar_IsVisibleChanged(CoreApplicationViewTitleBar sender, object args) {
            TitleBar.Visibility = sender.IsVisible ? Visibility.Visible : Visibility.Collapsed;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e) {
            if (AktualnaStrona_Frame.CurrentSourcePageType == typeof(OperationsPage))
                return;

            OperationsAppBarRadioButton.IsChecked = true;
            e.Handled = true;
        }
        private void BackRequestedEvent(object sender, BackRequestedEventArgs e) {
            if (AktualnaStrona_Frame.CurrentSourcePageType == typeof(OperationsPage))
                return;

            AktualnaStrona_Frame.Navigate(typeof(OperationsPage));
            e.Handled = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            SystemNavigationManager.GetForCurrentView().BackRequested += BackRequestedEvent;
            //BackButton.Click += BackRequest;
            base.OnNavigatedTo(e);
        }


        private void OperationsAppBarRadioButton_Click(object sender, RoutedEventArgs e) {

        }

        private void CloseCommandBar() {
            if (CommandBar != null)
                CommandBar.IsOpen = false;
        }

        private async void OperationsAppBarRadioButton_Checked(object sender, RoutedEventArgs e) {
            CloseCommandBar();

            if (Strona_glowna_ListBoxItem != null)
                Strona_glowna_ListBoxItem.IsChecked = true;
            if (OperationsAppBarRadioButton != null)
                OperationsAppBarRadioButton.IsChecked = true;

            await Task.Delay(5);
            if (AktualnaStrona_Frame.CurrentSourcePageType != typeof(OperationsPage))
                AktualnaStrona_Frame.Navigate(typeof(OperationsPage));
        }

        private async void CategoriesAppBarRadioButton_Checked(object sender, RoutedEventArgs e) {
            CloseCommandBar();

            if (Kategorie_ListBoxItem != null)
                Kategorie_ListBoxItem.IsChecked = true;
            if (CategoriesAppBarRadioButton != null)
                CategoriesAppBarRadioButton.IsChecked = true;

            await Task.Delay(5);
            AktualnaStrona_Frame.Navigate(typeof(CategoriesPage));
        }

        private async void AddNewOperationAppBarRadioButton_Checked(object sender, RoutedEventArgs e) {
            CloseCommandBar();

            if (AddNewOperation_ListBoxItem != null)
                AddNewOperation_ListBoxItem.IsChecked = true;
            if (AddNewOperationAppBarRadioButton != null)
                AddNewOperationAppBarRadioButton.IsChecked = true;

            await Task.Delay(5);
            AktualnaStrona_Frame.Navigate(typeof(NewOperationPage));
        }

        private async void StatisticsAppBarRadioButton_Checked(object sender, RoutedEventArgs e) {
            CloseCommandBar();

            if (Statystyki_ListBoxItem != null)
                Statystyki_ListBoxItem.IsChecked = true;
            if (StatisticsAppBarRadioButton != null)
                StatisticsAppBarRadioButton.IsChecked = true;

            await Task.Delay(5);
            AktualnaStrona_Frame.Navigate(typeof(StatisticsPage));
        }

        private async void RadioButton_Click(object sender, RoutedEventArgs e) {
            MoreAppBarRadioButton.IsChecked = false;
            await Task.Delay(5);
            CommandBar.IsOpen = !CommandBar.IsOpen;
        }

        private void NavigateIfDifferentPage(Type pageType) {
            if (AktualnaStrona_Frame.CurrentSourcePageType != pageType)
                AktualnaStrona_Frame.Navigate(pageType);
        }

        private async void SzablonyAppBarButton_Click(object sender, RoutedEventArgs e) {
            UncheckAllMenuButtons();

            if (Szablony_ListBoxItem != null)
                Szablony_ListBoxItem.IsChecked = true;

            await Task.Delay(5);
            NavigateIfDifferentPage(typeof(OperationPatternsPage));
        }

        private async void ZleceniaStaleAppBarButton_Click(object sender, RoutedEventArgs e) {
            UncheckAllMenuButtons();

            if (ZleceniaStale_ListBoxItem != null)
                ZleceniaStale_ListBoxItem.IsChecked = true;

            await Task.Delay(5);
            NavigateIfDifferentPage(typeof(StandingOrdersPage));
        }

        private async void KontaAppBarButton_Click(object sender, RoutedEventArgs e) {
            UncheckAllMenuButtons();

            if (Konta_ListBoxItem != null)
                Konta_ListBoxItem.IsChecked = true;

            await Task.Delay(5);
            NavigateIfDifferentPage(typeof(AccountsPage));
        }

        private async void UstawieniaAppBarButton_Click(object sender, RoutedEventArgs e) {
            UncheckAllMenuButtons();

            if (Ustawienia_ListBoxItem != null)
                Ustawienia_ListBoxItem.IsChecked = true;

            await Task.Delay(5);
            NavigateIfDifferentPage(typeof(SettingsPage));
        }

        private void CommandBar_Opening(object sender, object e) {
            OperationsAppBarRadioButton.Height = double.NaN;
            CategoriesAppBarRadioButton.Height = double.NaN;
            AddNewOperationAppBarRadioButton.Height = double.NaN;
            StatisticsAppBarRadioButton.Height = double.NaN;
        }

        private void CommandBar_Closing(object sender, object e) {
            OperationsAppBarRadioButton.Height = 40;
            CategoriesAppBarRadioButton.Height = 40;
            AddNewOperationAppBarRadioButton.Height = 40;
            StatisticsAppBarRadioButton.Height = 40;
        }

        private void UncheckAllMenuButtons() {
            if (AddNewOperationAppBarRadioButton == null)
                return;

            OperationsAppBarRadioButton.IsChecked = false;
            CategoriesAppBarRadioButton.IsChecked = false;
            AddNewOperationAppBarRadioButton.IsChecked = false;
            StatisticsAppBarRadioButton.IsChecked = false;
        }
        
        private void AktualnaStrona_Frame_Navigated(object sender, NavigationEventArgs e) {
            if (((Frame)sender).SourcePageType == typeof(OperationsPage)) {
                //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
                if (BackButton != null)
                    BackButton.Visibility = Visibility.Collapsed;

                TitleBar.Margin = new Thickness(16, 0, 0, 0);
                if (OperationsAppBarRadioButton != null)
                    OperationsAppBarRadioButton.IsChecked = true;
                if (Strona_glowna_ListBoxItem != null)
                    Strona_glowna_ListBoxItem.IsChecked = true;
            }
            else if (( (Frame)sender ).SourcePageType == typeof(NewOperationPage)) {
                //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                if (BackButton != null)
                    BackButton.Visibility = Visibility.Visible;

                TitleBar.Margin = new Thickness(64, 0, 0, 0);
                if (AddNewOperationAppBarRadioButton != null)
                    AddNewOperationAppBarRadioButton.IsChecked = true;
                if (AddNewOperation_ListBoxItem != null)
                    AddNewOperation_ListBoxItem.IsChecked = true;
            }
            else {
                //SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
                if (BackButton != null)
                    BackButton.Visibility = Visibility.Visible;

                TitleBar.Margin = new Thickness(64, 0, 0, 0);
            }
        }

        private void MainPage_SizeChanged(object sender, SizeChangedEventArgs e) {
            StatusBarMethods.WhichOrientation();
            StatusBarMethods.SetStatusBarColors();

            if (TitleBarRowDefinition != null)
                TitleBarRowDefinition.Height = ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar") && ActualWidth < 800
                    ? new GridLength(0)
                    : new GridLength(31);

            TitleBar.Visibility = ActualWidth < 800 
                ? Visibility.Visible 
                : Visibility.Collapsed;
        } 
        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            var sb = (int) Pane.Width == 320
                ? Resources["ClosePane"] as Storyboard
                : Resources["OpenPane"] as Storyboard;

            sb?.Begin();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e) {
            if (AktualnaStrona_Frame.CurrentSourcePageType != typeof(OperationsPage))
                AktualnaStrona_Frame.Navigate(typeof(OperationsPage));
        }
    }
}
