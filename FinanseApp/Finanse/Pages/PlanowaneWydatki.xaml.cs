using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

    public sealed partial class PlanowaneWydatki : Page {

        private ObservableCollection<GroupInfoList<Operation>> _source;
        private FontFamily iconStyle = new FontFamily(Settings.GetActualIconStyle());

        decimal actualMoney;
        int actualMonth;
        int actualYear;

        public PlanowaneWydatki() {

            this.InitializeComponent();

            actualMonth = DateTime.Today.Month;
            actualYear = DateTime.Today.Year;

            _source = (new StoreData()).GetGroupsByDay(9, actualYear);

            ContactsCVS.Source = _source;

            foreach (var group in _source) {
                actualMoney += group.decimalCost;
            }
        }

        private void OperacjeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {

        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {

        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {

        }

        private void DetailsButton_Click(object sender, RoutedEventArgs e) {

        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {

        }

        private void ListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {

        }

        private void AppBarToggleButton_Checked(object sender, RoutedEventArgs e) {

        }

        private void OperationsCommandBarButton_Click(object sender, RoutedEventArgs e) {

        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e) {

        }

        private void RadioButton_Click(object sender, RoutedEventArgs e) {
            MoreAppBarRadioButton.IsChecked = false;
            if (CommandBar.IsOpen) {
                OperationsAppBarRadioButton.Height = 40;
                CategoriesAppBarRadioButton.Height = 40;
                AddNewOperationAppBarRadioButton.Height = 40;
                StatisticsAppBarRadioButton.Height = 40;
                CommandBar.IsOpen = false;
            }
            else {
                OperationsAppBarRadioButton.Height = Double.NaN;
                CategoriesAppBarRadioButton.Height = Double.NaN;
                AddNewOperationAppBarRadioButton.Height = Double.NaN;
                StatisticsAppBarRadioButton.Height = Double.NaN;
                CommandBar.IsOpen = true;
            }
        }

        private void AddNewOperationAppBarRadioButton_Click(object sender, RoutedEventArgs e) {
            AddNewOperationAppBarRadioButton.IsChecked = false;
        }
    }
}
