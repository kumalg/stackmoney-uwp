using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Collections.ObjectModel;
using System.Globalization;
using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;

namespace Finanse.Pages {
    public sealed partial class Strona_glowna : Page {

        private FontFamily iconStyle = new FontFamily(Settings.GetActualIconStyle());
        private List<int> visiblePayFormList = new List<int>();

        private OperationData storeData = new OperationData(DateTime.Today.Month, DateTime.Today.Year, false, null);
        private ObservableCollection<GroupInfoList<Operation>> operationGroups = new ObservableCollection<GroupInfoList<Operation>>();

        private decimal actualMoney;
        private int actualMonth;
        private int actualYear;

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

            ByDateRadioButton.IsChecked = true;

            //ActualMonthText.Text = getActualMonthText(actualYear, actualMonth);
            setActualMonthText();

            SetActualMoneyBar();
        }

        private void setActualMonthText() {
            if ((actualMonth <= DateTime.Today.Month && actualYear == DateTime.Today.Year) || actualYear < DateTime.Today.Year) {

                storeData = new OperationData(actualMonth, actualYear, false, visiblePayFormList);

                ActualMonthText.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).First().ToString().ToUpper() + DateTimeFormatInfo.CurrentInfo.GetMonthName(actualMonth).Substring(1);
                if (actualYear != DateTime.Today.Year)
                    ActualMonthText.Text += " " + actualYear.ToString();
            } else {

                storeData = new OperationData(actualMonth, actualYear, true, visiblePayFormList);
                ActualMonthText.Text = "Zaplanowane";
            }
        }

        private void setOperationGroups() {
            operationGroups.Clear();
            foreach (var s in (bool)ByDateRadioButton.IsChecked ? storeData.GroupsByDay : storeData.GroupsByCategory) {
                operationGroups.Add(s);
            }
        }

        public void Clicky(object sender, RoutedEventArgs e) {
            if (((ToggleMenuFlyoutItem)sender).IsChecked == true)
                visiblePayFormList.Add((int)((ToggleMenuFlyoutItem)sender).Tag);
            else
                visiblePayFormList.Remove((int)((ToggleMenuFlyoutItem)sender).Tag);

            storeData.SetVisiblePayFormList(visiblePayFormList);

            setOperationGroups();

            SetActualMoneyBar();

            SetListOfOperations(visiblePayFormList);

            if ((semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate == null)
                //    if ((bool)ByDateRadioButton.IsChecked)
                (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;
        }

        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private async void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            var ContentDialogItem = new OperationDetailsContentDialog(operationGroups, (Operation)datacontext, "");
            var result = await ContentDialogItem.ShowAsync();
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            var ContentDialogItem = new NewOperationContentDialog(operationGroups, (Operation)datacontext);
            var result = await ContentDialogItem.ShowAsync();

            SetActualMoneyBar();
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            var ContentDialogItem = new Delete_ContentDialog(operationGroups, (Operation)datacontext, "");
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

                var group = operationGroups.SingleOrDefault(d => ((GroupHeaderByDay)d.Key).dayNum == hi.Day);

                if (group != null)
                    e.DestinationItem = new SemanticZoomLocation() { Item = group };
            }

            //e.DestinationItem = new SemanticZoomLocation { Item = e.SourceItem.Item };
        }

        private void PreviousMonthButton_Click(object sender, RoutedEventArgs e) {

            if (actualMonth > 1) {
                actualMonth--;
            } else {
                actualMonth = 12;
                actualYear--;
            }

            SetListOfOperations(visiblePayFormList);
        }

        private void NextMonthButton_Click(object sender, RoutedEventArgs e) {

            if (actualMonth < 12) {
                actualMonth++;
            } else {
                actualMonth = 1;
                actualYear++;
            }

            SetListOfOperations(visiblePayFormList);
        }

        private void IncomingOperationsButton_Click(object sender, RoutedEventArgs e) {
            NextMonthButton_Click(sender, e);

            IncomingOperationsButton.Visibility = Visibility.Collapsed;
        }

        private void SetListOfOperations(List<int> visiblePayFormList) {

            setActualMonthText();

            setOperationGroups();

            if ((semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate == null)
                // if ((bool)ByDateRadioButton.IsChecked)
                (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;

            SetActualMoneyBar();

            SetNextMonthButtonEnabling();
            SetPreviousMonthButtonEnabling();
        }

        private void SetNextMonthButtonEnabling() {

            if (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) <= new DateTime(actualYear, actualMonth, 1)) {
                NextMonthButton.Visibility = Visibility.Collapsed;
                IncomingOperationsButton.Visibility = Visibility.Visible;
            } else {
                NextMonthButton.Visibility = Visibility.Visible;
                IncomingOperationsButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SetPreviousMonthButtonEnabling() {

            PrevMonthButton.IsEnabled = !(Convert.ToDateTime(Dal.GetEldestOperation().Date) > new DateTime(actualYear, actualMonth, 1));
        }

        private void SetActualMoneyBar() {

            actualMoney = 0;

            foreach (var group in operationGroups)
                actualMoney += group.decimalCost;

            ActualMoneyBar.Text = actualMoney.ToString("C", Settings.GetActualCurrency());
        }

        private void ListViewByDate() {

            operationGroups.Clear();
            foreach (GroupInfoList<Operation> dayGroup in storeData.GroupsByDay)
                operationGroups.Add(dayGroup);

            //    OperacjeListView.ItemsSource = ContactsCVS.View;
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

            operationGroups.Clear();
            foreach (GroupInfoList<Operation> singleGroup in storeData.GroupsByCategory)
                operationGroups.Add(singleGroup);

            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByCategoryGroupStyle);
            //   OperacjeListView.ItemsSource = ContactsCVS.View;

            (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = ContactsCVS.View.CollectionGroups;
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

            var ContentDialogItem = new OperationDetailsContentDialog(operationGroups, thisOperation, "");

            var result = await ContentDialogItem.ShowAsync();
        }

        private void ByCategoryRadioButton_Checked(object sender, RoutedEventArgs e) {
            setOperationGroups();
            ListViewByCategory();
        }

        private void ByDateRadioButton_Checked(object sender, RoutedEventArgs e) {
            setOperationGroups();
            ListViewByDate();
        }
    }
}
