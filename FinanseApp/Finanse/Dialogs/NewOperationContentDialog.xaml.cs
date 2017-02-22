using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Finanse.DataAccessLayer;
using Finanse.Models;
using System.Text.RegularExpressions;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Dialogs {
    public sealed partial class NewOperationContentDialog : ContentDialog {

        private Regex regex = NewOperation.getRegex();

        private bool _isSaved = false;

        private Operation operationToEdit;
        
        private readonly ObservableCollection<OperationPattern> patterns;

        private bool isPatternEditing = false;
        private string acceptedCostValue = "";
        private int whereIsSelection;

        private bool isUnfocused = true;

        public NewOperationContentDialog(Operation operationToEdit) {

            this.InitializeComponent();

            this.operationToEdit = operationToEdit;

            IsPrimaryButtonEnabled = false;

            DateValue.MaxDate = Settings.getMaxDate();

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            foreach (Account account in AccountsDal.getAllMoneyAccounts()) {

                PayFormValue.Items.Add(new ComboBoxItem {
                    Content = account.Name,
                    Tag = account.Id
                });
            }

            SaveAsAssetTitle.Visibility = Visibility.Collapsed;
            SaveAsAssetToggle.Visibility = Visibility.Collapsed;

            EditAndPatternSetters();
            if (!operationToEdit.Date.Equals(""))
                DateValue.Date = Convert.ToDateTime(operationToEdit.Date); /// tu się wywala kiedy edytujesz zaplanowaną bez daty /// JUŻ NIE
        }
        public NewOperationContentDialog(ObservableCollection<OperationPattern> patterns, Operation editedOperation, bool isPatternEditing) {

            this.InitializeComponent();

            this.operationToEdit = editedOperation;
            this.isPatternEditing = isPatternEditing;
            this.patterns = patterns;

            IsPrimaryButtonEnabled = false;

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            foreach (Account account in AccountsDal.getAllMoneyAccounts()) {

                PayFormValue.Items.Add(new ComboBoxItem {
                    Content = account.Name,
                    Tag = account.Id
                });
            }

            switch (isPatternEditing) {

                case true: {

                        Title = "Edycja szablonu";
                        PrimaryButtonText = "Zapisz";
                        SaveAsAssetTitle.Visibility = Visibility.Collapsed;
                        SaveAsAssetToggle.Visibility = Visibility.Collapsed;

                        EditAndPatternSetters();
                        DateValue.Visibility = Visibility.Collapsed;
                        break;
                    };

                default: {

                        EditAndPatternSetters();

                        if (PayFormValue.Items.Count > 0)
                            PayFormValue.SelectedIndex = 0;

                        break;
                    };
            }
        }

        private void SetNowaOperacjaButton() {

            IsPrimaryButtonEnabled = (CostValue.Text != "");
        }

        private void EditAndPatternSetters() {

            if (operationToEdit.isExpense) {
                Expense_RadioButton.IsChecked = true;
            }
            else
                Income_RadioButton.IsChecked = true;

            NameValue.Text = operationToEdit.Title;

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => (int)i.Tag == operationToEdit.CategoryId);
            SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => (int)item.Tag == operationToEdit.SubCategoryId);
            PayFormValue.SelectedItem = PayFormValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => (int)item.Tag == operationToEdit.MoneyAccountId);

            CostValue.Text = NewOperation.toCurrencyString(operationToEdit.Cost);//editedOperation.Cost.ToString("C", Settings.getActualCultureInfo());
            acceptedCostValue = NewOperation.toCurrencyWithoutSymbolString(operationToEdit.Cost);//editedOperation.Cost.ToString(Settings.getActualCultureInfo());

            if (operationToEdit.MoreInfo != null)
                MoreInfoValue.Text = operationToEdit.MoreInfo;
        }

        public bool isSaved() {
            return _isSaved;
        }

        public Operation newOperation() {
            return new Operation {
                Id = operationToEdit.Id,
                Date = DateValue.Date.Value.ToString("yyyy.MM.dd"),
                Title = NameValue.Text,
                Cost = decimal.Parse(acceptedCostValue, Settings.getActualCultureInfo()),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                CategoryId = CategoryValue.SelectedIndex != -1 ?
                             (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag
                             : 1,
                SubCategoryId = SubCategoryValue.SelectedIndex != -1 ?
                                (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag
                                : -1,
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
            };
        }

        public OperationPattern newOperationPattern() {
            return new Operation {
                Id = operationToEdit.Id,
                Title = NameValue.Text,
                Cost = decimal.Parse(acceptedCostValue, Settings.getActualCultureInfo()),
                isExpense = (bool)Expense_RadioButton.IsChecked,
                CategoryId = CategoryValue.SelectedIndex != -1 ?
                             (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag
                             : 1,
                SubCategoryId = SubCategoryValue.SelectedIndex != -1 ?
                                (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag
                                : -1,
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
            };
        }

        private void NowaOperacja_DodajClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            int subCategoryId = -1;

            if (SubCategoryValue.SelectedIndex != -1)
                subCategoryId = (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;

            Dal.saveOperation(newOperation());

            if (isPatternEditing) {

                OperationPattern itemPattern = new OperationPattern {
                    Id = operationToEdit.Id,
                    Title = NameValue.Text,
                    Cost = decimal.Parse(acceptedCostValue, Settings.getActualCultureInfo()),
                    isExpense = (bool)Expense_RadioButton.IsChecked,
                    CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                    SubCategoryId = subCategoryId,
                    MoreInfo = MoreInfoValue.Text,
                    MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
                };

                patterns[patterns.IndexOf(patterns.SingleOrDefault(i => i.Id == operationToEdit.Id))] = itemPattern;

                Dal.saveOperationPattern(itemPattern);
            }

            _isSaved = true;
        }

        private void TypeOfOperationRadioButton_Checked(object sender, RoutedEventArgs e) {

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
                        SubCategory subCatItem = Dal.getSubCategoryById(idOfSelectedSubCategory);
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

            foreach (Category catItem in Dal.getAllCategories()) {

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

                foreach (SubCategory subCatItem in Dal.getSubCategoriesByBossId((int)((ComboBoxItem)CategoryValue.SelectedItem).Tag)) {

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

        private void DateValue_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) {
            if (DateValue.Date == null)
                DateValue.Date = DateTime.Today;
        }

        private void DateValue_Closed(object sender, object e) {
            DateValue.Focus(FocusState.Programmatic);
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
                CostValue.Text = acceptedCostValue;
                CostValue.SelectionStart = whereIsSelection;
            }

            whereIsSelection = CostValue.SelectionStart;
            acceptedCostValue = CostValue.Text;
        }

        private void CostValue_SelectionChanged(object sender, RoutedEventArgs e) {
            whereIsSelection = CostValue.SelectionStart;
        }

        private async void UsePatternButton_Click(object sender, RoutedEventArgs e) {
            Hide();
            Operation selectedOperation = null;
            var ContentDialogItem = new OperationPatternsContentDialog(selectedOperation);
            var result = await ContentDialogItem.ShowAsync();
        }

        private void UsePatternButton_Loading(FrameworkElement sender, object args) {
            Button yco = sender as Button;

            //if (whichOptions == "edit")
                yco.Visibility = Visibility.Collapsed;
        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetNowaOperacjaButton();
        }

        private void OperationType_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
            //RadioButton radioButton = sender as RadioButton;

          //  IsPrimaryButtonEnabled = true;//Expense_RadioButton.IsChecked != editedOperation.isExpense;
        }

        private void NameValue_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
          //  IsPrimaryButtonEnabled = NameValue.Text != editedOperation.Title && !CostValue.Text.Equals("");
        }

        private void DateValue_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
           // IsPrimaryButtonEnabled = String.Format("{0:yyyy.MM.dd}", DateValue.Date) != editedOperation.Date && !CostValue.Text.Equals("");
        }

        private void CategoryValue_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
          //  IsPrimaryButtonEnabled = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag != editedOperation.CategoryId && !CostValue.Text.Equals("");
        }

        private void SubCategoryValue_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
           // IsPrimaryButtonEnabled = (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag != editedOperation.SubCategoryId && !CostValue.Text.Equals("");
        }

        private void PayFormValue_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
          //  IsPrimaryButtonEnabled = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag != editedOperation.MoneyAccountId && !CostValue.Text.Equals("");
        }

        private void MoreInfoValue_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args) {
          //  IsPrimaryButtonEnabled = MoreInfoValue.Text != editedOperation.MoreInfo && !CostValue.Text.Equals("");
        }
    }
}
