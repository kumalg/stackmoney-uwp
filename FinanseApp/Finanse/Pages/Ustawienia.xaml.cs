using Finanse.Models;
using System;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Pages {

    public sealed partial class Ustawienia : Page {
       
        public Ustawienia() {

            this.InitializeComponent();

            foreach (CultureInfo item in Settings.GetAllCurrencies()) {

                CurrencyValue.Items.Add(new ComboBoxItem {
                    Content = item.DisplayName,
                    Tag = item.Name
                });
            }
            /*
            if (Settings.GetActualIconStyle() == "Segoe UI") {
                ColorIcon_RadioButton.IsChecked = true;
            }
            else
                MonoIcon_RadioButton.IsChecked = true;
                */
           // ThemeToggle.IsOn = (Settings.GetTheme() == ApplicationTheme.Dark);
            if (Settings.GetTheme() == ApplicationTheme.Dark)
                DarkThemeRadioButton.IsChecked = true;
            else
                LightThemeRadioButton.IsChecked = true;

            DarkThemeRadioButton.Checked += DarkThemeRadioButton_Checked;
            LightThemeRadioButton.Checked += LightThemeRadioButton_Checked;

            CurrencyValue.SelectedItem = CurrencyValue
                .Items
                .OfType<ComboBoxItem>()
                .SingleOrDefault(i => i.Content.ToString() == Settings.GetActualCurrency().DisplayName);

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
            Settings.SetActualCurrency((string)((ComboBoxItem)CurrencyValue.SelectedItem).Tag);
        }
        /*
        private void IconStyleRadioButton_Checked(object sender, RoutedEventArgs e) {

            Settings.SetActualIconStyle(((RadioButton)sender).Tag.ToString());
        }
        */
        private void ThemeToggle_Toggled(object sender, RoutedEventArgs e) {
            Settings.SetTheme(ThemeToggle.IsOn ? 1 : 0);
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

        private void FirstDayOfWeek_ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ComboBoxItem item = (ComboBoxItem)((ComboBox)sender).SelectedItem;
         //   Settings.SetFirstDayOfWeek((DayOfWeek)item.Tag);
        }

        private void SyncSettingsToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.SetSyncSettings(SyncSettingsToggleButton.IsOn);
        }

        private void DarkThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.SetTheme(1);
        }

        private void LightThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.SetTheme(0);
        }
    }
}
