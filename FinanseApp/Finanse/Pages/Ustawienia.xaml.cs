using Finanse.Models;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Pages {

    public sealed partial class Ustawienia : Page {

        private ObservableCollection<ComboBoxItem> daysOfWeek = new ObservableCollection<ComboBoxItem> {
            new ComboBoxItem {
                Content = "ff",
                Tag = DayOfWeek.Sunday
            },
            new ComboBoxItem {
                Content = "d",
                Tag = DayOfWeek.Monday
            }
        };
        
        public Ustawienia() {

            this.InitializeComponent();

            foreach (CultureInfo item in Settings.GetAllCurrencies()) {

                CurrencyValue.Items.Add(new ComboBoxItem {
                    Content = item.DisplayName,
                    Tag = item.Name
                });
            }
            
            FirstDayOfWeek_ComboBox.Items.Add(
                new ComboBoxItem {
                    Content = "Niedziela",
                    Tag = DayOfWeek.Sunday
                }
            );

            FirstDayOfWeek_ComboBox.Items.Add(
                new ComboBoxItem {
                    Content = "Poniedziałek",
                    Tag = DayOfWeek.Monday
                }
            );

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
