using Finanse.Elements;
using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

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

            if (Settings.GetActualIconStyle() == "Segoe UI") {
                ColorIcon_RadioButton.IsChecked = true;
            }
            else
                MonoIcon_RadioButton.IsChecked = true;

            ThemeToggle.IsOn = (Settings.GetTheme() == ApplicationTheme.Dark);

            CurrencyValue.SelectedItem = CurrencyValue.Items.SingleOrDefault(i => ((ComboBoxItem)i).Content.ToString() == Settings.GetActualCurrency().DisplayName);

            for (int i = 1; i <= 12; i++)
                MaxNumberOfNextMonth.Items.Add(new ComboBoxItem {
                    Content = i,
                });

            MaxNumberOfNextMonth.SelectedIndex = Settings.GetMaxFutureMonths() - 1;
            CategoryNameVisibilityToggleButton.IsOn = Settings.GetCategoryNameVisibility();
            AccountEllipseVisibilityToggleButton.IsOn = Settings.GetAccountEllipseVisibility();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            Settings.SetActualCurrency((string)((ComboBoxItem)CurrencyValue.SelectedItem).Tag);
        }

        private void IconStyleRadioButton_Checked(object sender, RoutedEventArgs e) {

            Settings.SetActualIconStyle(((RadioButton)sender).Tag.ToString());
        }

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
    }
}
