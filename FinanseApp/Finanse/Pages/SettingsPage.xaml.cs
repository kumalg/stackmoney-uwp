using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using Finanse.Models.MoneyAccounts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Pages {

    public sealed partial class SettingsPage : Page {
       
        public SettingsPage() {

            this.InitializeComponent();

            foreach (CultureInfo item in Settings.GetAllCurrencies()) {
                CurrencyValue.Items.Add(new ComboBoxItem {
                    Content = new RegionInfo(item.Name).ISOCurrencySymbol,
                    Tag = item.Name
                });
            }

            if (Settings.GetTheme() == ApplicationTheme.Dark)
                DarkThemeRadioButton.IsChecked = true;
            else
                LightThemeRadioButton.IsChecked = true;

            DarkThemeRadioButton.Checked += DarkThemeRadioButton_Checked;
            LightThemeRadioButton.Checked += LightThemeRadioButton_Checked;

            CurrencyValue.SelectedItem = CurrencyValue
                .Items
                .OfType<ComboBoxItem>()
                .SingleOrDefault(i => i.Tag.Equals(Settings.GetActualCultureInfo().Name));

            for (int i = 1; i <= 12; i++)
                MaxNumberOfNextMonth.Items.Add(new ComboBoxItem {
                    Content = i,
                });

            MaxNumberOfNextMonth.SelectedIndex = Settings.GetMaxFutureMonths() - 1;
            CategoryNameVisibilityToggleButton.IsOn = Settings.GetCategoryNameVisibility();
            AccountEllipseVisibilityToggleButton.IsOn = Settings.GetAccountEllipseVisibility();
           
            SyncSettingsToggleButton.IsOn = Settings.GetSyncSettings();
            SyncSettingsToggleButton.Toggled += SyncSettingsToggleButton_Toggled;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Settings.SetActualCultureInfo((string)((ComboBoxItem)CurrencyValue.SelectedItem).Tag);
        }

        private void MaxNumberOfFutureMonths_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBoxItem item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            Settings.SetMaxFutureMonths((int)item.Content);
        }

        private void CategoryNameVisibilityToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.SetCategoryNameVisibility(CategoryNameVisibilityToggleButton.IsOn);
        }

        private void AccountEllipseVisibilityToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.SetAccountEllipseVisibility(AccountEllipseVisibilityToggleButton.IsOn);
        }

        private void ReminderToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (ReminderStackPanel != null)
                ReminderStackPanel.Visibility = ((ToggleSwitch)sender).IsOn ? Visibility.Visible : Visibility.Collapsed;
        }
        /*
        private void FirstDayOfWeek_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBoxItem item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
         //   Settings.SetFirstDayOfWeek((DayOfWeek)item.Tag);
        }
        */
        private void SyncSettingsToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.SetSyncSettings(SyncSettingsToggleButton.IsOn);
        }

        private void DarkThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.SetTheme(1);
        }

        private void LightThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.SetTheme(0);
        }
        public string AppVersion {
            get {
                Package package = Package.Current;
                PackageId packageId = package.Id;
                PackageVersion version = packageId.Version;

                return $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
            }
        }

        public string DisplayName => Package.Current.DisplayName;
        public string PublisherDisplayName => Package.Current.PublisherDisplayName;

        private void Button_Click(object sender, RoutedEventArgs e) {
            ShowDeleteAllContentDialog();
        }

        private static async void ShowDeleteAllContentDialog() {

            AcceptContentDialog acceptContentDialog = new AcceptContentDialog("Jesteś pewien?");

            ContentDialogResult result = await acceptContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            Dal.DeleteAll();
            Dal.AddInitialElements();
        }
    }
}
