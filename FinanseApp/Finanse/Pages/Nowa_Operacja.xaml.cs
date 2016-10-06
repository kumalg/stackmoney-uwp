using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Finanse.Pages {

    public sealed partial class Nowa_Operacja : Page {

        string acceptedCostValue = "";
        int whereIsSelection;

        bool isUnfocused = true;

        public Nowa_Operacja() {

            this.InitializeComponent();

            DateValue.MaxDate = DateTime.Today;
            DateValue.Date = DateTime.Today;

            Expense_RadioButton.IsChecked = true;

            SetCategoryComboBoxItems((bool)Expense_RadioButton.IsChecked, (bool)Income_RadioButton.IsChecked);

            foreach (MoneyAccount account in Dal.GetAllMoneyAccounts()) {

                PayFormValue.Items.Add(new ComboBoxItem {
                    Content = account.Name,
                    Tag = account.Id
                });
            }

            if (PayFormValue.Items.Count > 0)
                PayFormValue.SelectedIndex = 0;

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
                            Tag = subCatItem.OperationCategoryId
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

            var ContentDialogItem = new OperationPatternsContentDialog();

            var result = await ContentDialogItem.ShowAsync();
        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {

            SaveButton.IsEnabled = !String.IsNullOrEmpty(CostValue.Text);
        }


        private void Button_Click_1(object sender, RoutedEventArgs e) {

            int catId = 1;
            int subCategoryId = -1;

            if (CategoryValue.SelectedIndex != -1)
                catId = (int)((ComboBoxItem)CategoryValue.SelectedItem).Tag;

            if (SubCategoryValue.SelectedIndex != -1)
                subCategoryId = (int)((ComboBoxItem)SubCategoryValue.SelectedItem).Tag;

            Operation item = new Operation {
                Id = 0,
                Title = NameValue.Text,
                isExpense = (bool)Expense_RadioButton.IsChecked,
                Cost = decimal.Parse(acceptedCostValue),
                CategoryId = catId,
                SubCategoryId = subCategoryId,
                Date = String.Format("{0:yyyy/MM/dd}", DateValue.Date),
                MoreInfo = MoreInfoValue.Text,
                MoneyAccountId = (int)((ComboBoxItem)PayFormValue.SelectedItem).Tag,
            };

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

            Frame.Navigate(typeof(Strona_glowna));
        }
    }
}
