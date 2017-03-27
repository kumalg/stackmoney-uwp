using Finanse.DataAccessLayer;
using Finanse.Models.MoneyAccounts;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Dialogs {

    public sealed partial class EditMoneyAccountContentDialog : INotifyPropertyChanged {
        public Account EditedAccount { get; }
        private readonly Account _accountToEdit;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<KeyValuePair<object, object>> _colorBase;
        private List<KeyValuePair<object, object>> ColorBase => _colorBase ??
                                                                (_colorBase = ((ResourceDictionary) Application.Current.Resources["ColorBase"]).ToList());

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
                EditedAccount.ColorKey = ColorKey;
                RaisePropertyChanged("PrimaryButtonEnabling");
            }
        }

        private readonly List<BankAccount> _bankAccounts = AccountsDal.GetAllBankAccounts();
        
        private Visibility BankAccountsComboBoxVisibility => _accountToEdit is CardAccount ? Visibility.Visible : Visibility.Collapsed;

        private BankAccount _selectedBankAccount;

        public BankAccount SelectedBankAccount {
            get {
                if (_selectedBankAccount == null && _bankAccounts.Count > 0)
                    _selectedBankAccount = _bankAccounts.ElementAt(0);
                return _selectedBankAccount;
            }
            set {
                _selectedBankAccount = value;
                if (EditedAccount is CardAccount)
                    ((CardAccount)EditedAccount).BankAccountId = _selectedBankAccount.Id;
                RaisePropertyChanged("PrimaryButtonEnabling");
            }
        }

        private bool PrimaryButtonEnabling => !(string.IsNullOrEmpty(NameValue.Text) || EditedAccount.Equals(_accountToEdit));

        public EditMoneyAccountContentDialog(Account accountToEdit) {
            InitializeComponent();

            _accountToEdit = accountToEdit;
            EditedAccount = accountToEdit.Clone();

            NameValue.Text = EditedAccount.Name;
            SelectedColor = ColorBase.SingleOrDefault(i => i.Key.ToString() == accountToEdit.ColorKey);
            if (accountToEdit is CardAccount)
                SelectedBankAccount = _bankAccounts.SingleOrDefault(i => i.Id == ((CardAccount)accountToEdit).BankAccountId);
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            AccountsDal.UpdateAccount(EditedAccount);
        }
        
        private void NameValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            EditedAccount.Name = sender.Text;
            RaisePropertyChanged("PrimaryButtonEnabling");
        }
    }
}
