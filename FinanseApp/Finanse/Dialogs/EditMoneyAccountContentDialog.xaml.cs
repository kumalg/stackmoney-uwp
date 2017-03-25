using Finanse.DataAccessLayer;
using Finanse.Models.MoneyAccounts;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Dialogs {

    public sealed partial class EditMoneyAccountContentDialog : ContentDialog, INotifyPropertyChanged {
        public Account EditedAccount { get; }
        private readonly Account accountToEdit;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        private List<KeyValuePair<object, object>> colorBase;
        private List<KeyValuePair<object, object>> ColorBase => colorBase ??
                                                                (colorBase = ((ResourceDictionary) Application.Current.Resources["ColorBase"]).ToList());

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
                EditedAccount.ColorKey = ColorKey;
                RaisePropertyChanged("PrimaryButtonEnabling");
            }
        }

        private readonly List<BankAccount> BankAccounts = AccountsDal.GetAllBankAccounts();
        
        private Visibility BankAccountsComboBoxVisibility => accountToEdit is CardAccount ? Visibility.Visible : Visibility.Collapsed;

        private BankAccount selectedBankAccount;

        public BankAccount SelectedBankAccount {
            get {
                if (selectedBankAccount == null && BankAccounts.Count > 0)
                    selectedBankAccount = BankAccounts.ElementAt(0);
                return selectedBankAccount;
            }
            set {
                selectedBankAccount = value;
                if (EditedAccount is CardAccount)
                    ((CardAccount)EditedAccount).BankAccountId = selectedBankAccount.Id;
                RaisePropertyChanged("PrimaryButtonEnabling");
            }
        }

        private bool PrimaryButtonEnabling => !(string.IsNullOrEmpty(NameValue.Text) || EditedAccount.Equals(accountToEdit));

        public EditMoneyAccountContentDialog(Account accountToEdit) {
            InitializeComponent();

            this.accountToEdit = accountToEdit;
            EditedAccount = accountToEdit.Clone();

            NameValue.Text = EditedAccount.Name;
            SelectedColor = ColorBase.SingleOrDefault(i => i.Key.ToString() == accountToEdit.ColorKey);
            if (accountToEdit is CardAccount)
                SelectedBankAccount = BankAccounts.SingleOrDefault(i => i.Id == ((CardAccount)accountToEdit).BankAccountId);
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
