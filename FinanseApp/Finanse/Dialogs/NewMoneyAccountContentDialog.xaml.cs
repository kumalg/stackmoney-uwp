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

    public sealed partial class NewMoneyAccountContentDialog : INotifyPropertyChanged {

        private readonly Regex _regex = NewOperation.GetSignedRegex();
        private string _acceptedCostValue = string.Empty;
        private bool _isUnfocused = true;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }


        private List<KeyValuePair<object, object>> _colorBase;
        private List<KeyValuePair<object, object>> ColorBase => _colorBase ??
                                                                (_colorBase = ((ResourceDictionary) Application.Current.Resources["ColorBase"]).ToList());

        private SolidColorBrush Color => (SolidColorBrush)SelectedColor.Value;

        private string ColorKey => SelectedColor.Key.ToString();

        private KeyValuePair<object, object> _selectedColor;

        public KeyValuePair<object, object> SelectedColor {
            get {
                if (_selectedColor.Key == null || _selectedColor.Value == null)
                    _selectedColor = ColorBase.ElementAt(3);

                return _selectedColor;
            }
            set {
                _selectedColor = value;
                RaisePropertyChanged("Color");
            }
        }

        private readonly List<BankAccount> _bankAccounts = AccountsDal.GetAllBankAccounts();
        
        private Visibility BankAccountsComboBoxVisibility => (bool)PayCardRadioButton.IsChecked ? Visibility.Visible : Visibility.Collapsed;

        private bool AnyBankAccounts => _bankAccounts.Count > 0;

        private int ComboBoxSelectedIndex => _bankAccounts.Count > 0 ? 0 : -1;

        private bool PrimaryButtonEnabling => !(string.IsNullOrEmpty(NameValue.Text) || Dal.AccountExistInBaseByName(NameValue.Text, TypeOfAccount()));
   
        public NewMoneyAccountContentDialog() {
            InitializeComponent();
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            AccountsDal.AddAccount(GetNewAccount());

            if (!string.IsNullOrEmpty(_acceptedCostValue))
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
            string cost = _acceptedCostValue;
            foreach (var c in charsToRemove)
                cost = _acceptedCostValue.Replace(c, string.Empty);

            return new Operation {
                Cost = decimal.Parse(cost),
                isExpense = _acceptedCostValue.Contains("-"),
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
            _isUnfocused = false;

            if (CostValue.Text == "")
                return;

            CostValue.Text = _acceptedCostValue;
            CostValue.SelectionStart = CostValue.Text.Length;
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
            _isUnfocused = true;

            if (CostValue.Text != "" && decimal.Parse(CostValue.Text) != 0.0m)
                CostValue.Text = NewOperation.ToCurrencyString(CostValue.Text);
            else {
                CostValue.Text = string.Empty;
                _acceptedCostValue = string.Empty;
            }
        }

        private void CostValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            if (string.IsNullOrEmpty(CostValue.Text)) {
                _acceptedCostValue = string.Empty;
                return;
            }

            if (_isUnfocused)
                return;

            if (_regex.Match(CostValue.Text).Value != CostValue.Text) {
                int whereIsSelection = CostValue.SelectionStart;
                CostValue.Text = _acceptedCostValue;
                CostValue.SelectionStart = whereIsSelection - 1;
            }
            _acceptedCostValue = CostValue.Text;
        }

        private void CostValue_SelectionChanged(object sender, RoutedEventArgs e) { }

        private void CostValue_TextChanged(object sender, TextChangedEventArgs e) { }

        private void NameValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) => RaisePropertyChanged("PrimaryButtonEnabling");


        private void PayCardRadioButton_Click(object sender, RoutedEventArgs e) {
            if (!AnyBankAccounts)
                Flyouts.ShowFlyoutBase(sender);
        }

        private void AccountTypeRadioButton_Click(object sender, RoutedEventArgs e) {
            RaisePropertyChanged("BankAccountsComboBoxVisibility");
            RaisePropertyChanged("PrimaryButtonEnabling");
        }

        private void HyperlinkButton_Click(object sender, RoutedEventArgs e) => Flyouts.ShowFlyoutBase(sender);

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e) {
            if (BankAccountsComboBox.Items.Count > 0)
                BankAccountsComboBox.SelectedIndex = 0;
        }

        private void PayCardRadioButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e) {
           if (!AnyBankAccounts)
                Flyouts.ShowFlyoutBase(sender);
        }

        private void PayCardRadioButton_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e) {
            if (!AnyBankAccounts)
                Flyouts.ShowFlyoutBase(sender);
        }
    }
    public enum AccountType {
        BankAccount, CashAccount, CardAccount
    }
}
