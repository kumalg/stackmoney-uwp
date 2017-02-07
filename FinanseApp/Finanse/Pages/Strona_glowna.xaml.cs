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
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.System.Profile;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI;
using System.ComponentModel;

namespace Finanse.Pages {
    public sealed partial class Strona_glowna : Page, INotifyPropertyChanged {

        private HashSet<int> visibleAccountsSet = new HashSet<int>();
        private OperationData storeData;
        private ObservableCollection<GroupInfoList<Operation>> _operationGroups = new ObservableCollection<GroupInfoList<Operation>>();
        private ObservableCollection<GroupInfoList<Operation>> operationGroups {
            get {
                return _operationGroups;
            }
            set {
                _operationGroups = value;
                RaisePropertyChanged("operationGroups");
            }
        }
        private DateTime actualYearAndMonth;
        private TextBlock actualMonthText = new TextBlock();

        protected override void OnNavigatedTo(NavigationEventArgs e) {

            if (e.Parameter is DateTime) {
                DateTime dateTimeWithDays = (DateTime)e.Parameter;

                actualYearAndMonth = Functions.dateTimeWithFirstDayOfMonth(dateTimeWithDays);

                setNextMonthButtonEnabling();
                setPreviousMonthButtonEnabling();

                storeData = setStoreData();

                groupOperations(operationGroups, storeData);

                if ((bool)ByDateRadioButton.IsChecked)
                    listViewByDate();
                else
                    listViewByCategory();

                setActualMonthText();
                setActualMoneyBar();
                setEmptyListViewInfoVisibility();
                
                base.OnNavigatedTo(e);
            }            
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e) {
            if (!semanticZoom.IsZoomedInViewActive)
                semanticZoom.IsZoomedInViewActive = true;
               // e.Handled = true;

        }
        private void BackRequestedEvent(object sender, BackRequestedEventArgs e) {
            if (!semanticZoom.IsZoomedInViewActive)
                semanticZoom.IsZoomedInViewActive = true;
            // e.Handled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Strona_glowna() {

            this.InitializeComponent();

            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
           
            foreach (var item in Dal.getAllMoneyAccounts()) {
                CheckBox itema = new CheckBox {
                    Content = item.Name,
                    Tag = item.Id,
                    IsChecked = true,
                };
                visibleAccountsSet.Add(item.Id);
                ((ListView)VisibleAccountsFlyout.Content).Items.Add(itema);
            }
           
            actualYearAndMonth = Functions.dateTimeWithFirstDayOfMonth(DateTime.Today);

            setNextMonthButtonEnabling();
            setPreviousMonthButtonEnabling();

            storeData = setStoreData();

            ByDateRadioButton.IsChecked = true;
            groupOperations(operationGroups, storeData);
            listViewByDate();

            ByDateRadioButton.Checked += ByDateRadioButton_Checked;
            ByCategoryRadioButton.Checked += ByCategoryRadioButton_Checked;

            setActualMonthText();
            setActualMoneyBar();
            setEmptyListViewInfoVisibility();

            VisibleAccountsFlyout.Closed += delegate {
                reloadOperationsWithNewVisibleAccounts();
            };
        }

        private void reloadOperationsWithNewVisibleAccounts() {
            HashSet<int> previousAccountsSet = new HashSet<int>(visibleAccountsSet);
            visibleAccountsSet.Clear();

            foreach (CheckBox item in ((ListView)VisibleAccountsFlyout.Content).Items)
                if (item.IsChecked == true)
                    visibleAccountsSet.Add((int)item.Tag);
            
            if (!previousAccountsSet.SetEquals(visibleAccountsSet)) {
                storeData.SetVisiblePayFormList(visibleAccountsSet);
                setListOfOperations(visibleAccountsSet);
                
                //  if ((semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate == null)
                if ((bool)ByDateRadioButton.IsChecked)
                    (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;
            }
        }

        private List<MoneyAccountBalance> listOfMoneyAccounts() {
            //    return Dal.listOfMoneyAccountBalances(actualYearAndMonth).Where(i => visiblePayFormList.Any(ac => ac == i.MoneyAccount.Id)).ToList();
            return Dal.listOfMoneyAccountBalances(actualYearAndMonth);
        }

        private bool isFutureMonth(DateTime date) {
            return date > new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1); //(actualYearAndMonth.Month <= DateTime.Today.Month && actualYearAndMonth.Year == DateTime.Today.Year) || actualYearAndMonth.Year < DateTime.Today.Year;
        }

        private OperationData setStoreData() {
            return new OperationData(actualYearAndMonth.Month, actualYearAndMonth.Year, isFutureMonth(actualYearAndMonth), visibleAccountsSet);
        }

        private void setActualMonthText() {

            if (!isFutureMonth(actualYearAndMonth)) {
                actualMonthText.Text = DateTimeFormatInfo.CurrentInfo.GetMonthName(actualYearAndMonth.Month).First().ToString().ToUpper() + DateTimeFormatInfo.CurrentInfo.GetMonthName(actualYearAndMonth.Month).Substring(1);
                if (actualYearAndMonth.Year != DateTime.Today.Year)
                    actualMonthText.Text += " " + actualYearAndMonth.Year.ToString();
            } else {
                var loader = new Windows.ApplicationModel.Resources.ResourceLoader();
                actualMonthText.Text = loader.GetString("plannedString");
            }
        }

        private void groupOperations(ObservableCollection<GroupInfoList<Operation>> operationGroupsTYMCZASOWO, OperationData operations) {
            operationGroups = (bool)ByDateRadioButton.IsChecked ? operations.GroupsByDay : operations.GroupsByCategory;
            /*
            operationGroups.Clear();

            foreach (var s in (bool)ByDateRadioButton.IsChecked ? operations.GroupsByDay : operations.GroupsByCategory)
                operationGroups.Add(s);
            */
                
        }
        
        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        /**
         * BUTTON CLICKS
         */

        private void DetailsButton_Click(object sender, ItemClickEventArgs e) {
            showDetailsContentDialog((Operation)e.ClickedItem);
        }
        private void DetailsButton_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            showDetailsContentDialog((Operation)datacontext);
        }
        private async void showDetailsContentDialog(Operation operation) {
            OperationDetailsContentDialog operationDetailsContentDialog = new OperationDetailsContentDialog(operationGroups, operation, "");

            operationDetailsContentDialog.MaxHeight = Window.Current.Bounds.Height - 80;

            ContentDialogResult result = await operationDetailsContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                showEditContentDialog(operation);
            else if (result == ContentDialogResult.Secondary)
                showDeleteOperationContentDialog(operation);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            Operation operation = (e.OriginalSource as FrameworkElement).DataContext as Operation;
            showEditContentDialog(operation);
        }

        private async void showEditContentDialog(Operation operation) {
            EditOperationContentDialog editOperationContentDialog = new EditOperationContentDialog(operation);

            ContentDialogResult result = await editOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                updateOperationInList(operation, editOperationContentDialog.editedOperation());
                setActualMoneyBar();
            }
        }
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            showDeleteOperationContentDialog((Operation)datacontext);
        }
        private async void showDeleteOperationContentDialog(Operation operation) {
            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć operację?");

            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                deleteOperation_DialogButtonClick(operation);
        }

        private void deleteOperation_DialogButtonClick(Operation operation) {
            removeOperationFromList(operation);
            Dal.deleteOperation(operation);
            setActualMoneyBar();
        }
        
        private void updateOperationInList(Operation previous, Operation actual) {
            removeOperationFromList(previous);
            tryAddOperationToList(actual);
        }

        private void removeOperationFromList(Operation operation) {
            if ((bool)ByDateRadioButton.IsChecked)
                removeOperationFromListByDay(operation);
            else
                removeOperationFromListByCategory(operation);
        }

        private void tryAddOperationToList(Operation operation) {
            if ((bool)ByDateRadioButton.IsChecked)
                tryAddOperationToListByDay(operation);
            else
                tryAddOperationToListByCategory(operation);
        }

        private void removeOperationFromListByDay(Operation operation) {
            int index = operationGroups.Count - 1;  /// głupio bo leci po ostatnim indeksie a nie widzi że jest bez daty
            if (!operation.Date.Equals(""))
                index = operationGroups.IndexOf(operationGroups.First(i => ((GroupHeaderByDay)i.Key).date == operation.Date));

            if (operationGroups.ElementAt(index).Count > 1)
                operationGroups.ElementAt(index).Remove(operation);
            else
                operationGroups.RemoveAt(index);
        }

        private void tryAddOperationToListByDay(Operation operation) {
            if (actualYearAndMonth > DateTime.Today.Date) {
                if (operation.Date.Equals("")) {
                    if (operationGroups.Any(i => ((GroupHeaderByDay)i.Key).date == ""))
                        operationGroups.First(i => ((GroupHeaderByDay)i.Key).date == "").Insert(0, operation);
                    else {
                        GroupInfoList<Operation> group = new GroupInfoList<Operation>() {
                            Key = new GroupHeaderByDay(""),
                        };
                        group.Add(operation);
                        operationGroups.Add(group);
                    }
                }
                else if (Convert.ToDateTime(operation.Date).Date > DateTime.Today.Date){

                }
            }
            else if (Convert.ToDateTime(operation.Date).Month == actualYearAndMonth.Month && Convert.ToDateTime(operation.Date).Year == actualYearAndMonth.Year) {
                if (operationGroups.Any(i => ((GroupHeaderByDay)i.Key).date == operation.Date)) {
                    operationGroups.First(i => ((GroupHeaderByDay)i.Key).date == operation.Date).Insert(0, operation);
                }
                else {
                    GroupInfoList<Operation> group = new GroupInfoList<Operation>() {
                        Key = new GroupHeaderByDay(operation.Date),
                    };
                    group.Add(operation);

                    int i;
                    for (i = 0; i < operationGroups.Count; i++)
                        if (((GroupHeaderByDay)operationGroups.ElementAt(i).Key).date.CompareTo(operation.Date) < 0)
                            break;

                    operationGroups.Insert(i, group);
                }
            }
        }

        private void removeOperationFromListByCategory(Operation operation) {
            int index = operationGroups.IndexOf(operationGroups.First(i => ((GroupHeaderByCategory)i.Key).categoryId == operation.CategoryId));
            if (operationGroups.ElementAt(index).Count > 1)
                operationGroups.ElementAt(index).Remove(operation);
            else
                operationGroups.RemoveAt(index);
        }

        private void tryAddOperationToListByCategory(Operation operation) {
            if (Convert.ToDateTime(operation.Date).Date > DateTime.Today.Date) {
                /// planowany wydatek
            }
            else if (Convert.ToDateTime(operation.Date).Month == actualYearAndMonth.Month && Convert.ToDateTime(operation.Date).Year == actualYearAndMonth.Year) {
                if (operationGroups.Any(i => ((GroupHeaderByCategory)i.Key).categoryId == operation.Id)) {
                    operationGroups.First(i => ((GroupHeaderByCategory)i.Key).categoryId == operation.Id).Add(operation);
                }
                else {
                    GroupInfoList<Operation> group = new GroupInfoList<Operation>();
                    /// tu trzeba dodać ikonkę i nazwę kategorii
                    /// jest pomysł żeby przesyłać wyłącznie Id
                    group.Add(operation);
                    operationGroups.Add(group);
                }
            }
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void OperacjeListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            OperacjeListView.SelectedItem = null;
        }

        private void semanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e) {
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

        private async void PreviousMonthButton_Click(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(5);

            actualYearAndMonth = actualYearAndMonth.AddMonths(-1);
            setListOfOperations(visibleAccountsSet);

            deactivateProgressRing();
        }

        private async void NextMonthButton_Click(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(5);

            actualYearAndMonth = actualYearAndMonth.AddMonths(1);
            setListOfOperations(visibleAccountsSet);

            deactivateProgressRing();
        }

        private async void IncomingOperationsButton_Click(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(5);

            actualYearAndMonth = actualYearAndMonth.AddMonths(1);
            setListOfOperations(visibleAccountsSet);
            PrevMonthButton.IsEnabled = true; // ponieważ trzeba wrócić z planowanych do aktualnego miesiąca
            IncomingOperationsButton.Visibility = Visibility.Collapsed;

            deactivateProgressRing();
        }

        private void activateProgressRing() {
            ProgressRingBackground.Visibility = Visibility.Visible;
            ProgressRing.IsActive = true;
        }

        private void deactivateProgressRing() {
            ProgressRingBackground.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;
        }

        private void setListOfOperations(HashSet<int> visiblePayFormList) {

            storeData = setStoreData();
            setActualMonthText();
            groupOperations(operationGroups, storeData);

            // if ((semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate == null)
            if ((bool)ByDateRadioButton.IsChecked)
                (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = storeData.OperationHeaders;

            setActualMoneyBar();

            setNextMonthButtonEnabling();
            setPreviousMonthButtonEnabling();

            setEmptyListViewInfoVisibility();

            BalanceListView.ItemsSource = listOfMoneyAccounts();
        }

        private bool setEmptyListViewInfoVisibility() {
            if (operationGroups.Count == 0) {
                semanticZoom.Visibility = Visibility.Collapsed;
                EmptyListViewInfo.Visibility = Visibility.Visible;
                return false;
            }
            else {
                semanticZoom.Visibility = Visibility.Visible;
                EmptyListViewInfo.Visibility = Visibility.Collapsed;
                return true;
            }
        }

        private void setNextMonthButtonEnabling() {
            if (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) <= actualYearAndMonth) {
                NextMonthButton.Visibility = Visibility.Collapsed;
                IncomingOperationsButton.Visibility = Visibility.Visible;
            } else {
                NextMonthButton.Visibility = Visibility.Visible;
                IncomingOperationsButton.Visibility = Visibility.Collapsed;
            }
        }

        private void setPreviousMonthButtonEnabling() {
            Operation eldestOperation = Dal.getEldestOperation();

            PrevMonthButton.IsEnabled = eldestOperation == null ? 
                false : 
                Convert.ToDateTime(eldestOperation.Date) < actualYearAndMonth;
        }

        private void setActualMoneyBar() {
            decimal actualMoney = 0;

            foreach (var group in operationGroups)
                actualMoney += group.decimalCost;

            ActualMoneyBar.Text = actualMoney.ToString("C", Settings.getActualCultureInfo());
        }

        private void listViewByDate() {
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

        private void listViewByCategory() {
            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByCategoryGroupStyle);

            (semanticZoom.ZoomedOutView as ListViewBase).ItemsSource = ContactsCVS.View.CollectionGroups;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemsPanel = ByCategoryGroupItemsPanelTemplate;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemTemplateSelector = null;
            (semanticZoom.ZoomedOutView as ListViewBase).ItemTemplate = ByCategoryGroupItemTemplate;
        }

        private async void ByCategoryRadioButton_Checked(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(1);

            groupOperations(operationGroups, storeData);
            listViewByCategory();

            deactivateProgressRing();
        }

        private async void ByDateRadioButton_Checked(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(1);

            groupOperations(operationGroups, storeData);
            listViewByDate();

            deactivateProgressRing();
        }

        private void ByDateButton_Click(object sender, RoutedEventArgs e) {
            ByDateButton.Foreground = ((SolidColorBrush)Application.Current.Resources["AccentColor"] as SolidColorBrush);
            ByCategoryButton.Foreground = ((SolidColorBrush)Application.Current.Resources["Text-1"] as SolidColorBrush);
            ByDateRadioButton.IsChecked = true;
        }

        private void ByCategoryButton_Click(object sender, RoutedEventArgs e) {
            ByDateButton.Foreground = ((SolidColorBrush)Application.Current.Resources["Text-1"] as SolidColorBrush);
            ByCategoryButton.Foreground = ((SolidColorBrush)Application.Current.Resources["AccentColor"] as SolidColorBrush);
            ByCategoryRadioButton.IsChecked = true;
        }
    }
}
