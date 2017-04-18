using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using System.Collections.ObjectModel;
using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using Windows.Phone.UI.Input;
using Windows.System.Profile;
using System.ComponentModel;
using Finanse.Models.DateTimeExtensions;
using Finanse.Models.Helpers;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;

namespace Finanse.Pages {
    public sealed partial class OperationsPage : INotifyPropertyChanged {

        private List<string> _visibleAccountsList = new List<string>();
        private readonly OperationData _storeData = new OperationData();

        private ObservableCollection<GroupInfoList<Operation>> _operationGroups;
        private ObservableCollection<GroupInfoList<Operation>> OperationGroups {
            get {
                _operationGroups = (bool)ByDateRadioButton.IsChecked ? _storeData.OperationsByDay : _storeData.OperationsByCategory;
                return _operationGroups;
            }
        }

        private DateTime MinDate => DateTime.Parse(Dal.GetEldestOperation().Date);
        private DateTime MaxDate => DateTime.Now.AddMonths(1);

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            if (!( e.Parameter is DateTime ))
                return;

            DateTime dateTimeWithDays = (DateTime)e.Parameter;
            _storeData.SetMonth(dateTimeWithDays);
            _storeData.ForceUpdate();
            RaisePropertyChanged("OperationGroups");
            RaisePropertyChanged("ListOfMoneyAccounts");
            
            SetPrevNextIncomingButtons();

            if ((bool)ByDateRadioButton.IsChecked)
                ListViewByDate();
            else
                ListViewByCategory();

            SetEmptyListViewInfoVisibility();

            base.OnNavigatedTo(e);
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e) {
            if (!SemanticZoom.IsZoomedInViewActive)
                SemanticZoom.IsZoomedInViewActive = true;
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
                var itema = new CheckBox {
                    Content = item.Name,
                    Tag = item.GlobalId,
                    IsChecked = true,
                };
                _visibleAccountsList.Add(item.GlobalId);
                ((ListView)VisibleAccountsFlyout.Content).Items?.Add(itema);
            }

            SetPrevNextIncomingButtons();

            ByDateRadioButton.IsChecked = true;
            ListViewByDate();

            ByDateRadioButton.Checked += ByDateRadioButton_Checked;
            ByCategoryRadioButton.Checked += ByCategoryRadioButton_Checked;
            
            SetEmptyListViewInfoVisibility();
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

        /*
         * OPERATION BUTTONS CLICK
         */

        private void DetailsButton_Click(object sender, ItemClickEventArgs e) => ShowDetailsContentDialog((Operation)e.ClickedItem);

        private void DetailsButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            ShowDetailsContentDialog((Operation)datacontext);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e) {
            var operation = (e.OriginalSource as FrameworkElement)?.DataContext as Operation;
            ShowEditContentDialog(operation);
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            var datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            ShowDeleteOperationContentDialog((Operation)datacontext);
        }

        private void DeleteOperation_DialogButtonClick(Operation operation) {
            _storeData.RemoveOperation(operation);
            Dal.DeleteOperation(operation);
        }



        private async void ShowDetailsContentDialog(Operation operation) {
            var operationDetailsContentDialog = new OperationDetailsContentDialog(operation, "") {
                MaxHeight = Window.Current.Bounds.Height - 80
            };
            
            var result = await operationDetailsContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
                ShowEditContentDialog(operation);
            else if (result == ContentDialogResult.Secondary)
                ShowDeleteOperationContentDialog(operation);
        }
        
        private async void ShowEditContentDialog(Operation operation) {
            var editOperationContentDialog = new EditOperationContentDialog(operation);

            var result = await editOperationContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            UpdateOperationInList(operation, editOperationContentDialog.EditedOperation());
            RaisePropertyChanged("ListOfMoneyAccounts");
        }
        
        private async void ShowDeleteOperationContentDialog(Operation operation) {
            var acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć operację?");

            var result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            DeleteOperation_DialogButtonClick(operation);
            RaisePropertyChanged("ListOfMoneyAccounts");
        }


        /*
         * PREV NEXT INCOMING BUTTONS CLICK
         */

        private async void PreviousMonthButton_Click(object sender, RoutedEventArgs e) {
            ActivateProgressRing();
            await Task.Delay(5);

            _storeData.PrevMonth();
            SetListOfOperations();

            DeactivateProgressRing();
        }

        private async void NextMonthButton_Click(object sender, RoutedEventArgs e) {
            ActivateProgressRing();
            await Task.Delay(5);

            _storeData.NextMonth();
            SetListOfOperations();

            DeactivateProgressRing();
        }

        private async void IncomingOperationsButton_Click(object sender, RoutedEventArgs e) {
            ActivateProgressRing();
            await Task.Delay(5);

            _storeData.NextMonth();
            SetListOfOperations();

            DeactivateProgressRing();
        }

        

        private void UpdateOperationInList(Operation previous, Operation actual) {
            _storeData.RemoveOperation(previous);
            _storeData.AddOperation(actual);
        }

        private void Grid_DragStarting(UIElement sender, DragStartingEventArgs args) => Flyouts.ShowFlyoutBase(sender);

        private void SemanticZoom_ViewChangeStarted(object sender, SemanticZoomViewChangedEventArgs e) {
            if (e.SourceItem == null)
                return;

            if (e.SourceItem.Item.GetType() != typeof(HeaderItem))
                return;

            var hi = (HeaderItem)e.SourceItem.Item;

            var group = OperationGroups.SingleOrDefault(d => ((GroupHeaderByDay)d.Key).DayNum == hi.Day);

            if (group != null)
                e.DestinationItem = new SemanticZoomLocation { Item = group };

            //e.DestinationItem = new SemanticZoomLocation { Item = e.SourceItem.Item };
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
            
            SetEmptyListViewInfoVisibility();
            SetPrevNextIncomingButtons();
        }

        private void SetPrevNextIncomingButtons() {
            var minMonth = MinDate.Date.First();
            var thisMonth = DateTime.Now.Date.First();
            
            if (_storeData.ActualMonth <= minMonth) {
                PrevMonthButton.IsEnabled = false;
                PrevMonthButton.Visibility = Visibility.Visible;
                NextMonthButton.Visibility = Visibility.Visible;
                IncomingOperationsButton.Visibility = Visibility.Collapsed;
            }
            else if (_storeData.ActualMonth == thisMonth) {
                PrevMonthButton.IsEnabled = true;
                PrevMonthButton.Visibility = Visibility.Visible;
                NextMonthButton.Visibility = Visibility.Collapsed;
                IncomingOperationsButton.Visibility = Visibility.Visible;
            }
            else if (_storeData.ActualMonth > thisMonth) {
                PrevMonthButton.IsEnabled = true;
                PrevMonthButton.Visibility = Visibility.Visible;
                NextMonthButton.Visibility = Visibility.Collapsed;
                IncomingOperationsButton.Visibility = Visibility.Collapsed;
            }
            else {
                PrevMonthButton.IsEnabled = true;
                PrevMonthButton.Visibility = Visibility.Visible;
                NextMonthButton.Visibility = Visibility.Visible;
                IncomingOperationsButton.Visibility = Visibility.Collapsed;
            }
        }

        private void SetEmptyListViewInfoVisibility() {
            if (OperationGroups.Count == 0) {
                SemanticZoom.Visibility = Visibility.Collapsed;
                EmptyListViewInfo.Visibility = Visibility.Visible;
            }
            else {
                SemanticZoom.Visibility = Visibility.Visible;
                EmptyListViewInfo.Visibility = Visibility.Collapsed;
            }
        }

        private void ListViewByDate() {
            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByDateGroupStyle);

            RaisePropertyChanged("GroupStyle");
            RaisePropertyChanged("ZoomedOut_ItemsSource");
            RaisePropertyChanged("ZoomedOut_ItemsPanel");
            RaisePropertyChanged("ZoomedOut_ItemTemplate");
            RaisePropertyChanged("ZoomedOut_ItemTemplateSelector");
            
            SemanticZoom.ViewChangeStarted -= SemanticZoom_ViewChangeStarted;
            SemanticZoom.ViewChangeStarted += SemanticZoom_ViewChangeStarted;
        }

        private void ListViewByCategory() {
            OperacjeListView.GroupStyle.Clear();
            OperacjeListView.GroupStyle.Add(ByCategoryGroupStyle);

            RaisePropertyChanged("GroupStyle");
            RaisePropertyChanged("ZoomedOut_ItemsSource");
            RaisePropertyChanged("ZoomedOut_ItemsPanel");
            RaisePropertyChanged("ZoomedOut_ItemTemplate");
            RaisePropertyChanged("ZoomedOut_ItemTemplateSelector");
        }

        private object ZoomedOut_ItemsSource => (bool)ByDateRadioButton.IsChecked ? _storeData.OperationHeaders : ContactsCVS.View.CollectionGroups as object;

        private GroupStyle GroupStyle => (bool)ByDateRadioButton.IsChecked ? ByDateGroupStyle : ByCategoryGroupStyle;

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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e) {
            Frame.Navigate(typeof(NewOperationPage));
        }

        private void ActualMonthDatePickerFlyout_Closed(object sender, object e) {
            if (_storeData.SetMonth(ActualMonthDatePickerFlyout.Date.Date))
                SetListOfOperations();
        }

        private void ActualMonthButton_Click(object sender, object e) {
            ActualMonthDatePickerFlyout.Date = _storeData.ActualMonth;
        }

        private void VisibleAccountsFlyout_OnClosed(object sender, object e) {
            var previousAccountsSet = _visibleAccountsList.ToList();

            _visibleAccountsList = ((ListView) VisibleAccountsFlyout.Content)
                .Items
                .Select(i => (CheckBox)i)
                .Where(i => i.IsChecked == true)
                .Select(i => i.Tag.ToString())
                .ToList();

            if (previousAccountsSet.SequenceEqual(_visibleAccountsList))
                return;

            _storeData.VisiblePayFormList = _visibleAccountsList;
            SetListOfOperations();
        }
    }
}
