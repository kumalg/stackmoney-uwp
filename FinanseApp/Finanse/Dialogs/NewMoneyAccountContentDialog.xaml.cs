using Finanse.DataAccessLayer;
using Finanse.Elements;
using Finanse.Models;
using Finanse.Models.MoneyAccounts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace Finanse.Dialogs {

    public sealed partial class NewMoneyAccountContentDialog : ContentDialog, INotifyPropertyChanged {

        private Regex regex = NewOperation.getSignedRegex();
        private string acceptedCostValue = string.Empty;
        private bool isUnfocused = true;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<KeyValuePair<object, object>> colorBase;
        private List<KeyValuePair<object, object>> ColorBase {
            get {
                if (colorBase == null) {
                    colorBase = ((ResourceDictionary)Application.Current.Resources["ColorBase"]).ToList();
                }
                return colorBase;
            }
        }
        private SolidColorBrush Color {
            get {
                return (SolidColorBrush)SelectedColor.Value;
            }
        }

        private string ColorKey {
            get {
                return SelectedColor.Key.ToString();
            }
        }

        private KeyValuePair<object, object> selectedColor;

        public KeyValuePair<object, object> SelectedColor {
            get {
                if (selectedColor.Key == null || selectedColor.Value == null)
                    selectedColor = ColorBase.ElementAt(3);

                return selectedColor;
            }
            set {
                selectedColor = value;
                RaisePropertyChanged("Color");
            }
        }

        public NewMoneyAccountContentDialog() {
            this.InitializeComponent();
        }

        private void SetPrimaryButtonEnabled() {
            IsPrimaryButtonEnabled = (NameValue.Text != "");
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            /*
            Account newAccount;
            AccountType accountType = typeOfAccount();

            if (accountType == AccountType.BankAccount) {
                newAccount = new BankAccount();
            }
            else if (accountType == AccountType.CardAccount) {
                newAccount = new CardAccount {
                    BankAccountId = 1,
                };
            }
            else {
                newAccount = new CashAccount();
            }

            newAccount.Name = NameValue.Text;
            newAccount.ColorKey = ColorKey;
            */
            AccountsDal.addAccount(getNewAccount());

            if (!string.IsNullOrEmpty(acceptedCostValue))
                Dal.saveOperation(makeOperation(AccountsDal.getHighestIdOfAccounts()));
        }

        public Account getNewAccount() {
            Account newAccount;
            AccountType accountType = typeOfAccount();

            if (accountType == AccountType.BankAccount) {
                newAccount = new BankAccount();
            }
            else if (accountType == AccountType.CardAccount) {
                newAccount = new CardAccount {
                    BankAccountId = 20, //TO DO
                };
            }
            else {
                newAccount = new CashAccount();
            }

            newAccount.Name = NameValue.Text;
            newAccount.ColorKey = ColorKey;

            return newAccount;
        }

        private Operation makeOperation(int moneyAccoundId) {
            var charsToRemove = new string[] { "+", "-"};
            string cost = acceptedCostValue.ToString();
            foreach (var c in charsToRemove)
                cost = acceptedCostValue.Replace(c, string.Empty);

            return new Operation {
                Cost = decimal.Parse(cost),
                isExpense = acceptedCostValue.Contains("-"),
                MoneyAccountId = moneyAccoundId,
                Title = NameValue.Text,
                Date = DateTime.Today.ToString("yyyy.MM.dd"),
                CategoryId = 1,
                SubCategoryId = -1
            };
        }

        private AccountType typeOfAccount() {
            if ((bool)BankAccountRadioButton.IsChecked)
                return AccountType.BankAccount;
            else if ((bool)PayCardRadioButton.IsChecked)
                return AccountType.CardAccount;
            else
                return AccountType.CashAccount;
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

            if (CostValue.Text != "" && decimal.Parse(CostValue.Text) != 0.0m)
                CostValue.Text = NewOperation.toCurrencyString(CostValue.Text);
            else {
                CostValue.Text = string.Empty;
                acceptedCostValue = string.Empty;
            }
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

        private void CostValue_SelectionChanged(object sender, RoutedEventArgs e) {

        }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) {

        }

        private void NameValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {

        }

        private void BankAccountRadioButton_Click(object sender, RoutedEventArgs e) {

        }

        private void PayCardRadioButton_Click(object sender, RoutedEventArgs e) {

        }

        private void CashAccountRadioButton_Click(object sender, RoutedEventArgs e) {

        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) {
            FrameworkElement s = sender as FrameworkElement;
            Flyout.ShowAttachedFlyout(s);
        }
    }
    public enum AccountType {
        BankAccount, CashAccount, CardAccount
    }
}
