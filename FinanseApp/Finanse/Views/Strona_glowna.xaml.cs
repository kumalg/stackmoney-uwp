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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Strona_glowna : Page
    {
        public ObservableCollection<Wydatek> Wydatki;

        //private ObservableCollection<Wplyw> Wplywy;

        public Strona_glowna()
        {
            this.InitializeComponent();
            Wydatki = new ObservableCollection<Wydatek>();
            //Wplywy = new ObservableCollection<Wplyw>();
        }

        private void Wplyw_Tapped(object sender, TappedRoutedEventArgs e)
        {
           
        }
            
        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e)
        {
            var ContentDialogItem = new NowaOperacjaContentDialog(Wydatki);
            var result = await ContentDialogItem.ShowAsync();
        }
    }
}
