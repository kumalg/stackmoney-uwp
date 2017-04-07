using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using System.Collections.ObjectModel;
using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.System.Profile;
using System.ComponentModel;
using Finanse.Models.DateTimeExtensions;
using Finanse.Models.Helpers;
using Finanse.Models.MAccounts;
using Finanse.Models.MoneyAccounts;
using Finanse.Models.Operations;

namespace Finanse.Pages {
    public sealed partial class OperationsPage : INotifyPropertyChanged {

        private readonly HashSet<string> _visibleAccountsSet = new HashSet<string>();
        private readonly OperationData _storeData = new OperationData();
        private ObservableCollection<GroupInfoList<Operation>> _operationGroups;
        private ObservableCollection<GroupInfoList<Operation>> OperationGroups {
            get {
                _operationGroups = (bool)ByDateRadioButton.IsChecked ? _storeData.OperationsByDay : _storeData.OperationsByCategory;
                return _operationGroups;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (!( e.Parameter is DateTime ))
                return;

            DateTime dateTimeWithDays = (DateTime)e.Parameter;
            _storeData.ActualMonth = dateTimeWithDays.First();
            _storeData.ForceUpdate();
            RaisePropertyChanged("OperationGroups");

            SetNextMonthButtonEnabling();
            SetPreviousMonthButtonEnabling();

            if ((bool)ByDateRadioButton.IsChecked)
                ListViewByDate();
            else
                ListViewByCategory();

            SetEmptyListViewInfoVisibility();
                
            base.OnNavigatedTo(e);
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
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public OperationsPage() {

            InitializeComponent();

            if (AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
                HardwareButtons.BackPressed += HardwareButtons_BackPressed;
           
            foreach (var item in MAccountsDal.GetAllAccountsAndSubAccounts()) {
                CheckBox itema = new CheckBox {
                    Content = item.Name,
                    Tag = item.GlobalId,
                    IsChecked = true,
                };
                _visibleAccountsSet.Add(item.GlobalId);
                ((ListView)VisibleAccountsFlyout.Content).Items?.Add(itema);
            }

            SetNextMonthButtonEnabling();
            SetPreviousMonthButtonEnabling();

            ByDateRadioButton.IsChecked = true;
            ListViewByDate();

            ByDateRadioButton.Checked += ByDateRadioButton_Checked;
            ByCategoryRadioButton.Checked += ByCategoryRadioButton_Checked;
            
            SetEmptyListViewInfoVisibility();

            VisibleAccountsFlyout.Closed += delegate {
                ReloadOperationsWithNewVisibleAccounts();
            };
        }

        private void ReloadOperationsWithNewVisibleAccounts() {
            HashSet<string> previousAccountsSet = new HashSet<string>(_visibleAccountsSet);
            _visibleAccountsSet.Clear();

            foreach (CheckBox item in ((ListView)VisibleAccountsFlyout.Content).Items)
                if (item.IsChecked == true)
                    _visibleAccountsSet.Add(item.Tag.ToString());

            if (previousAccountsSet.SetEquals(_visibleAccountsSet))
                return;

            _storeData.VisiblePayFormList = _visibleAccountsSet;
            SetListOfOperations();
        }

        private List<MoneyMAccountBalance> _listOfMoneyAccounts;

        private List<MoneyMAccountBalance> ListOfMoneyAccounts {
            get {
                _listOfMoneyAccounts = MAccountsDal.ListOfMoneyAccountBalances(_storeData.ActualMonth)
                           .Where(i => _storeData.VisiblePayFormList.Any(ac => ac == i.Account.GlobalId))
                           .ToList();
                RaisePropertyChanged("InitialSum");
                RaisePropertyChanged("FinalSum");
                return _listOfMoneyAccounts;
            }
        }

        private decimal InitialSum => _listOfMoneyAccounts.Sum(i => i.InitialValue);
        private decimal FinalSum => _listOfMoneyAccounts.Sum(i => i.FinalValue);


        private void Grid_RightTapped(object sender, RightTappedRoutedEventArgs e) => Flyouts.ShowFlyoutBase(sender);

        /**
         * BUTTON CLICKS
         */

        private void DetailsButton_Click(object sender, ItemClickEventArgs e) => ShowDetailsContentDialog((Operation)e.ClickedItem);

        private void DetailsButton_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            ShowDetailsContentDialog((Operation)datacontext);
        }
        private async void ShowDetailsContentDialog(Operation operation) {
            OperationDetailsContentDialog operationDetailsContentDialog = new OperationDetailsContentDialog(operation, "") {
                MaxHeight = Window.Current.Bounds.Height - 80
            };
            
            ContentDialogResult result = await operationDetailsContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                ShowEditContentDialog(operation);
            else if (result == ContentDialogResult.Secondary)
                ShowDeleteOperationContentDialog(operation);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            Operation operation = (e.OriginalSource as FrameworkElement)?.DataContext as Operation;
            ShowEditContentDialog(operation);
        }

        private async void ShowEditContentDialog(Operation operation) {
            EditOperationContentDialog editOperationContentDialog = new EditOperationContentDialog(operation);

            ContentDialogResult result = await editOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                UpdateOperationInList(operation, editOperationContentDialog.EditedOperation());
            }
        }
        
        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            ShowDeleteOperationContentDialog((Operation)datacontext);
        }
        private async void ShowDeleteOperationContentDialog(Operation operation) {
            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć operację?");

            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                DeleteOperation_DialogButtonClick(operation);
            }
        }

        private void DeleteOperation_DialogButtonClick(Operation operation) {
            _storeData.RemoveOperation(operation);
            Dal.DeleteOperation(operation);
        }
        
        private void UpdateOperationInList(Operation previous, Operation actual) {
            _storeData.RemoveOperation(previous);
            _storeData.AddOperation(actual);
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
            flyoutBase.ShowAt(senderElement);
        }

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e) {
            if (e.SourceItem == null)
                return;

            if (e.SourceItem.Item.GetType() == typeof(HeaderItem)) {
                HeaderItem hi = (HeaderItem)e.SourceItem.Item;

                var group = OperationGroups.SingleOrDefault(d => ((GroupHeaderByDay)d.Key).DayNum == hi.Day);

                if (group != null)
                    e.DestinationItem = new SemanticZoomLocation() { Item = group };
            }

            //e.DestinationItem = new SemanticZoomLocation { Item = e.SourceItem.Item };
        }

        private async void PreviousMonthButton_Click(object sender, RoutedEventArgs e) {
            ActivateProgressRing();
            await Task.Delay(5);

            _storeData.ActualMonth = _storeData.ActualMonth.AddMonths(-1);
            SetListOfOperations();

            DeactivateProgressRing();
        }

        private async void NextMonthButton_Click(object sender, RoutedEventArgs e) {
            ActivateProgressRing();
            await Task.Delay(5);

            _storeData.ActualMonth = _storeData.ActualMonth.AddMonths(1);
            SetListOfOperations();

            DeactivateProgressRing();
        }

        private async void IncomingOperationsButton_Click(object sender, RoutedEventArgs e) {
            ActivateProgressRing();
            await Task.Delay(5);

            _storeData.ActualMonth = _storeData.ActualMonth.AddMonths(1);
            SetListOfOperations();
            PrevMonthButton.IsEnabled = true; // ponieważ trzeba wrócić z planowanych do aktualnego miesiąca
            IncomingOperationsButton.Visibility = Visibility.Collapsed;

            DeactivateProgressRing();
        }

        private void ActivateProgressRing() {
            ProgressRingBackground.Visibility = Visibility.Visible;
            ProgressRing.IsActive = true;
        }

        private void DeactivateProgressRing() {
            ProgressRingBackground.Visibility = Visibility.Collapsed;
            ProgressRing.IsActive = false;
        }

        private void SetListOfOperations() {
            
            RaisePropertyChanged("OperationGroups");
            RaisePropertyChanged("ZoomedOut_ItemsSource");
            RaisePropertyChanged("ListOfMoneyAccounts");

            SetNextMonthButtonEnabling();
            SetPreviousMonthButtonEnabling();

            SetEmptyListViewInfoVisibility();

        //    BalanceListView.ItemsSource = ListOfMoneyAccounts();
        }

        private bool SetEmptyListViewInfoVisibility() {
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

        private void SetNextMonthButtonEnabling() {
            if (new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1) <= _storeData.ActualMonth) {
                NextMonthButton.Visibility = Visibility.Collapsed;
                IncomingOperationsButton.Visibility = Visibility.Visible;
            }
            else {
                NextMonthButton.Visibility = Visibility.Visible;
                IncomingOperationsButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SetPreviousMonthButtonEnabling() {
            Operation eldestOperation = Dal.GetEldestOperation();
            PrevMonthButton.IsEnabled = eldestOperation != null && Convert.ToDateTime(eldestOperation.Date) < _storeData.ActualMonth;
        }


        private void ListViewByDate() {
            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByDateGroupStyle);
            
            RaisePropertyChanged("ZoomedOut_ItemsSource");
            RaisePropertyChanged("ZoomedOut_ItemsPanel");
            RaisePropertyChanged("ZoomedOut_ItemTemplate");
            RaisePropertyChanged("ZoomedOut_ItemTemplateSelector");
            
            semanticZoom.ViewChangeStarted -= SemanticZoom_ViewChangeStarted;
            semanticZoom.ViewChangeStarted += SemanticZoom_ViewChangeStarted;
        }

        private void ListViewByCategory() {
            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByCategoryGroupStyle);
            
            RaisePropertyChanged("ZoomedOut_ItemsSource");
            RaisePropertyChanged("ZoomedOut_ItemsPanel");
            RaisePropertyChanged("ZoomedOut_ItemTemplate");
            RaisePropertyChanged("ZoomedOut_ItemTemplateSelector");
        }

        private object ZoomedOut_ItemsSource => (bool)ByDateRadioButton.IsChecked ? _storeData.OperationHeaders : ContactsCVS.View.CollectionGroups as object;

        private DataTemplate HeaderTemplate => (bool)ByDateRadioButton.IsChecked ? ByDateGroupStyle.HeaderTemplate : ByCategoryGroupStyle.HeaderTemplate;

        private ItemsPanelTemplate ZoomedOut_ItemsPanel => (bool)ByDateRadioButton.IsChecked ? ByDateGroupItemsPanelTemplate : ByCategoryGroupItemsPanelTemplate;

        private DataTemplate ZoomedOut_ItemTemplate => (bool)ByDateRadioButton.IsChecked ? null : ByCategoryGroupItemTemplate;

        private DataTemplateSelector ZoomedOut_ItemTemplateSelector => (bool)ByDateRadioButton.IsChecked ? GroupEmptyOrFullSelector : null;

        private async void ByCategoryRadioButton_Checked(object sender, RoutedEventArgs e) {
            ActivateProgressRing();
            await Task.Delay(1);

            RaisePropertyChanged("OperationGroups");
            ListViewByCategory();

            DeactivateProgressRing();
        }

        private async void ByDateRadioButton_Checked(object sender, RoutedEventArgs e) {
            ActivateProgressRing();
            await Task.Delay(1);
            
            RaisePropertyChanged("OperationGroups");
            ListViewByDate();

            DeactivateProgressRing();
        }

        private void ByDateButton_Click(object sender, RoutedEventArgs e) {
            ByDateButton.Foreground = (SolidColorBrush)Application.Current.Resources["AccentColor"];
            ByCategoryButton.Foreground = (SolidColorBrush)Application.Current.Resources["Text-1"];
            ByDateRadioButton.IsChecked = true;
        }

        private void ByCategoryButton_Click(object sender, RoutedEventArgs e) {
            ByDateButton.Foreground = (SolidColorBrush)Application.Current.Resources["Text-1"];
            ByCategoryButton.Foreground = (SolidColorBrush)Application.Current.Resources["AccentColor"];
            ByCategoryRadioButton.IsChecked = true;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(NewOperationPage));
        }
    }
}
