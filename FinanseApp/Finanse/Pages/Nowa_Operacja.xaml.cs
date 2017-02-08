using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Finanse.Pages {

    public sealed partial class Nowa_Operacja : Page {

        private Regex regex = NewOperation.getRegex();
        private string acceptedCostValue = string.Empty;
        private bool isUnfocused = true;

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            setDefaultPageValues();
            base.OnNavigatedTo(e); 
        }

        private void setDefaultPageValues() {
            CostValue.Text = string.Empty;
            acceptedCostValue = string.Empty;
            NameValue.Text = string.Empty;
            DateValue.Date = DateTime.Today;
            CategoryValue.SelectedIndex = -1;
            SubCategoryValue.SelectedIndex = -1;
            SubCategoryValue.IsEnabled = false;
            PayFormValue.SelectedIndex = 0;
            MoreInfoValue.Text = string.Empty;
            SaveAsAssetToggle.IsOn = false;
            HideInStatisticsToggle.IsOn = false;
            Expense_RadioButton.IsChecked = true;
        }

        public List<ComboBoxItem> listOfMoneyAccounts() {
            List<ComboBoxItem> list = new List<ComboBoxItem>();
            foreach (MoneyAccount account in Dal.getAllMoneyAccounts()) {
                list.Add(new ComboBoxItem {
                    Content = account.Name,
                    Tag = account.Id
                });
            }
            return list;
        }

        private void setItemSource(ComboBox comboBox, List<ComboBoxItem> list) {
            comboBox.ItemsSource = list;
        }

        public Nowa_Operacja() {
          
            this.InitializeComponent();

            PayFormValue.ItemsSource = listOfMoneyAccounts();
            InitialAccount.ItemsSource = listOfMoneyAccounts();
            DestinationAccount.ItemsSource = listOfMoneyAccounts();

            DateValue.Date = DateTime.Today;
            DateValue.MaxDate = Settings.getMaxDate();
            DateValue.MinDate = Settings.getMinDate();

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            if (PayFormValue.Items.Count > 0)
                PayFormValue.SelectedIndex = 0;
            if (InitialAccount.Items.Count > 0)
                InitialAccount.SelectedIndex = 0;
        }

        public Windows.Globalization.DayOfWeek firstDayOfWeek() {
            return Settings.getFirstDayOfWeek();
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

                if (idOfSelectedSubCategory != -1) {
                    if (SubCategoryValue.Items.OfType<OperationSubCategory>().Any(i => i.Id == idOfSelectedSubCategory)) {
                        OperationSubCategory subCatItem = Dal.getOperationSubCategoryById(idOfSelectedSubCategory);
                        SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == subCatItem.Name);
                    }
                }
            }

            else {
                SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
                SubCategoryValue.IsEnabled = false;
            }
        }

        private void SetCategoryComboBoxItems(bool inExpenses, bool inIncomes) {

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

        private void SetSubCategoryComboBoxItems(bool inExpenses, bool inIncomes) {

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
            SetSubCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
        }
        
        private void SubCategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (SubCategoryValue.SelectedIndex == 0)
                SubCategoryValue.SelectedIndex--;
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
            isUnfocused = false;

            if (CostValue.Text != "") {
                CostValue.Text = acceptedCostValue;
                CostValue.SelectionStart = CostValue.Text.Length;
            }
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
            isUnfocused = true;

            if (CostValue.Text != "")
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

        private async void UsePatternButton_Click(object sender, RoutedEventArgs e) {

            Operation selectedOperation = null;
            var ContentDialogItem = new OperationPatternsContentDialog(selectedOperation);
            var result = await ContentDialogItem.ShowAsync();
            selectedOperation = ContentDialogItem.setOperation();

            if (selectedOperation != null)
                setValuesFromPattern(selectedOperation);
        }

        private void setValuesFromPattern(Operation selectedOperation) {

            if (selectedOperation.isExpense)
                Expense_RadioButton.IsChecked = true;
            else
                Income_RadioButton.IsChecked = true;

            CostValue.Text = NewOperation.toCurrencyString(selectedOperation.Cost);
            acceptedCostValue = NewOperation.toCurrencyWithoutSymbolString(selectedOperation.Cost);

            NameValue.Text = selectedOperation.Title;

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => (int)i.Tag == selectedOperation.CategoryId);
            SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => (int)item.Tag == selectedOperation.SubCategoryId);
            PayFormValue.SelectedItem = PayFormValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => (int)item.Tag == selectedOperation.MoneyAccountId);

            if (selectedOperation.MoreInfo != null)
                MoreInfoValue.Text = selectedOperation.MoreInfo;
        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {
            SaveButton.IsEnabled = !String.IsNullOrEmpty(CostValue.Text);
        }
       
        private OperationPattern getNewOperationPattern() {
            return new OperationPattern {
                Title = NameValue.Text,
                isExpense = (bool)Expense_RadioButton.IsChecked,
                Cost = decimal.Parse(acceptedCostValue, Settings.getActualCultureInfo()),
                CategoryId = getCategoryId(),
                SubCategoryId = getSubCategoryId(),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag,
            };
        }

        private Operation getNewOperation(ComboBoxItem moneyAccount, bool isExpense) {
            Operation operation = getNewOperationPattern().toOperation();
            operation.isExpense = isExpense;
            operation.Date = DateValue.Date == null ? string.Empty : DateValue.Date.Value.ToString("yyyy.MM.dd");
            operation.VisibleInStatistics = !HideInStatisticsToggle.IsOn;
            return operation;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e) {

            if ((bool)Transfer_RadioButton.IsChecked) {
                if (InitialAccount.SelectedItem == null || DestinationAccount.SelectedItem == null) {
                    showMessageDialog("Nie wybrano kont");
                    return;
                }
                
                Dal.saveOperation(getNewOperation((ComboBoxItem)InitialAccount.SelectedItem, true));
                Dal.saveOperation(getNewOperation((ComboBoxItem)DestinationAccount.SelectedItem, false));
            }
            else {
                Dal.saveOperation(getNewOperation((ComboBoxItem)PayFormValue.SelectedItem, (bool)Expense_RadioButton.IsChecked));

                if (SaveAsAssetToggle.IsOn)
                    Dal.saveOperationPattern(getNewOperationPattern());
            }
            
            Frame.Navigate(typeof(Strona_glowna), navigateToThisMonthAfterSave());
        }

        private async void showMessageDialog(string message) {
            MessageDialog dialog = new MessageDialog(message);
            var result = await dialog.ShowAsync();
        }

        private int getCategoryId() {
            return CategoryValue.SelectedIndex == -1 ?
                        1 :
                        (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;
        }

        private int getSubCategoryId() {
            return SubCategoryValue.SelectedIndex == -1 ?
                        -1 :
                        (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;
        }
       
        private DateTime navigateToThisMonthAfterSave() {
            return DateValue.Date == null || DateValue.Date > DateTime.Today ?
                DateTime.Today.AddMonths(1) :
                DateValue.Date.Value.DateTime;
        }
    }
}
