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

namespace Finanse.Dialogs {
    public sealed partial class EditOperationContentDialog : ContentDialog, INotifyPropertyChanged {

        private Regex regex = NewOperation.getRegex();
        
        private bool isLoaded = false;
        private bool isUnfocused = true;

        private Operation operationToEdit = null;
        private OperationPattern operationPatternToEdit = null;
        private OperationPattern dupa;

        private string acceptedCostValue = string.Empty;

        public EditOperationContentDialog(Operation operationToEdit) {

            this.InitializeComponent();
            this.operationToEdit = operationToEdit;

            DateValue.MaxDate = Settings.getMaxDate();
            this.dupa = operationToEdit.toOperation();
            //  setMoneyAccountComboBoxItems();

            setEditedOperationValues(operationToEdit);
        }

        public EditOperationContentDialog(OperationPattern operationPatternToEdit) {

            this.InitializeComponent();
            this.operationPatternToEdit = operationPatternToEdit;

            DateValue.Visibility = Visibility.Collapsed;
            this.dupa = operationPatternToEdit.toOperation();
            //setMoneyAccountComboBoxItems();

            setEditedOperationValues(operationPatternToEdit);
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<Account> AccountsWithoutCards = AccountsDal.getAccountsWithoutCards();
        private List<Account> Accounts = AccountsDal.getAllMoneyAccounts();


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

        private void setEditedOperationValues(OperationPattern operationPattern) {

            if (operationPattern.isExpense)
                Expense_RadioButton.IsChecked = true;
            else Income_RadioButton.IsChecked = true;

            CostValue.Text = NewOperation.toCurrencyString(operationPattern.Cost);
            acceptedCostValue = NewOperation.toCurrencyWithoutSymbolString(operationPattern.Cost);
            
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


        private void setPrimaryButtonEnabling() {
            IsPrimaryButtonEnabled = !string.IsNullOrEmpty(CostValue.Text) && isOperationEdited();
        }

        private bool isOperationEdited() {
            if (isLoaded)
                return operationPatternToEdit == null ?
                    !operationToEdit.Equals(editedOperation()) :
                    !operationPatternToEdit.Equals(editedOperationPattern());
            return false;
        }

        public Operation editedOperation() {
            return new Operation {
                Id = operationToEdit.Id,
                Date = getDate(),
                Title = NameValue.Text,
                Cost = decimal.Parse(acceptedCostValue, Settings.getActualCultureInfo()),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                CategoryId = getCategoryId(),
                SubCategoryId = getSubCategoryId(),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = ((Account)PayFormValue.SelectedItem).Id,
                VisibleInStatistics = !HideInStatisticsToggle.IsOn,
            };
        }

        public OperationPattern editedOperationPattern() {
            return new OperationPattern {
                Id = operationPatternToEdit.Id,
                Title = NameValue.Text,
                Cost = decimal.Parse(acceptedCostValue, Settings.getActualCultureInfo()),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                CategoryId = getCategoryId(),
                SubCategoryId = getSubCategoryId(),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = ((Account)PayFormValue.SelectedItem).Id,
            };
        }

        private string getDate() {
            return DateValue.Date == null ?
                string.Empty :
                DateValue.Date.Value.ToString("yyyy.MM.dd");
        }

        private int getCategoryId() {
            return CategoryValue.SelectedIndex != -1 ?
                (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag :
                1;
        }

        private int getSubCategoryId() {
            return SubCategoryValue.SelectedIndex != -1 ?
                (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag :
                -1;
        }

        private void NowaOperacja_DodajClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

             if (operationPatternToEdit == null)
                Dal.saveOperation(editedOperation());
             else
                Dal.saveOperationPattern(editedOperationPattern());
        }

        private void TypeOfOperationRadioButton_Checked(object sender, RoutedEventArgs e) {
            RaisePropertyChanged("AccountsComboBox");

            if (CategoryValue.SelectedIndex != -1) {
                int idOfSelectedCategory = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;
                int idOfSelectedSubCategory = -1;

                if (SubCategoryValue.SelectedIndex != -1)
                    idOfSelectedSubCategory = (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;

                setCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

                if (CategoryValue.Items.OfType<ComboBoxItem>().Any(i => (int)i.Tag == idOfSelectedCategory))
                    CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().Single(i => (int)i.Tag == idOfSelectedCategory);

                else
                    SubCategoryValue.IsEnabled = false;

                if (idOfSelectedSubCategory != -1) {
                    if (SubCategoryValue.Items.OfType<OperationSubCategory>().Any(i => i.Id == idOfSelectedSubCategory)) {
                        OperationSubCategory subCatItem = Dal.getOperationSubCategoryById(idOfSelectedSubCategory);
                        SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == subCatItem.Name);
                    }
                }
            }

            else {
                setCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
                SubCategoryValue.IsEnabled = false;
            }

            setPrimaryButtonEnabling();
        }

        private void setCategoryComboBoxItems(bool inExpenses, bool inIncomes) {

            CategoryValue.Items.Clear();

            foreach (OperationCategory catItem in Dal.getAllCategories()) {

                if ((catItem.VisibleInExpenses && inExpenses) 
                    || (catItem.VisibleInIncomes && inIncomes)) {

                    CategoryValue.Items.Add(new ComboBoxItem {
                        Content = catItem.Name,
                        Tag = catItem.Id
                    });
                }
            }
        }

        private void setSubCategoryComboBoxItems(bool inExpenses, bool inIncomes) {

            SubCategoryValue.Items.Clear();

            if (CategoryValue.SelectedIndex != -1) {

                foreach (OperationSubCategory subCatItem in Dal.getOperationSubCategoriesByBossId((int)((ComboBoxItem)CategoryValue.SelectedItem).Tag)) {

                    if ((subCatItem.VisibleInExpenses && inExpenses)
                        || (subCatItem.VisibleInIncomes && inIncomes)) {

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
                }

                SubCategoryValue.IsEnabled = !(SubCategoryValue.Items.Count == 0);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            setSubCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
            setPrimaryButtonEnabling();
        }

        private void SubCategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (SubCategoryValue.SelectedIndex == 0)
                SubCategoryValue.SelectedIndex--;

            setPrimaryButtonEnabling();
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
           isUnfocused = false;

            if (!string.IsNullOrEmpty(CostValue.Text)) {
                CostValue.Text = acceptedCostValue;
                CostValue.SelectionStart = CostValue.Text.Length;
            }
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
           isUnfocused = true;

            if (!string.IsNullOrEmpty(CostValue.Text))
                CostValue.Text = NewOperation.toCurrencyString(CostValue.Text);
        }

        private void CostValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

            if (string.IsNullOrEmpty(CostValue.Text)) {
                acceptedCostValue = string.Empty;
                return;
            }

            else if (isUnfocused)
                return;

            else if (regex.Match(CostValue.Text).Value != CostValue.Text) {
                int whereIsSelection = CostValue.SelectionStart;
                CostValue.Text = acceptedCostValue;
                CostValue.SelectionStart = whereIsSelection - 1;
            }
            
            acceptedCostValue = CostValue.Text;
        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {
            setPrimaryButtonEnabling();
        }

        private void NameValue_TextChanged(object sender, TextChangedEventArgs e) {
            setPrimaryButtonEnabling();
        }

        private void DateValue_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) {
            setPrimaryButtonEnabling();
        }

        private void PayFormValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (PayFormValue.SelectedItem == null)
                PayFormValue.SelectedIndex = 0;
            setPrimaryButtonEnabling();
        }

        private void MoreInfoValue_TextChanged(object sender, TextChangedEventArgs e) {
            setPrimaryButtonEnabling();
        }

        private void HideInStatisticsToggle_Toggled(object sender, RoutedEventArgs e) {
            setPrimaryButtonEnabling();
        }

        private void PayFormValue_Loaded(object sender, RoutedEventArgs e) {
            PayFormValue.SelectedItem = PayFormValue.Items.OfType<Account>().SingleOrDefault(item => item.Id == dupa.MoneyAccountId);
        }
    }
}
