using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Foundation.Metadata;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Finanse.Elements;
using Finanse.Views;
using System.Collections.ObjectModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Finanse {
    public sealed partial class MainPage : Page {

        public MainPage() {
            this.InitializeComponent();
            AktualnaStrona_Frame.Navigate(typeof(Strona_glowna));
            Strona_glowna_ListBoxItem.IsChecked = true;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            base.OnNavigatedTo(e);
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e) {
            MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }

        private void Strona_glowna_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Strona_glowna));
            ClosingPane();
        }

        private void Kategorie_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Kategorie));
            ClosingPane();
        }

        private void Szablony_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Szablony));
            ClosingPane();
        }

        private void ZleceniaStale_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(ZleceniaStale));
            ClosingPane();
        }

        private void PlanowaneWydatki_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(PlanowaneWydatki));
            ClosingPane();
        }

        private void Ustawienia_ListBoxItem_Checked(object sender, RoutedEventArgs e) {
            AktualnaStrona_Frame.Navigate(typeof(Ustawienia));
            ClosingPane();
        }

        private void ClosingPane() {
            if (MySplitView.IsPaneOpen)
                MySplitView.IsPaneOpen = !MySplitView.IsPaneOpen;
        }
    }
}
