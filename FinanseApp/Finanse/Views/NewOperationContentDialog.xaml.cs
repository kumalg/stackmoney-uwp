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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class NewOperationContentDialog : ContentDialog {

        public ObservableCollection<Operation> Operations;

        public List<OperationCategory> OperationCategories;

        SQLite.Net.SQLiteConnection conn;

        int editedId;
        string acceptedCostValue;
        int whereIsSelection;
        bool isUnfocused = true;

        public NewOperationContentDialog(ObservableCollection<Operation> Operations, SQLite.Net.SQLiteConnection conn,
            string editedTitle, int editedId, decimal editedCost, DateTimeOffset? editedDate, string editedCategory, string editedSubCategory, string editedExpenseOrIncome, string editedPayForm) {

            this.InitializeComponent();

            IsPrimaryButtonEnabled = false;

            this.Operations = Operations;
            this.conn = conn;
            this.editedId = editedId;

            DateValue.MaxDate = DateTime.Today;

            /* DODAWANIE KATEGORII DO COMBOBOX'A */
            foreach (OperationCategory OperationCategories_ComboBox in conn.Query<OperationCategory>("SELECT * FROM OperationCategory ORDER BY Name ASC")) {

                if (OperationCategories_ComboBox.VisibleInExpenses && OperationCategories_ComboBox.VisibleInIncomes) {
                    CategoryValue.Items.Add(new ComboBoxItem {
                        Content = OperationCategories_ComboBox.Name
                    });
                }
            }

            if (editedId != -1) {
                Title = "Edycja operacji";
                PrimaryButtonText = "Zapisz";
                SaveAsAssetTitle.Visibility = Visibility.Collapsed;
                SaveAsAssetToggle.Visibility = Visibility.Collapsed;

                if (editedExpenseOrIncome == "expense") {
                    Expense_RadioButton.IsChecked = true;
                }
                else
                    Income_RadioButton.IsChecked = true;

                NameValue.Text = editedTitle;
                DateValue.Date = editedDate;

                CostValue.Text = String.Format("{0:c}", editedCost);   
            }

            else
                SubCategoryValue.IsEnabled = false;
        }

        private void NowaOperacja_AnulujClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

        }

        private void SetNowaOperacjaButton() {
            if (CostValue.Text != ""
                && NameValue.Text != ""
                && (Income_RadioButton.IsChecked == true || Expense_RadioButton.IsChecked == true)
                && DateValue.Date != null
                && CategoryValue.SelectedItem != null
                && PayFormValue.SelectedItem != null) {

                IsPrimaryButtonEnabled = true;
            }
            else
                IsPrimaryButtonEnabled = false;
        }

        private void NowaOperacja_DodajClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {

            string subCategoryString = null;

            if (SubCategoryValue.SelectedIndex != -1)
                subCategoryString = ((ComboBoxItem)SubCategoryValue.SelectedItem).Content.ToString();

            Operation item = new Operation {
                Title = NameValue.Text,
                Cost = decimal.Parse(CostValue.Text.Replace(" zł", "")),
                Category = ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString(),
                SubCategory = subCategoryString,
                Date = DateValue.Date,
                PayForm = ((ComboBoxItem)PayFormValue.SelectedItem).Content.ToString(),
            };

            // WYDATEK CZY WPŁYW
            if (Expense_RadioButton.IsChecked == true)
                item.ExpenseOrIncome = "expense";
            else if (Income_RadioButton.IsChecked == true)
                item.ExpenseOrIncome = "income";

            // DODAWANIE
            if (editedId == -1) {
                Operations.Add(item);
                conn.Insert(item);
            }

            // EDYTOWANIE
            else {
                item.Id = Operations.Single(id => id.Id == editedId).Id;

                Operations[Operations.IndexOf(Operations.Single(id => id.Id == editedId))] = item;
                conn.Update(item);
            }
        }

        private void TextChanged(object sender, TextChangedEventArgs e) {
            SetNowaOperacjaButton();
        }

        private void ExpenseRadioButton_Checked(object sender, RoutedEventArgs e) {
            SetNowaOperacjaButton();

            CategoryValue.Items.Clear();
            foreach (OperationCategory OperationCategories_ComboBox in conn.Query<OperationCategory>("SELECT * FROM OperationCategory ORDER BY Name ASC")) {

                if (OperationCategories_ComboBox.VisibleInExpenses) {
                    CategoryValue.Items.Add(new ComboBoxItem {
                        Content = OperationCategories_ComboBox.Name
                    });
                }
            }
            SubCategoryValue.IsEnabled = false;
        }

        private void IncomeRadioButton_Checked(object sender, RoutedEventArgs e) {
            SetNowaOperacjaButton();

            CategoryValue.Items.Clear();
            foreach (OperationCategory OperationCategories_ComboBox in conn.Query<OperationCategory>("SELECT * FROM OperationCategory ORDER BY Name ASC")) {

                if (OperationCategories_ComboBox.VisibleInIncomes) {
                    CategoryValue.Items.Add(new ComboBoxItem {
                        Content = OperationCategories_ComboBox.Name
                    });
                }
            }
            SubCategoryValue.IsEnabled = false;
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetNowaOperacjaButton();

            SubCategoryValue.Items.Clear();
            if (CategoryValue.SelectedValue != null) {
                foreach (OperationSubCategory OperationSubCategories_ComboBox in conn.Query<OperationSubCategory>("SELECT * FROM OperationSubCategory ORDER BY Name ASC")) {

                    if (OperationSubCategories_ComboBox.BossCategory == ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString()) {
                        if (SubCategoryValue.Items.Count == 0)
                            SubCategoryValue.Items.Add(new ComboBoxItem {
                            Content = "Brak"
                        });
                        SubCategoryValue.Items.Add(new ComboBoxItem {
                            Content = OperationSubCategories_ComboBox.Name
                        });
                    }
                }
                if (SubCategoryValue.Items.Count == 0)
                    SubCategoryValue.IsEnabled = false;
                else
                    SubCategoryValue.IsEnabled = true;
            }
        }

        private void DateValue_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) {
            SetNowaOperacjaButton();
        }

        private void CostValue_KeyDown(object sender, KeyRoutedEventArgs e) {
            //e.Handled = !char.IsDigit((char)e.Key);

            /*
            for (int i = 0; i < 10; i++) {
                if (e.Key.ToString() == string.Format("Number{0}", i)) {
                    e.Handled = false;
                    return;
                }
            }
            if (Regex.IsMatch(CostValue.Text, @"\,\d\d")) {
                e.Handled = true;
            }*/
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
                CostValue.Text = CostValue.Text.Replace(" zł", "");
                CostValue.SelectionStart = CostValue.Text.Length;
            }
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
            isUnfocused = true;

            if(CostValue.Text != "")
                CostValue.Text = String.Format("{0:c}", decimal.Parse(CostValue.Text));
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
    }
}
