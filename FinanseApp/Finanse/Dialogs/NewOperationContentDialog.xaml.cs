using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Finanse.DataAccessLayer;
using Finanse.Models;

namespace Finanse.Dialogs {
    public sealed partial class NewOperationContentDialog : ContentDialog {

        private Operation editedOperation;

        private readonly ObservableCollection<GroupInfoList<Operation>> _source;
        private readonly ObservableCollection<OperationPattern> patterns;

        private bool isPatternEditing = false;
        private string acceptedCostValue = "";
        private int whereIsSelection;

        private bool isUnfocused = true;

        public NewOperationContentDialog(ObservableCollection<GroupInfoList<Operation>> _source, Operation editedOperation) {

            this.InitializeComponent();

            this.editedOperation = editedOperation;
            this._source = _source;

            IsPrimaryButtonEnabled = false;

            DateValue.MaxDate = Settings.GetMaxDate();

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            foreach (MoneyAccount account in Dal.GetAllMoneyAccounts()) {

                PayFormValue.Items.Add(new ComboBoxItem {
                    Content = account.Name,
                    Tag = account.Id
                });
            }

            Title = "Edycja operacji";
            PrimaryButtonText = "Zapisz";
            SaveAsAssetTitle.Visibility = Visibility.Collapsed;
            SaveAsAssetToggle.Visibility = Visibility.Collapsed;

            EditAndPatternSetters();
            DateValue.Date = Convert.ToDateTime(editedOperation.Date);
        }
        public NewOperationContentDialog(ObservableCollection<OperationPattern> patterns, Operation editedOperation, bool isPatternEditing) {

            this.InitializeComponent();

            this.editedOperation = editedOperation;
            this.isPatternEditing = isPatternEditing;
            this.patterns = patterns;

            IsPrimaryButtonEnabled = false;

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            foreach (MoneyAccount account in Dal.GetAllMoneyAccounts()) {

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

            if (editedOperation.isExpense) {
                Expense_RadioButton.IsChecked = true;
            }
            else
                Income_RadioButton.IsChecked = true;

            NameValue.Text = editedOperation.Title;

            CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(i => (int)i.Tag == editedOperation.CategoryId);
            SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => (int)item.Tag == editedOperation.SubCategoryId);
            PayFormValue.SelectedItem = PayFormValue.Items.OfType<ComboBoxItem>().SingleOrDefault(item => (int)item.Tag == editedOperation.MoneyAccountId);

            CostValue.Text = editedOperation.Cost.ToString("C", Settings.GetActualCurrency());
            acceptedCostValue = editedOperation.Cost.ToString();

            if (editedOperation.MoreInfo != null)
                MoreInfoValue.Text = editedOperation.MoreInfo;
        }

        private void NowaOperacja_DodajClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            int subCategoryId = -1;

            if (SubCategoryValue.SelectedIndex != -1)
                subCategoryId = (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;

            if (_source != null) {

                Operation item = new Operation {
                    Id = editedOperation.Id,
                    Date = String.Format("{0:yyyy/MM/dd}", DateValue.Date),
                    Title = NameValue.Text,
                    Cost = decimal.Parse(acceptedCostValue),
                    isExpense = (bool)Expense_RadioButton.IsChecked,
                    CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                    SubCategoryId = subCategoryId,
                    MoreInfo = MoreInfoValue.Text,
                    MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
                };

                GroupInfoList<Operation> group = _source.SingleOrDefault(i => ((GroupHeaderByDay)i.Key).date == editedOperation.Date);

                if (item.Date == editedOperation.Date)
                    group[group.IndexOf(group.Single(i => i.Id == item.Id))] = item;

                else {

                    if (group.Count == 1)
                        _source.Remove(group);

                    else
                        group.Remove(group.Single(i => i.Id == item.Id));

                    if (!DateValue.Date.ToString().Equals("")
                        && DateValue.Date.Value.Year == Convert.ToDateTime(editedOperation.Date).Year
                        && DateValue.Date.Value.Month == Convert.ToDateTime(editedOperation.Date).Month)

                        AddOperationToList(item);
                }

                Dal.SaveOperation(item);
            }

            else if (isPatternEditing) {

                OperationPattern itemPattern = new OperationPattern {
                    Id = editedOperation.Id,
                    Title = NameValue.Text,
                    Cost = decimal.Parse(acceptedCostValue),
                    isExpense = (bool)Expense_RadioButton.IsChecked,
                    CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                    SubCategoryId = subCategoryId,
                    MoreInfo = MoreInfoValue.Text,
                    MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
                };

                patterns[patterns.IndexOf(patterns.SingleOrDefault(i => i.Id == editedOperation.Id))] = itemPattern;

                Dal.SaveOperationPattern(itemPattern);
            }
        }

        private void AddOperationToList(Operation item) {

            GroupInfoList<Operation> group = _source.SingleOrDefault(i => ((GroupHeaderByDay)i.Key).date == item.Date);

            if (group == null) {
                group = new GroupInfoList<Operation>() { Key = new GroupHeaderByDay(item.Date) };
                group.Add(item);

                int i = 0;

                for (i = 0; i < _source.Count; i++)
                    if (((GroupHeaderByDay)_source.ElementAt(i).Key).date.CompareTo(item.Date) < 0)
                        break;

                _source.Insert(i, group);
            }
            else {
                group.Insert(0, item);
            }
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
                    if (SubCategoryValue.Items.OfType<OperationSubCategory>().Any(i => i.Id == idOfSelectedSubCategory)) {
                        OperationSubCategory subCatItem = Dal.GetOperationSubCategoryById(idOfSelectedSubCategory);
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

            foreach (OperationCategory catItem in Dal.GetAllCategories()) {

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

                foreach (OperationSubCategory subCatItem in Dal.GetOperationSubCategoriesByBossId((int)((ComboBoxItem)CategoryValue.SelectedItem).Tag)) {

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

            if(CostValue.Text != "")
                CostValue.Text = decimal.Parse(CostValue.Text).ToString("C", Settings.GetActualCurrency());
        }

        private void CostValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

            if (CostValue.Text == "")
                return;

            if (isUnfocused)
                return;

            if (CostValue.Text.Any(c => !char.IsDigit(c))) {
                foreach (char letter in CostValue.Text)
                    if (!char.IsDigit(letter) && letter != ',') {
                        CostValue.Text = acceptedCostValue;
                        CostValue.SelectionStart = whereIsSelection;
                    }

                if (CostValue.Text.Count(c => c == ',') > 1) {
                    CostValue.Text = acceptedCostValue;
                    CostValue.SelectionStart = whereIsSelection;
                }

                if (CostValue.Text.Count(c => c == ',') == 1) {

                    int charactersAfterComma = CostValue.Text.Length - CostValue.Text.IndexOf(",") - 1;

                    if (charactersAfterComma > 2) {
                        CostValue.Text = acceptedCostValue;
                        CostValue.SelectionStart = whereIsSelection;
                    }
                }
            }
            acceptedCostValue = CostValue.Text;
            whereIsSelection = CostValue.SelectionStart;
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
           // IsPrimaryButtonEnabled = String.Format("{0:yyyy/MM/dd}", DateValue.Date) != editedOperation.Date && !CostValue.Text.Equals("");
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
