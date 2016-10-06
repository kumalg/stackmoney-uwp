using SQLite.Net.Attributes;

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
using SQLite.Net.Platform.WinRT;
using System.Globalization;
using Windows.Graphics.Display;
using Windows.Foundation.Metadata;
using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using System.Threading.Tasks;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Pages {


    public sealed partial class Strona_glowna : Page {

        private ObservableCollection<GroupInfoList<Operation>> _source;
        private ObservableCollection<CategoryGroupInfoList<Operation>> _sourceByCategory;
        private FontFamily iconStyle = new FontFamily(Settings.GetActualIconStyle());
        private List<int> visiblePayFormList = new List<int>();

        bool isSelectionChanged = false;

        decimal actualMoney;
        int actualMonth;
        int actualYear;

        public Strona_glowna() {

            this.InitializeComponent();

            actualMonth = DateTime.Today.Month;
            actualYear = DateTime.Today.Year;
            NextMonthButton.IsEnabled = false;

            ActualMonthText.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).First().ToString().ToUpper() + DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).Substring(1);

            foreach (var item in Dal.GetAllMoneyAccounts()) {
                ToggleMenuFlyoutItem itema = new ToggleMenuFlyoutItem {
                    Text = item.Name,
                    Tag = item.Id,
                    IsChecked = true,
                };
                itema.Click += Clicky;
                visiblePayFormList.Add(item.Id);
                VisiblePayFormMenuFlyout.Items.Add(itema);
            }

            _source = (new StoreData()).GetGroupsByDay(actualMonth, actualYear, visiblePayFormList);
            _sourceByCategory = (new StoreData()).GetGroupsByCategory(actualMonth, actualYear, visiblePayFormList);

            ContactsCVS.Source = _source;
            CategorizedCVS.Source = _sourceByCategory;

            SetActualMoneyBar();

          //  CategorizedCVS.Source = Operation.GetOperationsByCategoryGrouped();

        }
        public void Clicky(object sender, RoutedEventArgs e) {
            if (((ToggleMenuFlyoutItem)sender).IsChecked == true) {
                visiblePayFormList.Add((int)((ToggleMenuFlyoutItem)sender).Tag);
            }
            else
                visiblePayFormList.Remove((int)((ToggleMenuFlyoutItem)sender).Tag);

            SetListOfOperations(visiblePayFormList);
        }

        private async void NowaOperacja_Click(object sender, RoutedEventArgs e) {

            var ContentDialogItem = new NewOperationContentDialog(_source, null, "");

            var result = await ContentDialogItem.ShowAsync();
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
             FrameworkElement senderElement = sender as FrameworkElement;
             FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
             flyoutBase.ShowAt(senderElement);
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new OperationDetailsContentDialog((Operation)datacontext, "");

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new NewOperationContentDialog(_source, (Operation)datacontext, "edit");

            var result = await ContentDialogItem.ShowAsync();

            SetActualMoneyBar();
            //this datacontext is probably some object of some type T
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new Delete_ContentDialog(_source, (Operation)datacontext,"");

            var result = await ContentDialogItem.ShowAsync();

            SetActualMoneyBar();
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void PivotItem_GotFocus(object sender, RoutedEventArgs e) {
        }

        private async void OperacjeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            ListView listView = sender as ListView;
            
            if (listView.SelectedIndex != -1 && isSelectionChanged) {
                Operation thisOperation = (Operation)listView.SelectedItem;

                var ContentDialogItem = new OperationDetailsContentDialog(thisOperation, "");
                listView.SelectedIndex = -1;
                var result = await ContentDialogItem.ShowAsync();
            }
        }

        private void ListView_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
            OperacjeListView.SelectedIndex = -1;
            isSelectionChanged = true;
        }

        private async void PreviousMonthButton_Click(object sender, RoutedEventArgs e) {

            try {
                LoadingIndicator.IsActive = true;
                await LongOperation();
            }
            finally {
                LoadingIndicator.IsActive = false;
            }
        }

        private async Task LongOperation() {
            if (actualMonth > 1) {
                actualMonth--;
            }
            else {
                actualMonth = 12;
                actualYear--;
            }

            SetListOfOperations(visiblePayFormList);
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e) {

            if (actualMonth < 12) {
                actualMonth++;
            }
            else {
                actualMonth = 1;
                actualYear++;
            }

            SetListOfOperations(visiblePayFormList);
        }

        private void SetListOfOperations(List<int> visiblePayFormList) {

            _source.Clear();
            _sourceByCategory.Clear();

            ObservableCollection<GroupInfoList<Operation>> source;
            ObservableCollection<CategoryGroupInfoList<Operation>> sourceByCategory;

            if (visiblePayFormList == null) {
                source = (new StoreData()).GetGroupsByDay(actualMonth, actualYear);
                sourceByCategory = (new StoreData()).GetGroupsByCategory(actualMonth, actualYear);
            }
            else {
                source = (new StoreData()).GetGroupsByDay(actualMonth, actualYear, visiblePayFormList);
                sourceByCategory = (new StoreData()).GetGroupsByCategory(actualMonth, actualYear, visiblePayFormList);
            }

            ActualMonthText.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).First().ToString().ToUpper() + DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).Substring(1);
            if (actualYear != DateTime.Today.Year)
                ActualMonthText.Text += " " + actualYear.ToString();

            foreach (var s in source) {
                _source.Add(s);
            };
            foreach (var s in sourceByCategory) {
                _sourceByCategory.Add(s);
            };

            actualMoney = 0;
            foreach (var item in source)
                actualMoney += item.decimalCost;

            ActualMoneyBar.Text = actualMoney.ToString("C", Settings.GetActualCurrency());

            SetNextMonthButtonEnabling();
            SetPreviousMonthButtonEnabling();
        }

        private void SetNextMonthButtonEnabling() {

            NextMonthButton.IsEnabled = !(DateTime.Today.Year == actualYear && DateTime.Today.Month == actualMonth);
        }

        private void SetPreviousMonthButtonEnabling() {

            string eldestYear = Dal.GetEldestOperation().Date.Substring(0, 4);
            string eldestMonth = Dal.GetEldestOperation().Date.Substring(5, 2);

            PrevMonthButton.IsEnabled = !(eldestMonth == actualMonth.ToString("00") && eldestYear == actualYear.ToString());
        }

        private void SetActualMoneyBar() {
            actualMoney = 0;
            foreach (var group in _source) {
                actualMoney += group.decimalCost;
            }
            ActualMoneyBar.Text = actualMoney.ToString("C", Settings.GetActualCurrency());
        }
    }
}
