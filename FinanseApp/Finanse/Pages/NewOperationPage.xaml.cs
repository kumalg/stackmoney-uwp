using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Finanse.Models.Categories;
using Finanse.Models.Helpers;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;

namespace Finanse.Pages {

    public sealed partial class NewOperationPage : INotifyPropertyChanged {

        private readonly Regex _regex = NewOperation.GetRegex();
        private string _acceptedCostValue = string.Empty;
        private bool _isUnfocused = true;
        private readonly int MaxLength = NewOperation.MaxLengthOfValue;

        private readonly TextBoxEvents _textBoxEvents = new TextBoxEvents();

        private IEnumerable<MAccount> _accounts;
        private IEnumerable<MAccount> Accounts {
            get { return _accounts; }
            set {
                _accounts = value;
                RaisePropertyChanged("Accounts");
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            SetDefaultPageValues();
            SetCategoryComboBoxItems(true, false);
            Accounts = MAccountsDal.GetAllAccountsAndSubAccounts();

            base.OnNavigatedTo(e);
        }

        private void SetDefaultPageValues() {
            CostValue.Text = string.Empty;
            _acceptedCostValue = string.Empty;
            NameValue.Text = string.Empty;
            DateValue.Date = DateTime.Today;
            CategoryValue.SelectedIndex = -1;
            SubCategoryValue.SelectedIndex = -1;
            SubCategoryValue.IsEnabled = false;
            MoreInfoValue.Text = string.Empty;
            SaveAsAssetToggle.IsOn = false;
            HideInStatisticsToggle.IsOn = false;
            Expense_RadioButton.IsChecked = true;
        }


        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        

        public NewOperationPage() {

            InitializeComponent();

            DateValue.Date = DateTime.Today;
            DateValue.MaxDate = Settings.MaxDate;
            DateValue.MinDate = Settings.MinDate;

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            if (PayFormValue.Items != null && PayFormValue.Items.Count > 0)
                PayFormValue.SelectedIndex = 0;
            if (AccountsFromComboBox.Items != null && AccountsFromComboBox.Items.Count > 0)
                AccountsFromComboBox.SelectedIndex = 0;
        }

        public Windows.Globalization.DayOfWeek FirstDayOfWeek => Settings.FirstDayOfWeek;

        private void TransferRadioButton_Checked(object sender, RoutedEventArgs e) {
            TransferAccounts_Grid.Visibility = Visibility.Visible;

            PayFormValue.Visibility = Visibility.Collapsed;
            SaveAsPattern_StackPanel.Visibility = Visibility.Collapsed;
            UsePatternButton.Visibility = Visibility.Collapsed;

            string selectedCategoryId = string.Empty;
            string selectedSubCategoryId = string.Empty;

            if (CategoryValue.SelectedIndex != -1) {
                selectedCategoryId = ((ComboBoxItem)CategoryValue.SelectedItem).Tag.ToString();

                if (SubCategoryValue.SelectedIndex != -1)
                    selectedSubCategoryId = ((ComboBoxItem)SubCategoryValue.SelectedItem).Tag.ToString();
            }

            SetCategoryComboBoxItems(true, true);

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => i.Tag.ToString() == selectedCategoryId);

            if (CategoryValue.SelectedItem != null)
                SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => i.Tag.ToString() == selectedSubCategoryId);
        }

        private void ExpenseOrIncomeRadioButton_Checked(object sender, RoutedEventArgs e) {
            TransferAccounts_Grid.Visibility = Visibility.Collapsed;

            PayFormValue.Visibility = Visibility.Visible;
            SaveAsPattern_StackPanel.Visibility = Visibility.Visible;
            UsePatternButton.Visibility = Visibility.Visible;


            if (CategoryValue.SelectedIndex != -1) {
                string idOfSelectedCategory = ((ComboBoxItem)CategoryValue.SelectedItem).Tag.ToString();
                string idOfSelectedSubCategory = string.Empty;

                if (SubCategoryValue.SelectedIndex != -1)
                    idOfSelectedSubCategory = ((ComboBoxItem)SubCategoryValue.SelectedItem).Tag.ToString();

                SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

                if (CategoryValue.Items.OfType<ComboBoxItem>().Any(i => i.Tag.ToString() == idOfSelectedCategory))
                    CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().Single(i => i.Tag.ToString() == idOfSelectedCategory);
                else
                    SubCategoryValue.IsEnabled = false;

                if (string.IsNullOrEmpty(idOfSelectedSubCategory))
                    return;

                if (SubCategoryValue.Items.OfType<SubCategory>().All(i => i.GlobalId != idOfSelectedSubCategory))
                    return;

                Category subCatItem = CategoriesDal.GetCategoryByGlobalId(idOfSelectedSubCategory);
                SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == subCatItem.Name);
            }

            else {
                SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
                SubCategoryValue.IsEnabled = false;
            }
        }

        private void SetCategoryComboBoxItems(bool inExpenses, bool inIncomes) {

            CategoryValue.Items?.Clear();

            foreach (Category catItem in CategoriesDal.GetAllCategories()) {

                if ((catItem.VisibleInExpenses && inExpenses)
                    || (catItem.VisibleInIncomes && inIncomes)) {

                    CategoryValue.Items?.Add(new ComboBoxItem {
                        Content = catItem.Name,
                        Tag = catItem.GlobalId
                    });
                }
            }
        }

        private void SetSubCategoryComboBoxItems(bool inExpenses, bool inIncomes) {

            SubCategoryValue.Items?.Clear();

            if (CategoryValue.SelectedIndex == -1)
                return;

            foreach (var subCatItem in CategoriesDal.GetSubCategoriesByBossId(((ComboBoxItem)CategoryValue.SelectedItem).Tag.ToString())) {
                if ((!subCatItem.VisibleInExpenses || !inExpenses) && (!subCatItem.VisibleInIncomes || !inIncomes))
                    continue;

                if (SubCategoryValue.Items.Count == 0)
                    SubCategoryValue.Items.Add(new ComboBoxItem {
                        Content = "Brak",
                        Tag = -1,
                    });

                SubCategoryValue.Items.Add(new ComboBoxItem {
                    Content = subCatItem.Name,
                    Tag = subCatItem.GlobalId
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
            _isUnfocused = false;

            if (CostValue.Text == "")
                return;

            CostValue.Text = _acceptedCostValue;
            CostValue.SelectionStart = CostValue.Text.Length;
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
            _isUnfocused = true;

            if (CostValue.Text != "")
                CostValue.Text = NewOperation.ToCurrencyString(CostValue.Text);
        }

        private void CostValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

            if (string.IsNullOrEmpty(CostValue.Text)) {
                _acceptedCostValue = string.Empty;
                return;
            }

            if (_isUnfocused)
                return;

            if (_regex.Match(CostValue.Text).Value != CostValue.Text) {
                int whereIsSelection = CostValue.SelectionStart;
                CostValue.Text = _acceptedCostValue;
                CostValue.SelectionStart = whereIsSelection - 1;
            }
            _acceptedCostValue = CostValue.Text;
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
            _acceptedCostValue = NewOperation.ToCurrencyWithoutSymbolString(selectedOperation.Cost);

            NameValue.Text = selectedOperation.Title;

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => i.Tag.ToString() == selectedOperation.Category.GlobalId);
            SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => item.Tag.ToString() == selectedOperation.SubCategory?.GlobalId);
            PayFormValue.SelectedItem = PayFormValue.Items.OfType<MAccount>().SingleOrDefault(item => item.GlobalId == selectedOperation.MoneyAccountId);

            if (selectedOperation.MoreInfo != null)
                MoreInfoValue.Text = selectedOperation.MoreInfo;
        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {
            decimal value;
            try {
                value = decimal.Parse(_acceptedCostValue, Settings.ActualCultureInfo);
            }
            catch (Exception ee) {
                Debug.WriteLine(ee.Message);
                SaveButton.IsEnabled = false;
                return;
            }
            SaveButton.IsEnabled = value > 0;
        }
       
        private OperationPattern GetNewOperationPattern() {
            return new OperationPattern {
                Title = NameValue.Text.Trim(),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                Cost = decimal.Parse(_acceptedCostValue, Settings.ActualCultureInfo),
                CategoryGlobalId = !string.IsNullOrEmpty(GetSubCategoryId) ? GetSubCategoryId : GetCategoryId,
                //CategoryId = GetCategoryId,
                //SubCategoryId = GetSubCategoryId,
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = ((MAccount)PayFormValue.SelectedItem)?.GlobalId
            };
        }

        private Operation GetNewOperation(MAccount moneyAccount, bool isExpense) {
            Operation operation = GetNewOperationPattern().ToOperation();
            operation.MoneyAccountId = moneyAccount.GlobalId;
            operation.isExpense = isExpense;
            operation.Date = DateValue.Date == null ? string.Empty : DateValue.Date.Value.ToString("yyyy.MM.dd");
            operation.VisibleInStatistics = !HideInStatisticsToggle.IsOn;
            return operation;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {

            if ((bool)Transfer_RadioButton.IsChecked) {
                if (AccountsFromComboBox.SelectedItem == null || AccountsToComboBox.SelectedItem == null) {
                    ShowMessageDialog("Nie wybrano kont");
                    return;
                }
                
                Dal.SaveOperation(GetNewOperation((MAccount)AccountsFromComboBox.SelectedItem, true));
                Dal.SaveOperation(GetNewOperation((MAccount)AccountsToComboBox.SelectedItem, false));
            }
            else {
                Dal.SaveOperation(GetNewOperation((MAccount)PayFormValue.SelectedItem, (bool)Expense_RadioButton.IsChecked));

                if (SaveAsAssetToggle.IsOn)
                    Dal.SaveOperationPattern(GetNewOperationPattern());
            }
            
            Frame.Navigate(typeof(OperationsPage), NavigateToThisMonthAfterSave());
        }

        private static async void ShowMessageDialog(string message) {
            MessageDialog dialog = new MessageDialog(message);
            var result = await dialog.ShowAsync();
        }

        private string GetCategoryId => CategoryValue.SelectedIndex == -1
            ? CategoriesDal.GetDefaultCategory().GlobalId //TODO
            : ((ComboBoxItem)CategoryValue.SelectedItem)?.Tag.ToString();

        private string GetSubCategoryId => SubCategoryValue.SelectedIndex == -1
            ? string.Empty
            : ((ComboBoxItem)SubCategoryValue.SelectedItem)?.Tag.ToString();

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

        private void AccountsFromComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) 
            => SameAccountsInTransfer(sender as ComboBox, AccountsToComboBox);

        private void AccountsToComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) 
            => SameAccountsInTransfer(sender as ComboBox, AccountsFromComboBox);

        private void SameAccountsInTransfer(ComboBox first, ComboBox second) {
            if (first.SelectedItem == null)
                return;

            var destinationSelectedAccount = AccountsToComboBox.SelectedItem;
            var initialSelectedAccount = AccountsFromComboBox.SelectedItem;

            if (destinationSelectedAccount == initialSelectedAccount)
                second.SelectedItem = null;
        }
    }
}
