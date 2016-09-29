using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Finanse.Elements;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Globalization;
using Finanse.DataAccessLayer;
using Finanse.Models;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Dialogs {

    public sealed partial class NewOperationContentDialog : ContentDialog {

        Operation editedOperation;

        private readonly ObservableCollection<GroupInfoList<Operation>> _source;

        string whichOptions;
        string acceptedCostValue = "";
        int whereIsSelection;

        bool isUnfocused = true;

        public NewOperationContentDialog(ObservableCollection<GroupInfoList<Operation>> _source, Operation editedOperation, string whichOptions) {

            this.InitializeComponent();

            this.editedOperation = editedOperation;
            this.whichOptions = whichOptions;
            this._source = _source;

            IsPrimaryButtonEnabled = false;

            DateValue.MaxDate = DateTime.Today;

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            foreach (MoneyAccount account in Dal.GetAllMoneyAccounts()) {

                PayFormValue.Items.Add(new ComboBoxItem {
                    Content = account.Name,
                    Tag = account.Id
                });
            }

            switch (whichOptions) {

                case "edit": {

                        Title = "Edycja operacji";
                        PrimaryButtonText = "Zapisz";
                        SaveAsAssetTitle.Visibility = Visibility.Collapsed;
                        SaveAsAssetToggle.Visibility = Visibility.Collapsed;

                        EditAndPatternSetters();
                        DateValue.Date = Convert.ToDateTime(editedOperation.Date);
                        break;
                    };

                case "editpattern": {

                        Title = "Edycja szablonu";
                        PrimaryButtonText = "Zapisz";
                        SaveAsAssetTitle.Visibility = Visibility.Collapsed;
                        SaveAsAssetToggle.Visibility = Visibility.Collapsed;

                        EditAndPatternSetters();
                        DateValue.Visibility = Visibility.Collapsed;
                        break;
                    };

                case "pattern": {

                        EditAndPatternSetters();
                        DateValue.Date = DateTime.Today;

                        if (PayFormValue.Items.Count > 0)
                            PayFormValue.SelectedIndex = 0;

                        break;
                    };

                default: {

                        SubCategoryValue.IsEnabled = false;
                        DateValue.Date = DateTime.Today;

                        if (PayFormValue.Items.Count > 0)
                            PayFormValue.SelectedIndex = 0;

                        break;
                    };
            }
        }

        private void SetNowaOperacjaButton() {
            if (CostValue.Text != "") {

                IsPrimaryButtonEnabled = true;
            }
            else
                IsPrimaryButtonEnabled = false;
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

            Operation item = new Operation {
                Id = 0,
                Title = NameValue.Text,
                isExpense = (bool)Expense_RadioButton.IsChecked,
                Cost = decimal.Parse(acceptedCostValue),
                CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                SubCategoryId = subCategoryId,
                Date = String.Format("{0:yyyy/MM/dd}", DateValue.Date),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag,
            };

            switch (whichOptions) {
                case "edit": {
                        item.Id = editedOperation.Id;

                        GroupInfoList<Operation> group = _source.SingleOrDefault(i => i.Key == editedOperation.Date);

                        if (item.Date == editedOperation.Date) {
                            group[group.IndexOf(group.Single(i => i.Id == item.Id))] = item;
                        }
                        else {
                            if (group.Count == 1)
                                _source.Remove(group);
                            else
                                group.Remove(group.Single(i => i.Id == item.Id));

                            AddOperationToList(item);
                        }

                        Dal.SaveOperation(item);

                        break;
                    }
                case "editpattern": {
                        OperationPattern itemPattern = new OperationPattern {
                            Id = editedOperation.Id,
                            Title = NameValue.Text,
                            isExpense = (bool)Expense_RadioButton.IsChecked,
                            Cost = decimal.Parse(acceptedCostValue),
                            CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                            SubCategoryId = subCategoryId,
                            MoreInfo = MoreInfoValue.Text,
                            MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
                        };

                        Dal.SaveOperationPattern(itemPattern);
                        break;
                    }
                default: {

                        AddOperationToList(item);

                        Dal.SaveOperation(item);

                        if (SaveAsAssetToggle.IsOn) {
                            OperationPattern itemPattern = new OperationPattern {
                                Id = 0,
                                Title = NameValue.Text,
                                Cost = decimal.Parse(acceptedCostValue),
                                isExpense = (bool)Expense_RadioButton.IsChecked,
                                CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                                SubCategoryId = subCategoryId,
                                MoreInfo = MoreInfoValue.Text,
                                MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
                            };

                            Dal.SaveOperationPattern(itemPattern);
                        }
                        break;
                    }
            }
        }

        private void AddOperationToList(Operation item) {

            GroupInfoList<Operation> group = _source.SingleOrDefault(i => i.Key == item.Date);
            if (group == null) {

                DateTime date = Convert.ToDateTime(item.Date);

                GroupInfoList<Operation> newGroup = new GroupInfoList<Operation> {
                    Key = item.Date,
                    dayNum = String.Format("{0:dd}", date),
                    day = String.Format("{0:dddd}", date),
                    month = String.Format("{0:MMMM yyyy}", date),
                };

                int i = 0;

                if (_source.Count != 0) {
                    bool check = true;
                    newGroup.Insert(0, item);
                    while (check) {
                        if (Convert.ToDateTime(_source[i].Key) > Convert.ToDateTime(newGroup.Key))
                            i++;
                        else
                            check = false;
                    }
                }
                _source.Insert(i, newGroup);
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
                    if (SubCategoryValue.Items.OfType<OperationSubCategory>().Any(i => i.OperationCategoryId == idOfSelectedSubCategory)) {
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

                foreach (OperationSubCategory subCatItem in Dal.GetOperationSubCategoryByBossId((int)((ComboBoxItem)CategoryValue.SelectedItem).Tag)) {

                    if ((subCatItem.VisibleInExpenses && inExpenses)
                        || (subCatItem.VisibleInIncomes && inIncomes)) {

                        if (SubCategoryValue.Items.Count == 0)
                            SubCategoryValue.Items.Add(new ComboBoxItem {
                                Content = "Brak",
                                Tag = -1,
                            });
                        SubCategoryValue.Items.Add(new ComboBoxItem {
                            Content = subCatItem.Name,
                            Tag = subCatItem.OperationCategoryId
                        });

                    }

                }

                if (SubCategoryValue.Items.Count == 0)
                    SubCategoryValue.IsEnabled = false;
                else
                    SubCategoryValue.IsEnabled = true;
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

            var ContentDialogItem = new OperationPatternsContentDialog(_source);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void UsePatternButton_Loading(FrameworkElement sender, object args) {
            Button yco = sender as Button;

            if (whichOptions == "edit")
                yco.Visibility = Visibility.Collapsed;
        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {
            SetNowaOperacjaButton();
        }
    }
}
