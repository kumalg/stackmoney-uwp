using System;
using System.Collections.Generic;
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
using Finanse.Elements;
using System.Collections.ObjectModel;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class NowaOperacjaContentDialog : ContentDialog {

        public ObservableCollection<Wydatek> Wydatki;

        public List<OperationCategory> OperationCategories;

        public NowaOperacjaContentDialog(ObservableCollection<Wydatek> Wydatki, List<OperationCategory> OperationCategories) {

            this.InitializeComponent();
            this.Wydatki = Wydatki;
            this.OperationCategories = OperationCategories;
            DateValue.MaxDate = DateTime.Today;

            CategoryValue.Items.Add(new ComboBoxItem {
                Content = OperationCategories[0].Name,
            });
            CategoryValue.Items.Add(new ComboBoxItem {
                Content = OperationCategories[1].Name,
            });
            CategoryValue.Items.Add(new ComboBoxItem {
                Content = OperationCategories[2].Name,
            });

        }

        private void NowaOperacja_AnulujClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

        }

        private void NowaOperacja_DodajClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            Wydatki.Add(new Wydatek() {

                Title = NameValue.Text,
                CostString = CostValue.Text,
                Category = ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString(),
                Date = DateValue.Date,
            });
        }
    }
}
