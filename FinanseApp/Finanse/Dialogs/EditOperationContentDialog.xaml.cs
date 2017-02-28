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

namespace Finanse.Dialogs {
    public sealed partial class EditOperationContentDialog : ContentDialog, INotifyPropertyChanged {

        private readonly Regex regex = NewOperation.GetRegex();
        
        private bool isLoaded = false;
        private bool isUnfocused = true;

        private readonly Operation operationToEdit = null;
        private readonly OperationPattern operationPatternToEdit = null;
        private readonly OperationPattern originalOperationPattern;

        private string acceptedCostValue = string.Empty;

        public EditOperationContentDialog(Operation operationToEdit) {
            this.InitializeComponent();
            this.operationToEdit = operationToEdit;

            DateValue.MaxDate = Settings.MaxDate;
            this.originalOperationPattern = operationToEdit.ToOperation();
            //  setMoneyAccountComboBoxItems();

            SetEditedOperationValues(operationToEdit);
        }

        public EditOperationContentDialog(OperationPattern operationPatternToEdit) {
            this.InitializeComponent();
            this.operationPatternToEdit = operationPatternToEdit;

            DateValue.Visibility = Visibility.Collapsed;
            this.originalOperationPattern = operationPatternToEdit.ToOperation();

            SetEditedOperationValues(operationPatternToEdit);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private readonly List<Account> AccountsWithoutCards = AccountsDal.GetAccountsWithoutCards();
        private readonly List<Account> Accounts = AccountsDal.GetAllMoneyAccounts();


        private ObservableCollection<Account> AccountsComboBox {
            get {
                if ((bool)Income_RadioButton.IsChecked) {
                    return new ObservableCollection<Account>(AccountsWithoutCards);
                }
                else {
                    return new ObservableCollection<Account>(Accounts);
                }
            }
        }

        private void SetEditedOperationValues(OperationPattern operationPattern) {

            if (operationPattern.isExpense)
                Expense_RadioButton.IsChecked = true;
            else Income_RadioButton.IsChecked = true;

            CostValue.Text = NewOperation.ToCurrencyString(operationPattern.Cost);
            acceptedCostValue = NewOperation.ToCurrencyWithoutSymbolString(operationPattern.Cost);
            
            NameValue.Text = operationPattern.Title;

            if (operationPattern is Operation && !string.IsNullOrEmpty(((Operation)operationPattern).Date)) {
                Operation operation = (Operation)operationPattern;
                DateValue.Date = DateTime.Parse(operation.Date);
                HideInStatisticsToggle.IsOn = !operation.VisibleInStatistics;
            }

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => (int)i.Tag == operationPattern.CategoryId);
            SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => (int)item.Tag == operationPattern.SubCategoryId);
      //      PayFormValue.SelectedItem = PayFormValue.Items.OfType<Account>().SingleOrDefault(item => item.Id == operationPattern.MoneyAccountId);

            if (!string.IsNullOrEmpty(operationPattern.MoreInfo))
                MoreInfoValue.Text = operationPattern.MoreInfo;
           
            isLoaded = true;
        }


        private void SetPrimaryButtonEnabling() {
            IsPrimaryButtonEnabled = !string.IsNullOrEmpty(CostValue.Text) && IsOperationEdited();
        }

        private bool IsOperationEdited() {
            if (isLoaded)
                /*
                 * 
                return operationPatternToEdit == null ?
                    !operationToEdit.Equals(EditedOperation()) :
                    !operationPatternToEdit.Equals(EditedOperationPattern());
                 */
                return !operationPatternToEdit?.Equals(EditedOperationPattern()) ?? !operationToEdit.Equals(EditedOperation());
            return false;
        }

        public Operation EditedOperation() {
            return new Operation {
                Id = operationToEdit.Id,
                Date = GetDate(),
                Title = NameValue.Text,
                Cost = decimal.Parse(acceptedCostValue, Settings.ActualCultureInfo),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                CategoryId = GetCategoryId(),
                SubCategoryId = GetSubCategoryId(),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = ((Account)PayFormValue.SelectedItem).Id,
                VisibleInStatistics = !HideInStatisticsToggle.IsOn,
            };
        }

        public OperationPattern EditedOperationPattern() {
            return new OperationPattern {
                Id = operationPatternToEdit.Id,
                Title = NameValue.Text,
                Cost = decimal.Parse(acceptedCostValue, Settings.ActualCultureInfo),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                CategoryId = GetCategoryId(),
                SubCategoryId = GetSubCategoryId(),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = ((Account)PayFormValue.SelectedItem).Id,
            };
        }

        private string GetDate() {
            return DateValue.Date == null ?
                string.Empty :
                DateValue.Date.Value.ToString("yyyy.MM.dd");
        }

        private int GetCategoryId() {
            return CategoryValue.SelectedIndex != -1 ?
                (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag :
                1;
        }

        private int GetSubCategoryId() {
            return SubCategoryValue.SelectedIndex != -1 ?
                (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag :
                -1;
        }

        private void NowaOperacja_DodajClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

             if (operationPatternToEdit == null)
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
            SetPrimaryButtonEnabling();
        }

        private void SubCategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (SubCategoryValue.SelectedIndex == 0)
                SubCategoryValue.SelectedIndex--;

            SetPrimaryButtonEnabling();
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
           isUnfocused = false;

            if (string.IsNullOrEmpty(CostValue.Text))
                return;

            CostValue.Text = acceptedCostValue;
            CostValue.SelectionStart = CostValue.Text.Length;
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
           isUnfocused = true;

            if (!string.IsNullOrEmpty(CostValue.Text))
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

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetPrimaryButtonEnabling();
        }

        private void NameValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetPrimaryButtonEnabling();
        }

        private void DateValue_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) {
            SetPrimaryButtonEnabling();
        }

        private void PayFormValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (PayFormValue.SelectedItem == null)
                PayFormValue.SelectedIndex = 0;
            SetPrimaryButtonEnabling();
        }

        private void MoreInfoValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetPrimaryButtonEnabling();
        }

        private void HideInStatisticsToggle_Toggled(object sender, RoutedEventArgs e) {
            SetPrimaryButtonEnabling();
        }

        private void PayFormValue_Loaded(object sender, RoutedEventArgs e) {
            PayFormValue.SelectedItem = PayFormValue.Items.OfType<Account>().SingleOrDefault(item => item.Id == originalOperationPattern.MoneyAccountId);
        }
    }
}
