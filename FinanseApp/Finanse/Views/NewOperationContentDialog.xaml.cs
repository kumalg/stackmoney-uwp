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

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Views {

    public sealed partial class NewOperationContentDialog : ContentDialog {

        public ObservableCollection<Operation> Operations;

        public List<OperationCategory> OperationCategories;

        SQLite.Net.SQLiteConnection conn;

        private bool focusedCostLeftValue = true;
        private bool focusedCostRightValue = true;

        public NewOperationContentDialog(ObservableCollection<Operation> Operations, SQLite.Net.SQLiteConnection conn) {

            this.InitializeComponent();

            IsPrimaryButtonEnabled = false;

            this.Operations = Operations;
            this.conn = conn;

            DateValue.MaxDate = DateTime.Today;

            /* DODAWANIE KATEGORII DO COMBOBOX'A */
            foreach (OperationCategory OperationCategories_ComboBox in conn.Query<OperationCategory>("SELECT * FROM OperationCategory ORDER BY Name ASC")) {

                if (OperationCategories_ComboBox.VisibleInExpenses && OperationCategories_ComboBox.VisibleInIncomes) {
                    CategoryValue.Items.Add(new ComboBoxItem {
                        Content = OperationCategories_ComboBox.Name
                    });
                }
            }
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

            if (Expense_RadioButton.IsChecked == true) {

                Operations.Add(new Operation {
                    Title = NameValue.Text,
                    Cost = decimal.Parse(MixCostToString(CostLeftValue.Text, CostRightValue.Text)),
                    Category = ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString(),
                    SubCategory = subCategoryString,
                    Date = DateValue.Date,
                    ExpenseOrIncome = "expense"
                });

                conn.Insert(new Operation() {
                    Title = NameValue.Text,
                    Cost = decimal.Parse(MixCostToString(CostLeftValue.Text, CostRightValue.Text)),
                    Category = ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString(),
                    SubCategory = subCategoryString,
                    Date = DateValue.Date,
                    ExpenseOrIncome = "expense"
                });
            }

            else if (Income_RadioButton.IsChecked == true) {

                Operations.Add(new Operation {
                    Title = NameValue.Text,
                    Cost = decimal.Parse(MixCostToString(CostLeftValue.Text, CostRightValue.Text)),
                    Category = ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString(),
                    SubCategory = subCategoryString,
                    Date = DateValue.Date,
                    ExpenseOrIncome = "income"
                });

                conn.Insert(new Operation() {
                    Title = NameValue.Text,
                    Cost = decimal.Parse(MixCostToString(CostLeftValue.Text, CostRightValue.Text)),
                    Category = ((ComboBoxItem)CategoryValue.SelectedItem).Content.ToString(),
                    SubCategory = subCategoryString,
                    Date = DateValue.Date,
                    ExpenseOrIncome = "income"
                });
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
                        SubCategoryValue.Items.Add(new ComboBoxItem {
                            Content = OperationSubCategories_ComboBox.Name
                        });
                    }
                }
                if (SubCategoryValue.Items.Count == 0)
                    SubCategoryValue.IsEnabled = false;
                else {
                    SubCategoryValue.IsEnabled = true;
                    SubCategoryValue.Items.Add(new ComboBoxItem {
                        Content = "Brak"
                    });
                }
            }
        }

        private void DateValue_DateChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) {
            SetNowaOperacjaButton();
        }

        private void CostValue_KeyDown(object sender, KeyRoutedEventArgs e) {

            if (e.Key.ToString().Equals("Back")) {
                e.Handled = false;
                return;
            }
            for (int i = 0; i < 10; i++) {
                if (e.Key.ToString() == string.Format("Number{0}", i)) {
                    e.Handled = false;
                    return;
                }
            }
            e.Handled = true;
        }

        private void SetCost(object sender, TextChangedEventArgs e) {
            /*
            if (!((CostLeftValue.Text == "0" || CostLeftValue.Text == "") && (CostRightValue.Text == "" || CostRightValue.Text == "0" || CostRightValue.Text == "00")))
                CostValue.Text = MixCostToString(CostLeftValue.Text, CostRightValue.Text) + " zł";
            else {
                CostValue.Text = "";
                CostLeftValue.Text = "";
                CostRightValue.Text = "";
            }
                */

            SetNowaOperacjaButton();
        }

        public string MixCostToString(string FirstCost, string SecondCost) {
            string outputCost;

            outputCost = FirstCost;
            if (SecondCost.Length == 0)
                outputCost += ",00";
            else if (SecondCost.Length == 1) {
                outputCost += "," + SecondCost + "0";
            }
                
            else
                outputCost += "," + SecondCost;

            return outputCost;
        }


        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
            CostValue.Visibility = Visibility.Collapsed;

            CostLeftValue.Visibility = Visibility.Visible;
            DotValue.Visibility = Visibility.Visible;
            CostRightValue.Visibility = Visibility.Visible;
            CurrencyValue.Visibility = Visibility.Visible;

            CostLeftValue.Focus(FocusState.Programmatic);
        }


        private void CostLeftValue_GotFocus(object sender, RoutedEventArgs e) {
            focusedCostLeftValue = true;
        }

        private void CostRightValue_GotFocus(object sender, RoutedEventArgs e) {
            focusedCostRightValue = true;
        }


        private void CostLeftValue_LostFocus(object sender, RoutedEventArgs e) {
            focusedCostLeftValue = false;

            if (!((CostLeftValue.Text == "0" || CostLeftValue.Text == "") && (CostRightValue.Text == "" || CostRightValue.Text == "0" || CostRightValue.Text == "00")))
                CostValue.Text = MixCostToString(CostLeftValue.Text, CostRightValue.Text) + " zł";
            else {
                CostValue.Text = "";
                CostLeftValue.Text = "";
                CostRightValue.Text = "";
            }
        }

        private void CostRightValue_LostFocus(object sender, RoutedEventArgs e) {
            focusedCostRightValue = false;

            if (!(
                (CostLeftValue.Text == "0" 
                || CostLeftValue.Text == "") 
                && 
                (CostRightValue.Text == "" 
                || CostRightValue.Text == "0" 
                || CostRightValue.Text == "00"))
                ) {

                CostValue.Text = MixCostToString(CostLeftValue.Text, CostRightValue.Text) + " zł";
                if (CostRightValue.Text.Length == 1)
                    CostRightValue.Text += "0";
                }
                
            else {
                CostValue.Text = "";
                CostLeftValue.Text = "";
                CostRightValue.Text = "";
            }
        }


        private void Expense_RadioButton_GotFocus(object sender, RoutedEventArgs e) {
            if (focusedCostLeftValue == false || focusedCostRightValue == false) {
                CostValue.Visibility = Visibility.Visible;

                CostLeftValue.Visibility = Visibility.Collapsed;
                DotValue.Visibility = Visibility.Collapsed;
                CostRightValue.Visibility = Visibility.Collapsed;
                CurrencyValue.Visibility = Visibility.Collapsed;

                focusedCostLeftValue = true;
                focusedCostRightValue = true;
            }
        }

        private void DateValue_Opened(object sender, object e) {

            if (focusedCostLeftValue == false || focusedCostRightValue == false) {
                CostValue.Visibility = Visibility.Visible;

                CostLeftValue.Visibility = Visibility.Collapsed;
                DotValue.Visibility = Visibility.Collapsed;
                CostRightValue.Visibility = Visibility.Collapsed;
                CurrencyValue.Visibility = Visibility.Collapsed;

                focusedCostLeftValue = true;
                focusedCostRightValue = true;
            }
        }

        private void DateValue_Closed(object sender, object e) {
            DateValue.Focus(FocusState.Programmatic);
        }

        private void PayFormValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            SetNowaOperacjaButton();
        }

        private void SubCategoryValue_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            //ComboBox jaki = sender as ComboBox;
            /*
            if (((ComboBoxItem)jaki.SelectedItem).Content.ToString() == "Brak")
                SubCategoryValue.SelectedIndex = -1;*/
        }
    }
}
