using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Finanse.Elements;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class Strona_glowna : Page {

        public ObservableCollection<Wydatek> Wydatki = new ObservableCollection<Wydatek>();

        public static List<OperationCategory> OperationCategories = new List<OperationCategory>();

        public Strona_glowna() {

            this.InitializeComponent();

            Wydatki.Add(new Wydatek {
                Title = "PKP Intercity",
                Cost = 17.10F,
                Category = "Transport"  
            });

            Wydatki.Add(new Wydatek {
                Title = "Biedronka",
                Cost = 23F,
                Category = "Jedzenie"
            });

            Wydatki.Add(new Wydatek {
                Title = "Drink Hala",
                Cost = 7.99F,
                Category = "Alkohol"
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Transport",
                Color = "#FF0b63c7",
                Icon = "",
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Jedzenie",
                Color = "#FF5bc70b",
                Icon = "",
            });

            OperationCategories.Add(new OperationCategory {
                Name = "Alkohol",
                Color = "#FF138b99",
                Icon = "",
            });
        }

        private void Wplyw_Tapped(object sender, TappedRoutedEventArgs e) {

        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e) {

            var ContentDialogItem = new NowaOperacjaContentDialog(Wydatki, OperationCategories);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
             FrameworkElement senderElement = sender as FrameworkElement;
             FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
             flyoutBase.ShowAt(senderElement);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            //this datacontext is probably some object of some type T
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            //this datacontext is probably some object of some type T
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }
    }
}
