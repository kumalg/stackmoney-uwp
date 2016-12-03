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

        //private ObservableCollection<CategoryGroupInfoList<Operation>> _sourceByCategory;
        private FontFamily iconStyle = new FontFamily(Settings.GetActualIconStyle());
        private List<int> visiblePayFormList = new List<int>();

        private OperationData storeData = new OperationData(DateTime.Today.Month, DateTime.Today.Year, false, null);
        private ObservableCollection<GroupInfoList<Operation>> groupsByDay;
        private ObservableCollection<GroupInfoList<Operation>> groupsByCategory;

        decimal actualMoney;
        int actualMonth;
        int actualYear;

        public Strona_glowna() {

            this.InitializeComponent();

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

            actualMonth = DateTime.Today.Month;
            actualYear = DateTime.Today.Year;
            NextMonthButton.Visibility = Visibility.Collapsed;

           // _sourceByCategory = (new StoreData(actualMonth, actualYear, false)).GetGroupsByCategory(actualMonth, actualYear, visiblePayFormList);

            groupsByDay = storeData.GroupsByDay;
            groupsByCategory = storeData.GroupsByCategory;

            ContactsCVS.Source = groupsByDay;
            CategorizedCVS.Source = groupsByCategory;

            (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;

            ActualMonthText.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).First().ToString().ToUpper() + DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).Substring(1);

            //GroupingComboBox.SelectedIndex = 0;
            ByDateRadioButton.IsChecked = true;

            SetActualMoneyBar();

        }
        public void Clicky(object sender, RoutedEventArgs e) {
            if (((ToggleMenuFlyoutItem)sender).IsChecked == true) {
                visiblePayFormList.Add((int)((ToggleMenuFlyoutItem)sender).Tag);
            }
            else
                visiblePayFormList.Remove((int)((ToggleMenuFlyoutItem)sender).Tag);

            storeData.SetVisiblePayFormList(visiblePayFormList);

            groupsByCategory.Clear();
            foreach (var s in storeData.GroupsByCategory) {
                groupsByCategory.Add(s);
            };

            groupsByDay.Clear();
            foreach (var s in storeData.GroupsByDay) {
                groupsByDay.Add(s);
            };

            SetActualMoneyBar();
            
            SetListOfOperations(visiblePayFormList);
            
            if ((semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate == null)
                (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;
                
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
             FrameworkElement senderElement = sender as FrameworkElement;
             FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
             flyoutBase.ShowAt(senderElement);
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new OperationDetailsContentDialog(groupsByDay, (Operation)datacontext, "");

            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new NewOperationContentDialog(groupsByDay, (Operation)datacontext);

            var result = await ContentDialogItem.ShowAsync();

            SetActualMoneyBar();
            //this datacontext is probably some object of some type T
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;

            var ContentDialogItem = new Delete_ContentDialog(groupsByDay, (Operation)datacontext,"");

            var result = await ContentDialogItem.ShowAsync();

            SetActualMoneyBar();
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void OperacjeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            this.OperacjeListView.SelectedItem = null;
        }

        void semanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e) {
            if (e.SourceItem == null)
                return;

            if (e.SourceItem.Item.GetType() == typeof(HeaderItem)) {
                HeaderItem hi = (HeaderItem)e.SourceItem.Item;

                var group = groupsByDay.SingleOrDefault(d => ((GroupHeaderByDay)d.Key).dayNum == hi.Day);

                if (group != null)
                    e.DestinationItem = new SemanticZoomLocation() { Item = group };
            }

            //e.DestinationItem = new SemanticZoomLocation { Item = e.SourceItem.Item };
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e) {

            if (actualMonth > 1) {
                actualMonth--;
            }
            else {
                actualMonth = 12;
                actualYear--;
            }

            SetListOfOperations(visiblePayFormList);
            /*
            if ((semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate == null)
                (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;   
                */
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

            //(semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;
            /*
            if ((semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate == null)
                (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;
                */
        }

        private void SetListOfOperations(List<int> visiblePayFormList) {

            if ((actualMonth <= DateTime.Today.Month && actualYear <= DateTime.Today.Year) || actualYear < DateTime.Today.Year) {

                storeData = new OperationData(actualMonth, actualYear, false, visiblePayFormList);

                ActualMonthText.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).First().ToString().ToUpper() + DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).Substring(1);
                if (actualYear != DateTime.Today.Year)
                    ActualMonthText.Text += " " + actualYear.ToString();
            }
            else {

                storeData = new OperationData(actualMonth, actualYear, true, visiblePayFormList);
                /*
                OperacjeListViewGroup.ItemTemplate = ByDateGroupItemTemplate;
                OperacjeListViewGroup.ItemTemplateSelector = null;
                OperacjeListViewGroup.ItemsSource = ContactsCVS.View.CollectionGroups;
                */

                ActualMonthText.Text = "Zaplanowane";
            }

            //storeData.SetVisiblePayFormList(visiblePayFormList);

            groupsByCategory.Clear();
            foreach (var s in storeData.GroupsByCategory) {
                groupsByCategory.Add(s);
            };

            groupsByDay.Clear();
            foreach (var s in storeData.GroupsByDay) {
                groupsByDay.Add(s);
            };

            SetActualMoneyBar();

            SetNextMonthButtonEnabling();
            SetPreviousMonthButtonEnabling();
        }

        private void SetNextMonthButtonEnabling() {

            if (DateTime.Today.Year <= actualYear && DateTime.Today.Month <= actualMonth) {
                NextMonthButton.Visibility = Visibility.Collapsed;
                IncomingOperationsButton.Visibility = Visibility.Visible;
                IncomingOperationsButton.IsEnabled = true;
            }
            else {
                NextMonthButton.Visibility = Visibility.Visible;
                IncomingOperationsButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SetPreviousMonthButtonEnabling() {

            PrevMonthButton.IsEnabled = !(Convert.ToDateTime(Dal.GetEldestOperation().Date) > new DateTime(actualYear, actualMonth, 1));
        }

        private void SetActualMoneyBar() {

            actualMoney = 0;

            foreach (var group in groupsByDay) {
                actualMoney += group.decimalCost;
            }

            ActualMoneyBar.Text = actualMoney.ToString("C", Settings.GetActualCurrency());
        }

        private void ListViewByDate() {

            OperacjeListView.ItemsSource = ContactsCVS.View;
            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByDateGroupStyle);

            OperacjeListView.SelectionChanged -= OperacjeListView_SelectionChanged;
            OperacjeListView.SelectedItem = null;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate = null;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemTemplateSelector = GroupEmptyOrFullSelector;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemsPanel = ByDateGroupItemsPanelTemplate;

            OperacjeListView.SelectionChanged += OperacjeListView_SelectionChanged;

            semanticZoom.ViewChangeStarted -= semanticZoom_ViewChangeStarted;
            semanticZoom.ViewChangeStarted += semanticZoom_ViewChangeStarted;
        }

        private void ListViewByCategory() {

            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByCategoryGroupStyle);
            OperacjeListView.ItemsSource = CategorizedCVS.View;

            (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = CategorizedCVS.View.CollectionGroups;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemsPanel = ByCategoryGroupItemsPanelTemplate;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemTemplateSelector = null;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate = ByCategoryGroupItemTemplate;
        }

        private void GroupingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (((ComboBoxItem)((ComboBox)sender).SelectedItem).Tag.ToString() == "0")
                ListViewByDate();
            else
                ListViewByCategory();
        }

        private async void OperacjeListView_ItemClick(object sender, ItemClickEventArgs e) {
            Operation thisOperation = (Operation)e.ClickedItem;

            var ContentDialogItem = new OperationDetailsContentDialog(groupsByDay, thisOperation, "");

            var result = await ContentDialogItem.ShowAsync();
        }

        private void IncomingOperationsButton_Click(object sender, RoutedEventArgs e) {

            if (actualMonth < 12) {
                actualMonth++;
            }
            else {
                actualMonth = 1;
                actualYear++;
            }
            
            SetListOfOperations(visiblePayFormList);
            IncomingOperationsButton.Visibility = Visibility.Collapsed;
        }

        private void ByCategoryRadioButton_Checked(object sender, RoutedEventArgs e) {
            ListViewByCategory();
        }

        private void ByDateRadioButton_Checked(object sender, RoutedEventArgs e) {
            ListViewByDate();
        }
    }
}
