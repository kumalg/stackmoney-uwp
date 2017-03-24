using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using Finanse.Models.MoneyAccounts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Finanse.Models.Categories;
using Finanse.Models.Helpers;
using Finanse.Models.Operations;
using Microsoft.Graph;

namespace Finanse.Pages {

    public sealed partial class NewOperationPage : Page, INotifyPropertyChanged {

        private readonly Regex regex = NewOperation.GetRegex();
        private string acceptedCostValue = string.Empty;
        private bool isUnfocused = true;

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            SetDefaultPageValues();

            SetCategoryComboBoxItems(true, false);
            AccountsWithoutCards = AccountsDal.GetAccountsWithoutCards();
            Accounts = AccountsDal.GetAllMoneyAccounts();
            RaisePropertyChanged("AccountsComboBox");
            RaisePropertyChanged("AccountsToComboBox");
            RaisePropertyChanged("AccountsFromComboBox");

            base.OnNavigatedTo(e);
        }

        private void SetDefaultPageValues() {
            CostValue.Text = string.Empty;
            acceptedCostValue = string.Empty;
            NameValue.Text = string.Empty;
            DateValue.Date = DateTime.Today;
            CategoryValue.SelectedIndex = -1;
            SubCategoryValue.SelectedIndex = -1;
            SubCategoryValue.IsEnabled = false;
            // PayFormValue.SelectedIndex = 0;
            MoreInfoValue.Text = string.Empty;
            SaveAsAssetToggle.IsOn = false;
            HideInStatisticsToggle.IsOn = false;
            Expense_RadioButton.IsChecked = true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<Account> AccountsWithoutCards = AccountsDal.GetAccountsWithoutCards();
        private List<Account> Accounts = AccountsDal.GetAllMoneyAccounts();


        private ObservableCollection<Account> AccountsComboBox {
            get {
                if ((bool)Income_RadioButton.IsChecked) {
                    return new ObservableCollection<Account>(AccountsWithoutCards);
                }
                return new ObservableCollection<Account>(Accounts);
            }
        }

        private List<Account> AccountsToComboBox => AccountsWithoutCards.ToList();

        private List<Account> AccountsFromComboBox => AccountsWithoutCards.ToList();

        public NewOperationPage() {

            this.InitializeComponent();

            DateValue.Date = DateTime.Today;
            DateValue.MaxDate = Settings.MaxDate;
            DateValue.MinDate = Settings.MinDate;

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            if (PayFormValue.Items.Count > 0)
                PayFormValue.SelectedIndex = 0;
            if (InitialAccount.Items.Count > 0)
                InitialAccount.SelectedIndex = 0;
        }

        public Windows.Globalization.DayOfWeek FirstDayOfWeek() {
            return Settings.FirstDayOfWeek;
        }

        private void TransferRadioButton_Checked(object sender, RoutedEventArgs e) {
            TransferAccounts_Grid.Visibility = Visibility.Visible;

            PayFormValue.Visibility = Visibility.Collapsed;
            SaveAsPattern_StackPanel.Visibility = Visibility.Collapsed;
            UsePatternButton.Visibility = Visibility.Collapsed;

            int selectedCategoryId = -1;
            int selectedSubCategoryId = -1;

            if (CategoryValue.SelectedIndex != -1) {
                selectedCategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;

                if (SubCategoryValue.SelectedIndex != -1)
                    selectedSubCategoryId = (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;
            }

            SetCategoryComboBoxItems(true, true);

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => (int)i.Tag == selectedCategoryId);

            if (CategoryValue.SelectedItem != null)
                SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => (int)i.Tag == selectedSubCategoryId);
        }

        private void ExpenseOrIncomeRadioButton_Checked(object sender, RoutedEventArgs e) {
            RaisePropertyChanged("AccountsComboBox");
            TransferAccounts_Grid.Visibility = Visibility.Collapsed;

            PayFormValue.Visibility = Visibility.Visible;
            SaveAsPattern_StackPanel.Visibility = Visibility.Visible;
            UsePatternButton.Visibility = Visibility.Visible;


            if (CategoryValue.SelectedIndex != -1) {
                int idOfSelectedCategory = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;
                int idOfSelectedSubCategory = -1;

                if (SubCategoryValue.SelectedIndex != -1)
                    idOfSelectedSubCategory = (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;

                SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

                if (CategoryValue.Items.OfType<ComboBoxItem>().Any(i => (int)i.Tag == idOfSelectedCategory))
                    CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().Single(i => (int)i.Tag == idOfSelectedCategory);
                else
                    SubCategoryValue.IsEnabled = false;

                if (idOfSelectedSubCategory == -1)
                    return;

                if (SubCategoryValue.Items.OfType<SubCategory>().All(i => i.Id != idOfSelectedSubCategory))
                    return;

                SubCategory subCatItem = Dal.GetSubCategoryById(idOfSelectedSubCategory);
                SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == subCatItem.Name);
            }

            else {
                SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
                SubCategoryValue.IsEnabled = false;
            }
        }

        private void SetCategoryComboBoxItems(bool inExpenses, bool inIncomes) {

            CategoryValue.Items.Clear();

            foreach (Category catItem in Dal.GetAllCategories()) {

                if ((catItem.VisibleInExpenses && inExpenses)
                    || (catItem.VisibleInIncomes && inIncomes)) {

                    CategoryValue.Items.Add(new ComboBoxItem {
                        Content = catItem.Name,
                        Tag = catItem.Id
                    });
                }
            }
        }

        private void SetSubCategoryComboBoxItems(bool inExpenses, bool inIncomes) {

            SubCategoryValue.Items.Clear();

            if (CategoryValue.SelectedIndex == -1)
                return;

            foreach (var subCatItem in Dal.GetSubCategoriesByBossId((int)((ComboBoxItem)CategoryValue.SelectedItem).Tag)) {
                if ((!subCatItem.VisibleInExpenses || !inExpenses) && (!subCatItem.VisibleInIncomes || !inIncomes))
                    continue;

                if (SubCategoryValue.Items.Count == 0)
                    SubCategoryValue.Items.Add(new ComboBoxItem {
                        Content = "Brak",
                        Tag = -1,
                    });

                SubCategoryValue.Items.Add(new ComboBoxItem {
                    Content = subCatItem.Name,
                    Tag = subCatItem.Id
                });
            }
            SubCategoryValue.IsEnabled = SubCategoryValue.Items.Count != 0;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetSubCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
        }
        
        private void SubCategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (SubCategoryValue.SelectedIndex == 0)
                SubCategoryValue.SelectedIndex--;
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
            isUnfocused = false;

            if (CostValue.Text == "")
                return;

            CostValue.Text = acceptedCostValue;
            CostValue.SelectionStart = CostValue.Text.Length;
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
            isUnfocused = true;

            if (CostValue.Text != "")
                CostValue.Text = NewOperation.ToCurrencyString(CostValue.Text);
        }

        private void CostValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

            if (string.IsNullOrEmpty(CostValue.Text)) {
                acceptedCostValue = string.Empty;
                return;
            }

            if (isUnfocused)
                return;

            if (regex.Match(CostValue.Text).Value != CostValue.Text) {
                int whereIsSelection = CostValue.SelectionStart;
                CostValue.Text = acceptedCostValue;
                CostValue.SelectionStart = whereIsSelection - 1;
            }
            acceptedCostValue = CostValue.Text;
        }

        private async void UsePatternButton_Click(object sender, RoutedEventArgs e) {
            var contentDialogItem = new OperationPatternsContentDialog();
            var result = await contentDialogItem.ShowAsync();
            SetValuesFromPattern(contentDialogItem.SetOperation());
        }

        private void SetValuesFromPattern(Operation selectedOperation) {

            if (selectedOperation == null)
                return;

            if (selectedOperation.isExpense)
                Expense_RadioButton.IsChecked = true;
            else
                Income_RadioButton.IsChecked = true;

            CostValue.Text = NewOperation.ToCurrencyString(selectedOperation.Cost);
            acceptedCostValue = NewOperation.ToCurrencyWithoutSymbolString(selectedOperation.Cost);

            NameValue.Text = selectedOperation.Title;

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => (int)i.Tag == selectedOperation.CategoryId);
            SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => (int)item.Tag == selectedOperation.SubCategoryId);
            PayFormValue.SelectedItem = PayFormValue.Items.OfType<Account>().SingleOrDefault(item => item.Id == selectedOperation.MoneyAccountId);

            if (selectedOperation.MoreInfo != null)
                MoreInfoValue.Text = selectedOperation.MoreInfo;
        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {
            SaveButton.IsEnabled = !string.IsNullOrEmpty(CostValue.Text);
        }
       
        private OperationPattern GetNewOperationPattern() {
            return new OperationPattern {
                Title = NameValue.Text,
                isExpense = (bool)Expense_RadioButton.IsChecked,
                Cost = decimal.Parse(acceptedCostValue, Settings.ActualCultureInfo),
                CategoryId = GetCategoryId(),
                SubCategoryId = GetSubCategoryId(),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = ((Account)PayFormValue.SelectedItem).Id,
                DeviceId = Settings.DeviceId
            };
        }

        private Operation GetNewOperation(Account moneyAccount, bool isExpense) {
            Operation operation = GetNewOperationPattern().ToOperation();
            operation.MoneyAccountId = moneyAccount.Id;
            operation.isExpense = isExpense;
            operation.Date = DateValue.Date == null ? string.Empty : DateValue.Date.Value.ToString("yyyy.MM.dd");
            operation.VisibleInStatistics = !HideInStatisticsToggle.IsOn;
            return operation;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {

            if ((bool)Transfer_RadioButton.IsChecked) {
                if (InitialAccount.SelectedItem == null || DestinationAccount.SelectedItem == null) {
                    ShowMessageDialog("Nie wybrano kont");
                    return;
                }
                
                Dal.SaveOperation(GetNewOperation((Account)InitialAccount.SelectedItem, true));
                Dal.SaveOperation(GetNewOperation((Account)DestinationAccount.SelectedItem, false));
            }
            else {
                Dal.SaveOperation(GetNewOperation((Account)PayFormValue.SelectedItem, (bool)Expense_RadioButton.IsChecked));

                if (SaveAsAssetToggle.IsOn)
                    Dal.SaveOperationPattern(GetNewOperationPattern());
            }
            
            Frame.Navigate(typeof(OperationsPage), NavigateToThisMonthAfterSave());
        }

        private static async void ShowMessageDialog(string message) {
            MessageDialog dialog = new MessageDialog(message);
            var result = await dialog.ShowAsync();
        }

        private int GetCategoryId() {
            return CategoryValue.SelectedIndex == -1 ?
                        1 :
                        (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;
        }

        private int GetSubCategoryId() {
            return SubCategoryValue.SelectedIndex == -1 ?
                        -1 :
                        (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;
        }
       
        private DateTime NavigateToThisMonthAfterSave() {
            return DateValue.Date == null || DateValue.Date > DateTime.Today ?
                DateTime.Today.AddMonths(1) :
                DateValue.Date.Value.DateTime;
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            DateValue.Date = null;
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) {
            PayFormValue.SelectedIndex = 0;
        }

        private void PayFormValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (PayFormValue.SelectedItem == null)
                PayFormValue.SelectedIndex = 0;
        }
    }
}
