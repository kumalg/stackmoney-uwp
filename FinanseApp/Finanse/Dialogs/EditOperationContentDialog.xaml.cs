using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Text.RegularExpressions;
using Finanse.Models.MoneyAccounts;
using System.Collections.Generic;
using System.ComponentModel;
using Finanse.Models.Categories;
using Finanse.Models.Helpers;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;

namespace Finanse.Dialogs {
    public sealed partial class EditOperationContentDialog : INotifyPropertyChanged {

        private readonly Regex _regex = NewOperation.GetRegex();
        
        private bool _isLoaded;
        private bool _isUnfocused = true;
        private readonly int MaxLength = NewOperation.MaxLength;

        private readonly Operation _operationToEdit;
        private readonly OperationPattern _operationPatternToEdit;
        private readonly OperationPattern _originalOperationPattern;

        private string _acceptedCostValue = string.Empty;

        public EditOperationContentDialog(Operation operationToEdit) {
            InitializeComponent();
            _operationToEdit = operationToEdit;

            DateValue.MaxDate = Settings.MaxDate;
            _originalOperationPattern = operationToEdit.ToOperation();
            //  setMoneyAccountComboBoxItems();

            SetEditedOperationValues(operationToEdit);
        }

        public EditOperationContentDialog(OperationPattern operationPatternToEdit) {
            InitializeComponent();
            _operationPatternToEdit = operationPatternToEdit;

            DateValue.Visibility = Visibility.Collapsed;
            _originalOperationPattern = operationPatternToEdit.ToOperation();

            SetEditedOperationValues(operationPatternToEdit);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private static readonly IEnumerable<MAccount> Accounts = MAccountsDal.GetAllAccountsAndSubAccounts();
        private static readonly IEnumerable<MAccount> AccountsWithoutCards = Accounts.Where(i => !(i is SubMAccount));


        private IEnumerable<MAccount> AccountsComboBox => (bool) Income_RadioButton.IsChecked
            ? AccountsWithoutCards
            : Accounts;

        private void SetEditedOperationValues(OperationPattern operationPattern) {

            if (operationPattern.isExpense)
                Expense_RadioButton.IsChecked = true;
            else Income_RadioButton.IsChecked = true;

            CostValue.Text = NewOperation.ToCurrencyString(operationPattern.Cost);
            _acceptedCostValue = NewOperation.ToCurrencyWithoutSymbolString(operationPattern.Cost);
            
            NameValue.Text = operationPattern.Title;

            if (operationPattern is Operation && !string.IsNullOrEmpty(((Operation)operationPattern).Date)) {
                Operation operation = (Operation)operationPattern;
                DateValue.Date = DateTime.Parse(operation.Date);
                HideInStatisticsToggle.IsOn = !operation.VisibleInStatistics;
            }

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => i.Tag.ToString() == operationPattern.CategoryId);
            SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => item.Tag.ToString() == operationPattern.SubCategoryId);
      //      PayFormValue.SelectedItem = PayFormValue.Items.OfType<Account>().SingleOrDefault(item => item.Id == operationPattern.MoneyAccountId);

            if (!string.IsNullOrEmpty(operationPattern.MoreInfo))
                MoreInfoValue.Text = operationPattern.MoreInfo;
           
            _isLoaded = true;
        }


        private void SetPrimaryButtonEnabling() {
            IsPrimaryButtonEnabled = !string.IsNullOrEmpty(CostValue.Text) && IsOperationEdited();
        }

        private bool IsOperationEdited() {
            if (_isLoaded)
                /*
                 * 
                return operationPatternToEdit == null ?
                    !operationToEdit.Equals(EditedOperation()) :
                    !operationPatternToEdit.Equals(EditedOperationPattern());
                 */
                return !_operationPatternToEdit?.Equals(EditedOperationPattern()) ?? !_operationToEdit.Equals(EditedOperation());
            return false;
        }

        public Operation EditedOperation() {
            return new Operation {
                Id = _operationToEdit.Id,
                GlobalId = _operationToEdit.GlobalId,
                Date = GetDate(),
                Title = NameValue.Text,
                Cost = decimal.Parse(_acceptedCostValue, Settings.ActualCultureInfo),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                CategoryId = GetCategoryId(),
                SubCategoryId = GetSubCategoryId(),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = ((MAccount)PayFormValue.SelectedItem).GlobalId,
                VisibleInStatistics = !HideInStatisticsToggle.IsOn,
            };
        }

        public OperationPattern EditedOperationPattern() {
            return new OperationPattern {
                Id = _operationPatternToEdit.Id,
                GlobalId = _operationPatternToEdit.GlobalId,
                Title = NameValue.Text,
                Cost = decimal.Parse(_acceptedCostValue, Settings.ActualCultureInfo),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                CategoryId = GetCategoryId(),
                SubCategoryId = GetSubCategoryId(),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = ((MAccount)PayFormValue.SelectedItem).GlobalId,
            };
        }

        private string GetDate() {
            return DateValue.Date == null ?
                string.Empty :
                DateValue.Date.Value.ToString("yyyy.MM.dd");
        }

        private string GetCategoryId() => CategoryValue.SelectedIndex != -1
            ? ((ComboBoxItem) CategoryValue.SelectedItem).Tag.ToString()
            : string.Empty;

        private string GetSubCategoryId() => SubCategoryValue.SelectedIndex != -1
            ? (string)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag
            : string.Empty;

        private void NowaOperacja_DodajClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

             if (_operationPatternToEdit == null)
                Dal.SaveOperation(EditedOperation());
             else
                Dal.SaveOperationPattern(EditedOperationPattern());
        }

        private void TypeOfOperationRadioButton_Checked(object sender, RoutedEventArgs e) {
            RaisePropertyChanged("AccountsComboBox");

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

                if (idOfSelectedSubCategory != -1) {
                    if (SubCategoryValue.Items.OfType<SubCategory>().Any(i => i.Id == idOfSelectedSubCategory)) {
                        SubCategory subCatItem = Dal.GetSubCategoryById(idOfSelectedSubCategory);
                        SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == subCatItem.Name);
                    }
                }
            }

            else {
                SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
                SubCategoryValue.IsEnabled = false;
            }

            SetPrimaryButtonEnabling();
        }

        private void SetCategoryComboBoxItems(bool inExpenses, bool inIncomes) {
            if (CategoryValue.Items == null)
                return;

            CategoryValue.Items.Clear();

            foreach (Category catItem in Dal.GetAllCategories()) {

                if ((catItem.VisibleInExpenses && inExpenses) 
                    || (catItem.VisibleInIncomes && inIncomes)) {

                    CategoryValue.Items.Add(new ComboBoxItem {
                        Content = catItem.Name,
                        Tag = catItem.GlobalId
                    });
                }
            }
        }

        private void SetSubCategoryComboBoxItems(bool inExpenses, bool inIncomes) {
            if (SubCategoryValue.Items == null)
                return;

            SubCategoryValue.Items.Clear();

            if (CategoryValue.SelectedIndex == -1)
                return;

            foreach (var subCatItem in Dal.GetSubCategoriesByBossId(((ComboBoxItem)CategoryValue.SelectedItem).Tag.ToString())) {
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
            SetPrimaryButtonEnabling();
        }

        private void SubCategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (SubCategoryValue.SelectedIndex == 0)
                SubCategoryValue.SelectedIndex--;

            SetPrimaryButtonEnabling();
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
           _isUnfocused = false;

            if (string.IsNullOrEmpty(CostValue.Text))
                return;

            CostValue.Text = _acceptedCostValue;
            CostValue.SelectionStart = CostValue.Text.Length;
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
           _isUnfocused = true;

            if (!string.IsNullOrEmpty(CostValue.Text))
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

        private void PayFormValue_Loaded(object sender, RoutedEventArgs e)
            => PayFormValue.SelectedItem = Accounts.SingleOrDefault(item => item.GlobalId == _originalOperationPattern.MoneyAccountId);


        private void PayFormValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (PayFormValue.SelectedItem == null)
                PayFormValue.SelectedIndex = 0;
            SetPrimaryButtonEnabling();
        }

        private void Element_TectChanged(object sender, TextChangedEventArgs e) => SetPrimaryButtonEnabling();
        private void HideInStatisticsToggle_Toggled(object sender, RoutedEventArgs e) => SetPrimaryButtonEnabling();
        private void DateValue_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) => SetPrimaryButtonEnabling();
        
    }
}
