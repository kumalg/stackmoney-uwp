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

namespace Finanse
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Strona_glowna : Page
    {
        private ObservableCollection<Wydatek> Wydatki;

        private ObservableCollection<Wplyw> Wplywy;

        public Strona_glowna()
        {
            this.InitializeComponent();

            Wydatki = new ObservableCollection<Wydatek>();

            Wplywy = new ObservableCollection<Wplyw>();
        }

        private void Wplyw_Tapped(object sender, TappedRoutedEventArgs e)
        {
           
        }

        private void NewWydatekButton_Click(object sender, RoutedEventArgs e)
        {
            Wydatki.Add(new Wydatek { Title = TitleTextBox.Text, Type = TypeTextBox.Text, Cost = CostTextBox.Text });

            TitleTextBox.Text = "";
            TypeTextBox.Text = "";
            CostTextBox.Text = "";

            TitleTextBox.Focus(FocusState.Programmatic);
        }

        private void WydatekButton_Click(string NameValue, string CostValue, string CategoryValueItem, DateTimeOffset? DateValue)
        {
            Wydatki.Add(new Wydatek
            {
                Title = NameValue,
                Cost = CostValue,
                Type = CategoryValueItem,
                Date = DateValue,
            });
        }

        private void IconsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e)
        {
            var ContentDialogItem = new NowaOperacjaContentDialog();
            var result = await ContentDialogItem.ShowAsync();

            /*
            var btn = sender as Button;
            var dialog = new ContentDialog()
            {
                Title = "Nowa operacja",
                RequestedTheme = ElementTheme.Dark,
                //FullSizeDesired = true,
                MaxWidth = this.ActualWidth // Required for Mobile!
            };

            // Setup Content
            var panel = new StackPanel();

            panel.Children.Add(new TextBlock
            {
                Text = "Typ operacji",
                Margin = new Thickness(0,20,0,10),
            });

            var panel2 = new StackPanel() { Orientation = Orientation.Horizontal };

            panel2.Children.Add(new RadioButton
            {
                Content = "Wpływ",
                GroupName = "CostType",
            });

            panel2.Children.Add(new RadioButton
            {
                Content = "Wydatek",
                GroupName = "CostType",
            });

            panel.Children.Add(panel2);

            var CostValue = new TextBox
            {
                PlaceholderText = "Wartość",
                Margin = new Thickness(0, 10, 0, 10),
            };

            InputScope scope = new InputScope();
            InputScopeName name = new InputScopeName();

            name.NameValue = InputScopeNameValue.CurrencyAmount;
            scope.Names.Add(name);

            CostValue.InputScope = scope;

            panel.Children.Add(CostValue);


            var NameValue = new TextBox
            {
                PlaceholderText = "Nazwa",
                Margin = new Thickness(0, 10, 0, 10),
            };
            panel.Children.Add(NameValue);

            var panel3 = new StackPanel { Orientation = Orientation.Horizontal };

            var DateValue = new CalendarDatePicker
            {
                PlaceholderText = "Data",
                Margin = new Thickness(0, 10, 0, 10),
            };
            panel3.Children.Add(DateValue);

            var CategoryValue = new ComboBox
            {
                PlaceholderText = "Kategoria",
                Margin = new Thickness(10, 10, 0, 10),
            };
            panel3.Children.Add(CategoryValue);

            CategoryValue.Items.Add("Jedzenie");
            CategoryValue.Items.Add("Transport");
            CategoryValue.Items.Add("Mieszkanie");

            panel.Children.Add(panel3);

            var PayFormValue = new ComboBox
            {
                PlaceholderText = "Forma płatności",
                Margin = new Thickness(0, 10, 0, 10),
            };
            PayFormValue.Items.Add("Gotówka");
            PayFormValue.Items.Add("Karta Visa");

            panel.Children.Add(PayFormValue);

            var cb = new CheckBox
            {
                Content = "Zapisz jako szablon",
                Margin = new Thickness(0, 10, 0, 10),
            };

            cb.SetBinding(CheckBox.IsCheckedProperty, new Binding
            {
                Source = dialog,
                Path = new PropertyPath("IsPrimaryButtonEnabled"),
                Mode = BindingMode.TwoWay,
            });

            panel.Children.Add(cb);
            dialog.Content = panel;

            // Add Buttons
            dialog.PrimaryButtonText = "Dodaj";
            dialog.IsPrimaryButtonEnabled = false;
            dialog.PrimaryButtonClick += delegate
            {
                WydatekButton_Click(NameValue.Text, CostValue.Text, CategoryValue.SelectedItem.ToString(), DateValue.Date);
            };

            dialog.SecondaryButtonText = "Anuluj";
            dialog.SecondaryButtonClick += delegate
            {
                btn.Content = "Result: Cancel";
            };

            // Show Dialog
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.None)
            {
                btn.Content = "Result: NONE";
            }*/
        }
    }
}
