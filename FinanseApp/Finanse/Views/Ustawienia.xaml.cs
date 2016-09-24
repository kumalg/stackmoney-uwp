using Finanse.Elements;
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

namespace Finanse.Views {

    public sealed partial class Ustawienia : Page {

        public Ustawienia() {

            this.InitializeComponent();

            foreach (CultureInfo item in Settings.GetAllCurrencies()) {

                CurrencyValue.Items.Add(new ComboBoxItem {
                    Content = item.DisplayName,
                    Tag = item.Name
                });
            }

            foreach (string item in Settings.GetAllFonts()) {

                IconValue.Items.Add(new ComboBoxItem {
                    Content = item
                });
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            Settings.SetActualCurrency((string)((ComboBoxItem)CurrencyValue.SelectedItem).Tag);
        }

        private void IconValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            Settings.SetActualIconStyle(((ComboBoxItem)IconValue.SelectedItem).Content.ToString());
        }
    }
}
