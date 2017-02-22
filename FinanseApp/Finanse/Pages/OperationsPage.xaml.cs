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
using Finanse.Models.Helpers;

namespace Finanse.Pages {
    public sealed partial class OperationsPage : Page, INotifyPropertyChanged {

        private HashSet<int> visibleAccountsSet = new HashSet<int>();
        private OperationData storeData = new OperationData();
        private ObservableCollection<GroupInfoList<Operation>> operationGroups;
        private ObservableCollection<GroupInfoList<Operation>> OperationGroups {
            get {
                operationGroups = (bool)ByDateRadioButton.IsChecked ? storeData.OperationsByDay : storeData.OperationsByCategory;
                return operationGroups;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {

            if (e.Parameter is DateTime) {
                DateTime dateTimeWithDays = (DateTime)e.Parameter;
                storeData.ActualMonth = Date.FirstDayInMonth(dateTimeWithDays);
                storeData.ForceUpdate();
                RaisePropertyChanged("OperationGroups");

                setNextMonthButtonEnabling();
                setPreviousMonthButtonEnabling();

                if ((bool)ByDateRadioButton.IsChecked)
                    listViewByDate();
                else
                    listViewByCategory();

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

        public OperationsPage() {

            this.InitializeComponent();

            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
           
            foreach (var item in AccountsDal.getAllMoneyAccounts()) {
                CheckBox itema = new CheckBox {
                    Content = item.Name,
                    Tag = item.Id,
                    IsChecked = true,
                };
                visibleAccountsSet.Add(item.Id);
                ((ListView)VisibleAccountsFlyout.Content).Items.Add(itema);
            }

            setNextMonthButtonEnabling();
            setPreviousMonthButtonEnabling();

            ByDateRadioButton.IsChecked = true;
            listViewByDate();

            ByDateRadioButton.Checked += ByDateRadioButton_Checked;
            ByCategoryRadioButton.Checked += ByCategoryRadioButton_Checked;
            
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
                storeData.VisiblePayFormList = visibleAccountsSet;
                setListOfOperations();
            }
        }

        private List<MoneyAccountBalance> listOfMoneyAccounts() {
            //    return Dal.listOfMoneyAccountBalances(actualYearAndMonth).Where(i => visiblePayFormList.Any(ac => ac == i.MoneyAccount.Id)).ToList();
            return AccountsDal.listOfMoneyAccountBalances(storeData.ActualMonth);
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
            OperationDetailsContentDialog operationDetailsContentDialog = new OperationDetailsContentDialog(OperationGroups, operation, "");

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
            }
        }
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            showDeleteOperationContentDialog((Operation)datacontext);
        }
        private async void showDeleteOperationContentDialog(Operation operation) {
            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć operację?");

            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                deleteOperation_DialogButtonClick(operation);
            }
        }

        private void deleteOperation_DialogButtonClick(Operation operation) {
            storeData.RemoveOperation(operation);
            Dal.deleteOperation(operation);
        }
        
        private void updateOperationInList(Operation previous, Operation actual) {
            storeData.RemoveOperation(previous);
            storeData.AddOperation(actual);
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void semanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e) {
            if (e.SourceItem == null)
                return;

            if (e.SourceItem.Item.GetType() == typeof(HeaderItem)) {
                HeaderItem hi = (HeaderItem)e.SourceItem.Item;

                var group = OperationGroups.SingleOrDefault(d => ((GroupHeaderByDay)d.Key).dayNum == hi.Day);

                if (group != null)
                    e.DestinationItem = new SemanticZoomLocation() { Item = group };
            }

            //e.DestinationItem = new SemanticZoomLocation { Item = e.SourceItem.Item };
        }

        private async void PreviousMonthButton_Click(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(5);

            storeData.ActualMonth = storeData.ActualMonth.AddMonths(-1);
            setListOfOperations();

            deactivateProgressRing();
        }

        private async void NextMonthButton_Click(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(5);

            storeData.ActualMonth = storeData.ActualMonth.AddMonths(1);
            setListOfOperations();

            deactivateProgressRing();
        }

        private async void IncomingOperationsButton_Click(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(5);

            storeData.ActualMonth = storeData.ActualMonth.AddMonths(1);
            setListOfOperations();
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

        private void setListOfOperations() {
            
            RaisePropertyChanged("OperationGroups");
            RaisePropertyChanged("ZoomedOut_ItemsSource");

            setNextMonthButtonEnabling();
            setPreviousMonthButtonEnabling();

            setEmptyListViewInfoVisibility();

            BalanceListView.ItemsSource = listOfMoneyAccounts();
        }

        private bool setEmptyListViewInfoVisibility() {
            if (OperationGroups.Count == 0) {
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
            if (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) <= storeData.ActualMonth) {
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
                Convert.ToDateTime(eldestOperation.Date) < storeData.ActualMonth;
        }


        private void listViewByDate() {
            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByDateGroupStyle);
            
            RaisePropertyChanged("ZoomedOut_ItemsSource");
            RaisePropertyChanged("ZoomedOut_ItemsPanel");
            RaisePropertyChanged("ZoomedOut_ItemTemplate");
            RaisePropertyChanged("ZoomedOut_ItemTemplateSelector");
            
            semanticZoom.ViewChangeStarted -= semanticZoom_ViewChangeStarted;
            semanticZoom.ViewChangeStarted += semanticZoom_ViewChangeStarted;
        }

        private void listViewByCategory() {
            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByCategoryGroupStyle);
            
            RaisePropertyChanged("ZoomedOut_ItemsSource");
            RaisePropertyChanged("ZoomedOut_ItemsPanel");
            RaisePropertyChanged("ZoomedOut_ItemTemplate");
            RaisePropertyChanged("ZoomedOut_ItemTemplateSelector");
        }

        private object ZoomedOut_ItemsSource {
            get { return (bool)ByDateRadioButton.IsChecked ? storeData.OperationHeaders : ContactsCVS.View.CollectionGroups as object; }
        }

        private DataTemplate HeaderTemplate {
            get { return (bool)ByDateRadioButton.IsChecked ? ByDateGroupStyle.HeaderTemplate : ByCategoryGroupStyle.HeaderTemplate; }
        }

        private ItemsPanelTemplate ZoomedOut_ItemsPanel {
            get { return (bool)ByDateRadioButton.IsChecked ? ByDateGroupItemsPanelTemplate : ByCategoryGroupItemsPanelTemplate; }
        }

        private DataTemplate ZoomedOut_ItemTemplate {
            get { return (bool)ByDateRadioButton.IsChecked ? null : ByCategoryGroupItemTemplate; }
        }

        private DataTemplateSelector ZoomedOut_ItemTemplateSelector {
            get { return (bool)ByDateRadioButton.IsChecked ? GroupEmptyOrFullSelector : null; }
        }

        private async void ByCategoryRadioButton_Checked(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(1);
            
            RaisePropertyChanged("OperationGroups");
            listViewByCategory();

            deactivateProgressRing();
        }

        private async void ByDateRadioButton_Checked(object sender, RoutedEventArgs e) {
            activateProgressRing();
            await Task.Delay(1);
            
            RaisePropertyChanged("OperationGroups");
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
