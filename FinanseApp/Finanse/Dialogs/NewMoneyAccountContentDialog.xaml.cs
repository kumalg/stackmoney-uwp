using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Finanse.Models;
using Finanse.Models.Helpers;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;

namespace Finanse.Dialogs {

    public sealed partial class NewMoneyAccountContentDialog : INotifyPropertyChanged {

        private readonly Regex _regex = NewOperation.GetSignedRegex();
        private string _acceptedCostValue = string.Empty;
        private bool _isUnfocused = true;
        private readonly int MaxLength = NewOperation.MaxLength;

        private TextBoxEvents _textBoxEvents = new TextBoxEvents();

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

        private readonly IEnumerable<MAccount> _bankAccounts =
            new List<MAccount> {new MAccount {Name = "Brak", Id = -1}}.AsEnumerable().Concat(MAccountsDal.GetAllAccounts());//MAccountsDal.GetAllAccounts().Concat();


        private bool PrimaryButtonEnabling => !(string.IsNullOrEmpty(NameValue.Text) || MAccountsDal.AccountExistInBaseByName(NameValue.Text));
   
        public NewMoneyAccountContentDialog() {
            InitializeComponent();
        }

        public MAccount AddedAccount;
        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            AddedAccount = MAccountsDal.AddAccount(GetNewAccount());

            if (!string.IsNullOrEmpty(_acceptedCostValue))
                Dal.SaveOperation(MakeOperation(AddedAccount.GlobalId));
        }

        public MAccount GetNewAccount() {
            MAccount newAccount;

            if (BankAccountsComboBox.SelectedIndex != -1)
                newAccount = new SubMAccount {
                    BossAccountGlobalId = ((MAccount)BankAccountsComboBox.SelectedItem)?.GlobalId, //TODO
                };
            else
                newAccount = new MAccount();
            
            newAccount.Name = NameValue.Text;
            newAccount.ColorKey = ColorKey;

            return newAccount;
        }

        private Operation MakeOperation(string moneyAccoundId) {
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
                CategoryGlobalId = CategoriesDal.GetDefaultCategory().GlobalId //TODO trzeba dać warunek żeby nie updateowało tych co nie można usuwać
            };
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
            _isUnfocused = false;

            if (string.IsNullOrEmpty(CostValue.Text))
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

        private void NameValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            RaisePropertyChanged("PrimaryButtonEnabling");
            NameValue.Foreground = NameValueForeground;
        }

        public Brush NameValueForeground => MAccountsDal.AccountExistInBaseByName(NameValue.Text)
            ? (SolidColorBrush)Application.Current.Resources["RedColorStyle"]
            : (SolidColorBrush)Application.Current.Resources["Text"];

        private void AccountTypeRadioButton_Click(object sender, RoutedEventArgs e) {
            RaisePropertyChanged("BankAccountsComboBoxVisibility");
            RaisePropertyChanged("PrimaryButtonEnabling");
        }

        private void ContentDialog_Loaded(object sender, RoutedEventArgs e) {
            if (BankAccountsComboBox.Items.Count > 0)
                BankAccountsComboBox.SelectedIndex = 0;
        }

        private void BankAccountsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) {
            if (( (ComboBox)sender ).SelectedIndex == 0)
                BankAccountsComboBox.SelectedIndex = -1;
        }
    }
}
