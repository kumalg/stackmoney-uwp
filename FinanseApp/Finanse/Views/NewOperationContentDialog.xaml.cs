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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class NewOperationContentDialog : ContentDialog {

        Operation editedOperation;

        SQLite.Net.SQLiteConnection conn;

        Settings settings;

        string whichOptions;
        string acceptedCostValue = "";
        int whereIsSelection;

        bool isUnfocused = true;

        public NewOperationContentDialog(SQLite.Net.SQLiteConnection conn, Operation editedOperation, string whichOptions) {

            this.InitializeComponent();

            IsPrimaryButtonEnabled = false;

            this.conn = conn;
            this.editedOperation = editedOperation;
            this.whichOptions = whichOptions;

            settings = conn.Table<Settings>().ElementAt(0);

            DateValue.MaxDate = DateTime.Today;

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            foreach (MoneyAccount account in conn.Table<MoneyAccount>()) {

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
                        DateValue.Date = editedOperation.Date;
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
                        break;
                    };

                default: {

                        SubCategoryValue.IsEnabled = false;
                        DateValue.Date = DateTime.Today;
                        break;
                    };
            }
        }

        private void SetNowaOperacjaButton() {
            if (CostValue.Text != ""
                && NameValue.Text != ""
                && (Income_RadioButton.IsChecked == true || Expense_RadioButton.IsChecked == true)
                //&& DateValue.Date != null
                && CategoryValue.SelectedItem != null
                && PayFormValue.SelectedItem != null) {

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

            foreach (var item in CategoryValue.Items.OfType<ComboBoxItem>()) {
                if ((int)item.Tag == editedOperation.CategoryId) {
                    CategoryValue.SelectedItem = CategoryValue.Items.OfType<ComboBoxItem>().Single(i => (int)i.Tag == editedOperation.CategoryId);
                    break;
                }
            }

            foreach (var item in conn.Table<OperationSubCategory>()) {
                if (item.OperationCategoryId == editedOperation.SubCategoryId) {
                    OperationSubCategory subCatItem = conn.Table<OperationSubCategory>().Single(i => i.OperationCategoryId == editedOperation.SubCategoryId);
                    SubCategoryValue.SelectedItem = SubCategoryValue.Items.OfType<ComboBoxItem>().Single(ri => ri.Content.ToString() == subCatItem.Name);
                    break;
                }
            }

            foreach (var item in conn.Table<MoneyAccount>()) {
                if (item.Id == editedOperation.MoneyAccountId) {
                    PayFormValue.SelectedItem = PayFormValue.Items.OfType<ComboBoxItem>().Single(i => (int)i.Tag == editedOperation.MoneyAccountId);
                    break;
                }
            }

            CostValue.Text = editedOperation.Cost.ToString("C", new CultureInfo(settings.CultureInfoName));
            acceptedCostValue = editedOperation.Cost.ToString();

            if (editedOperation.MoreInfo != null)
                MoreInfoValue.Text = editedOperation.MoreInfo;
        }

        private void NowaOperacja_DodajClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            int subCategoryInt = -1;

            if (SubCategoryValue.SelectedIndex != -1)
                subCategoryInt = (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;

            Operation item = new Operation {
                Title = NameValue.Text,
                Cost = decimal.Parse(acceptedCostValue),
                CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                SubCategoryId = subCategoryInt,
                Date = DateValue.Date,
                PayForm = ((ComboBoxItem)PayFormValue.SelectedItem).Content.ToString(),
                MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag,
            };
            if (MoreInfoValue.Text != "")
                item.MoreInfo = MoreInfoValue.Text;

            // WYDATEK CZY WPŁYW
            if (Expense_RadioButton.IsChecked == true)
                item.isExpense = true;
            else
                item.isExpense = false;

            switch (whichOptions) {
                case "edit": {
                        item.Id = editedOperation.Id;
                        conn.Update(item);
                        break;
                    }
                case "editpattern": {
                        OperationPattern itemPattern = new OperationPattern {
                            Id = editedOperation.Id,
                            Title = NameValue.Text,
                            Cost = decimal.Parse(acceptedCostValue),
                            CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                            SubCategoryId = subCategoryInt,
                            MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
                        };
                        if (MoreInfoValue.Text != "")
                            itemPattern.MoreInfo = MoreInfoValue.Text;
                        if (Expense_RadioButton.IsChecked == true)
                            itemPattern.isExpense = true;
                        else
                            itemPattern.isExpense = false;
                        conn.Update(itemPattern);
                        break;
                    }
                default: {
                        conn.Insert(item);

                        if (SaveAsAssetToggle.IsOn) {
                            OperationPattern itemPattern = new OperationPattern {
                                Title = NameValue.Text,
                                Cost = decimal.Parse(acceptedCostValue),
                                CategoryId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag,
                                SubCategoryId = subCategoryInt,
                                MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag
                            };
                            if (MoreInfoValue.Text != "")
                                itemPattern.MoreInfo = MoreInfoValue.Text;
                            if (Expense_RadioButton.IsChecked == true)
                                itemPattern.isExpense = true;
                            else
                                itemPattern.isExpense = false;
                            conn.Insert(itemPattern);
                        }
                        break;
                    }
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e) {
            SetNowaOperacjaButton();
        }

        private void TypeOfOperationRadioButton_Checked(object sender, RoutedEventArgs e) {
            SetNowaOperacjaButton();

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
                        OperationSubCategory subCatItem = conn.Table<OperationSubCategory>().Single(i => i.OperationCategoryId == idOfSelectedSubCategory);
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

            foreach (OperationCategory catItem in conn.Query<OperationCategory>("SELECT * FROM OperationCategory ORDER BY Name ASC")) {

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

                foreach (OperationSubCategory subCatItem in conn.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory ORDER BY Name ASC")) {

                    if (subCatItem.BossCategoryId == (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag) {

                        if ((subCatItem.VisibleInExpenses && inExpenses)
                            || (subCatItem.VisibleInIncomes && inIncomes)) {

                            if (SubCategoryValue.Items.Count == 0)
                                SubCategoryValue.Items.Add(new ComboBoxItem {
                                    Content = "Brak",
                                });
                            SubCategoryValue.Items.Add(new ComboBoxItem {
                                Content = subCatItem.Name,
                                Tag = subCatItem.OperationCategoryId
                            });

                        }

                    }

                }

                if (SubCategoryValue.Items.Count == 0)
                    SubCategoryValue.IsEnabled = false;
                else
                    SubCategoryValue.IsEnabled = true;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetNowaOperacjaButton();

            SetSubCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);
        }

        private void DateValue_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) {
            //SetNowaOperacjaButton();
            if (DateValue.Date == null)
                DateValue.Date = DateTime.Today;
        }

        private void DateValue_Closed(object sender, object e) {
            DateValue.Focus(FocusState.Programmatic);
        }

        private void PayFormValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetNowaOperacjaButton();
        }

        private void SubCategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {

            if (SubCategoryValue.SelectedIndex == 0)
                SubCategoryValue.SelectedIndex--;
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
            isUnfocused = false;

            if (CostValue.Text != "") {
                CostValue.Text = acceptedCostValue;//CostValue.Text.Replace(" zł", "");
                CostValue.SelectionStart = CostValue.Text.Length;
            }
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
            isUnfocused = true;

            if(CostValue.Text != "")
                CostValue.Text = decimal.Parse(CostValue.Text).ToString("C", new CultureInfo(settings.CultureInfoName));
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

            var ContentDialogItem = new OperationPatternsContentDialog(conn);

            var result = await ContentDialogItem.ShowAsync();
        }

        private void UsePatternButton_Loading(FrameworkElement sender, object args) {
            Button yco = sender as Button;

            if (whichOptions == "edit")
                yco.Visibility = Visibility.Collapsed;
        }
    }
}
