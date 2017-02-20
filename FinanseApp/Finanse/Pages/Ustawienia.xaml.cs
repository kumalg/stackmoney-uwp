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

    public sealed partial class Ustawienia : Page {
       
        public Ustawienia() {

            this.InitializeComponent();

            foreach (CultureInfo item in Settings.getAllCurrencies()) {
                CurrencyValue.Items.Add(new ComboBoxItem {
                    Content = new RegionInfo(item.Name).ISOCurrencySymbol,
                    Tag = item.Name
                });
            }

            if (Settings.getTheme() == ApplicationTheme.Dark)
                DarkThemeRadioButton.IsChecked = true;
            else
                LightThemeRadioButton.IsChecked = true;

            DarkThemeRadioButton.Checked += DarkThemeRadioButton_Checked;
            LightThemeRadioButton.Checked += LightThemeRadioButton_Checked;

            CurrencyValue.SelectedItem = CurrencyValue
                .Items
                .OfType<ComboBoxItem>()
                .SingleOrDefault(i => i.Tag.Equals(Settings.getActualCultureInfo().Name));

            for (int i = 1; i <= 12; i++)
                MaxNumberOfNextMonth.Items.Add(new ComboBoxItem {
                    Content = i,
                });

            MaxNumberOfNextMonth.SelectedIndex = Settings.getMaxFutureMonths() - 1;
            CategoryNameVisibilityToggleButton.IsOn = Settings.getCategoryNameVisibility();
            AccountEllipseVisibilityToggleButton.IsOn = Settings.getAccountEllipseVisibility();
           
            SyncSettingsToggleButton.IsOn = Settings.getSyncSettings();
            SyncSettingsToggleButton.Toggled += SyncSettingsToggleButton_Toggled;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            Settings.setActualCultureInfo((string)((ComboBoxItem)CurrencyValue.SelectedItem).Tag);
        }

        private void MaxNumberOfFutureMonths_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBoxItem item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
            Settings.setMaxFutureMonths((int)item.Content);
        }

        private void CategoryNameVisibilityToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.setCategoryNameVisibility(CategoryNameVisibilityToggleButton.IsOn);
        }

        private void AccountEllipseVisibilityToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.setAccountEllipseVisibility(AccountEllipseVisibilityToggleButton.IsOn);
        }

        private void ReminderToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (ReminderStackPanel != null)
                ReminderStackPanel.Visibility = ((ToggleSwitch)sender).IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void FirstDayOfWeek_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBoxItem item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
         //   Settings.SetFirstDayOfWeek((DayOfWeek)item.Tag);
        }

        private void SyncSettingsToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.setSyncSettings(SyncSettingsToggleButton.IsOn);
        }

        private void DarkThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.setTheme(1);
        }

        private void LightThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.setTheme(0);
        }
        public string GetAppVersion() {

            Package package = Package.Current;
            PackageId packageId = package.Id;
            PackageVersion version = packageId.Version;

            return string.Format("{0}.{1}.{2}.{3}", version.Major, version.Minor, version.Build, version.Revision);

        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            showDeleteAllContentDialog();
        }
        private async void showDeleteAllContentDialog() {

            Dialogs.AcceptContentDialog acceptContentDialog = new Dialogs.AcceptContentDialog("Jesteś pewien?");

            ContentDialogResult result = await acceptContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                Dal.DeleteAll();
                Dal.AddInitialElements();
            }
        }
    }
}
