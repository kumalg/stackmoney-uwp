using System;
using Finanse.DataAccessLayer;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.Models;
using Finanse.Models.Helpers;
using Finanse.Models.MAccounts;
using Finanse.Models.Operations;

namespace Finanse.Dialogs {

    public sealed partial class BalanceAccountContentDialog : INotifyPropertyChanged {

        private readonly Regex _regex = NewOperation.GetSignedRegex();
        private string _acceptedCostValue = string.Empty;
        private bool _isUnfocused = true;
        private readonly int MaxLength = NewOperation.MaxLengthOfValue;

        private readonly TextBoxEvents _textBoxEvents = new TextBoxEvents();

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    

     
        private bool PrimaryButtonEnabling {
            get {
                return true;
            }
        }


        private readonly MAccount _account;

        public BalanceAccountContentDialog(MAccount account) {
            InitializeComponent();
            _account = account;
            _actualMoneyValue = _account.ActualMoneyValueDecimal;

            if (account is SubMAccount) {
                ParentAccountNameTextBlock.Visibility = Visibility.Visible;
                ParentAccountNameTextBlock.Text = ((SubMAccount) account).ParentAccount.Name;
            }
        }

        private void CostValue_GotFocus(object sender, RoutedEventArgs e) {
            _isUnfocused = false;

            if (string.IsNullOrEmpty(NewBalanceValue.Text))
                return;

            NewBalanceValue.Text = _acceptedCostValue;
            NewBalanceValue.SelectionStart = NewBalanceValue.Text.Length;
        }

        private void CostValue_LostFocus(object sender, RoutedEventArgs e) {
            _isUnfocused = true;

            if (!string.IsNullOrEmpty(NewBalanceValue.Text))
                NewBalanceValue.Text = NewOperation.ToCurrencyString(NewBalanceValue.Text);
            else {
                NewBalanceValue.Text = string.Empty;
                _acceptedCostValue = string.Empty;
            }
        }

        private void CostValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            if (string.IsNullOrEmpty(NewBalanceValue.Text)) {
                _acceptedCostValue = string.Empty;
                return;
            }

            if (_isUnfocused)
                return;

            if (_regex.Match(NewBalanceValue.Text).Value != NewBalanceValue.Text) {
                int whereIsSelection = NewBalanceValue.SelectionStart;
                NewBalanceValue.Text = _acceptedCostValue;
                NewBalanceValue.SelectionStart = whereIsSelection - 1;
            }
            _acceptedCostValue = NewBalanceValue.Text;
        }

        private async void BalanceAccountContentDialog_OnPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            Dal.SaveOperation(await MakeOperationAsync());
        }

        private readonly decimal _actualMoneyValue;

        private async Task<Operation> MakeOperationAsync() {
            var newMoneyValue = decimal.Parse(_acceptedCostValue);
            var operationCost = newMoneyValue - _actualMoneyValue;

            return new Operation {
                Cost = Math.Abs(operationCost),
                isExpense = operationCost < 0,
                MoneyAccountId = _account.GlobalId,
                Title = "Aktualizacja salda",
                Date = DateTime.Today.ToString("yyyy.MM.dd"),
                CategoryGlobalId = (await CategoriesDal.GetDefaultCategoryAsync()).GlobalId //TODO trzeba dać warunek żeby nie updateowało tych co nie można usuwać
            };
        }
    }
}
