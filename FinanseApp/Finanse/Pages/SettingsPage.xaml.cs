using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using System;
using System.Globalization;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Finanse.Models.Helpers;

namespace Finanse.Pages {

    public sealed partial class SettingsPage : Page {

        public string AppVersion = Informations.AppVersion;
        public string DisplayName = Informations.DisplayName;
        public string PublisherDisplayName = Informations.PublisherDisplayName;

        public SettingsPage() {

            this.InitializeComponent();

            foreach (CultureInfo item in Settings.AllCurrencies) {
                CurrencyValue.Items.Add(new ComboBoxItem {
                    Content = new RegionInfo(item.Name).ISOCurrencySymbol,
                    Tag = item.Name
                });
            }

            if (Settings.Theme== ApplicationTheme.Dark)
                DarkThemeRadioButton.IsChecked = true;
            else
                LightThemeRadioButton.IsChecked = true;

            DarkThemeRadioButton.Checked += DarkThemeRadioButton_Checked;
            LightThemeRadioButton.Checked += LightThemeRadioButton_Checked;

            CurrencyValue.SelectedItem = CurrencyValue
                .Items
                .OfType<ComboBoxItem>()
                .SingleOrDefault(i => i.Tag.Equals(Settings.ActualCultureInfo.Name));

            for (int i = 1; i <= 12; i++)
                MaxNumberOfNextMonth.Items.Add(new ComboBoxItem {
                    Content = i,
                });

            MaxNumberOfNextMonth.SelectedIndex = Settings.MaxFutureMonths- 1;
            CategoryNameVisibilityToggleButton.IsOn = Settings.CategoryNameVisibility;
            AccountEllipseVisibilityToggleButton.IsOn = Settings.AccountEllipseVisibility;
           
            SyncSettingsToggleButton.IsOn = Settings.SyncSettings;
            SyncSettingsToggleButton.Toggled += SyncSettingsToggleButton_Toggled;

            SyncDataToggleButton.IsOn = Settings.SyncData;
            SyncDataToggleButton.Toggled += SyncDataToggleButton_Toggled;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var cultureString = (string)( (ComboBoxItem)CurrencyValue.SelectedItem ).Tag;
            Settings.ActualCultureInfo = new CultureInfo(cultureString);
        }

        private void MaxNumberOfFutureMonths_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            var item = (ComboBoxItem)( (ComboBox)sender ).SelectedItem;
            if (item?.Content != null)
                Settings.MaxFutureMonths = (int)item.Content;
        }

        private void CategoryNameVisibilityToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.CategoryNameVisibility = CategoryNameVisibilityToggleButton.IsOn;
        }

        private void AccountEllipseVisibilityToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.AccountEllipseVisibility = AccountEllipseVisibilityToggleButton.IsOn;
        }

        private void ReminderToggleSwitch_Toggled(object sender, RoutedEventArgs e) {
            if (ReminderStackPanel != null)
                ReminderStackPanel.Visibility = ((ToggleSwitch)sender).IsOn ? Visibility.Visible : Visibility.Collapsed;
        }

        private void SyncSettingsToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.SyncSettings = SyncSettingsToggleButton.IsOn;
        }

        private void SyncDataToggleButton_Toggled(object sender, RoutedEventArgs e) {
            Settings.SyncData = SyncDataToggleButton.IsOn;
        }

        private void DarkThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.Theme = ApplicationTheme.Dark;

            if (Frame != null) {
                ( (ThemeAwareFrame)Frame ).AppTheme = ElementTheme.Dark;
            }
        }

        private void LightThemeRadioButton_Checked(object sender, RoutedEventArgs e) {
            Settings.Theme = ApplicationTheme.Light;

            if (Frame != null) {
                ( (ThemeAwareFrame)Frame ).AppTheme = ElementTheme.Light;
            }
        }

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

        private void MarkdownText_MarkdownRendered(object sender, Microsoft.Toolkit.Uwp.UI.Controls.MarkdownRenderedEventArgs e) {

        }

        private string whatsNewText =
            "###1.1.5.0\n" +
            "* synchronizacja operacji\n" +
            "* suma pieniędzy na poczatku i końcu każdego miesiąca\n" +
            "* poprawiony sposób wyświetlania aktualnego salda na stronie \"Konta\"\n" +
            "* zabezpieczenie przed usunięciem domyślnej kategorii\n" +
            "* zabezpieczenie przed usunięciem wszystkich kont\n" +
            "* zmiana motywu jasny/ciemny następuje natychmiast (jednak wciąż z drobnymi kłopotami)\n" +
            "* naprawiony brak możliwości scrollowania kategorii dla trybu kompaktowego\n" +
            "* poprawki drobnych błędów\n" +
            "\n\n\n###1.1.4.0\n" + 
            "* poprawki wydajności\n" +
            "* zabezpieczenie przed dodaniem kategorii albo konta o takiej samej nazwie\n" +
            "* poprawki w tłumaczeniu\n" +
            "* podstrona 'Co nowego'\n" +
            "\n\n\n###1.1.3.0\n" +
            "* Pierwszy build";
    }
}
