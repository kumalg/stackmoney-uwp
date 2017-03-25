using Finanse.DataAccessLayer;
using Finanse.Models.MoneyAccounts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Finanse.Models.Helpers;
using Finanse.Models.Operations;

namespace Finanse.Dialogs {

    public sealed partial class NewMoneyAccountContentDialog : ContentDialog, INotifyPropertyChanged {

        private readonly Regex regex = NewOperation.GetSignedRegex();
        private string acceptedCostValue = string.Empty;
        private bool isUnfocused = true;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<KeyValuePair<object, object>> colorBase;
        private List<KeyValuePair<object, object>> ColorBase => colorBase ??
                                                                (colorBase = ((ResourceDictionary) Application.Current.Resources["ColorBase"]).ToList());

        private SolidColorBrush Color => (SolidColorBrush)SelectedColor.Value;

        private string ColorKey => SelectedColor.Key.ToString();

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

        private readonly List<BankAccount> BankAccounts = AccountsDal.GetAllBankAccounts();
        
        private Visibility BankAccountsComboBoxVisibility => (bool)PayCardRadioButton.IsChecked ? Visibility.Visible : Visibility.Collapsed;

        private bool AnyBankAccounts => BankAccounts.Count > 0;

        private int ComboBoxSelectedIndex => BankAccounts.Count > 0 ? 0 : -1;

        private bool PrimaryButtonEnabling => !(string.IsNullOrEmpty(NameValue.Text) || Dal.AccountExistInBaseByName(NameValue.Text, TypeOfAccount()));
   
        public NewMoneyAccountContentDialog() {
            InitializeComponent();
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            AccountsDal.AddAccount(GetNewAccount());

            if (!string.IsNullOrEmpty(acceptedCostValue))
                Dal.SaveOperation(MakeOperation(AccountsDal.GetHighestIdOfAccounts()));
        }

        public Account GetNewAccount() {
            Account newAccount;
            AccountType accountType = TypeOfAccount();

            switch (accountType)
            {
                case AccountType.BankAccount:
                    newAccount = new BankAccount();
                    break;
                case AccountType.CardAccount:
                    newAccount = new CardAccount {
                        BankAccountId = ((BankAccount)BankAccountsComboBox.SelectedItem).Id, //TO DO
                    };
                    break;
                default:
                    newAccount = new CashAccount();
                    break;
            }

            newAccount.Name = NameValue.Text;
            newAccount.ColorKey = ColorKey;

            return newAccount;
        }

        private Operation MakeOperation(int moneyAccoundId) {
            var charsToRemove = new[] { "+", "-"};
            string cost = acceptedCostValue;
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

        private AccountType TypeOfAccount() {
            if ((bool)BankAccountRadioButton.IsChecked)
                return AccountType.BankAccount;
            if ((bool)PayCardRadioButton.IsChecked)
                return AccountType.CardAccount;
            return AccountType.CashAccount;
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
            isUnfocused = false;

            if (CostValue.Text == "")
                return;

            CostValue.Text = acceptedCostValue;
            CostValue.SelectionStart = CostValue.Text.Length;
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
            isUnfocused = true;

            if (CostValue.Text != "" && decimal.Parse(CostValue.Text) != 0.0m)
                CostValue.Text = NewOperation.ToCurrencyString(CostValue.Text);
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

            if (isUnfocused)
                return;

            if (regex.Match(CostValue.Text).Value != CostValue.Text) {
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
            RaisePropertyChanged("PrimaryButtonEnabling");
        }


        private void PayCardRadioButton_Click(object sender, RoutedEventArgs e) {
            if (!AnyBankAccounts)
                ShowFlyout(sender as FrameworkElement);
        }

        private static void ShowFlyout(FrameworkElement senderElement) {
            FlyoutBase.GetAttachedFlyout(senderElement).ShowAt(senderElement);
        }

        private void AccountTypeRadioButton_Click(object sender, RoutedEventArgs e) {
            RaisePropertyChanged("BankAccountsComboBoxVisibility");
            RaisePropertyChanged("PrimaryButtonEnabling");
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) {
            FrameworkElement s = sender as FrameworkElement;
            FlyoutBase.ShowAttachedFlyout(s);
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e) {
            if (BankAccountsComboBox.Items.Count > 0)
                BankAccountsComboBox.SelectedIndex = 0;
        }

        private void PayCardRadioButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
           if (!AnyBankAccounts)
                ShowFlyout(sender as FrameworkElement);
        }

        private void PayCardRadioButton_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            if (!AnyBankAccounts)
                ShowFlyout(sender as FrameworkElement);
        }
    }
    public enum AccountType {
        BankAccount, CashAccount, CardAccount
    }
}
