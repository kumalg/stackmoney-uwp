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

    public sealed partial class EditMoneyAccountContentDialog : ContentDialog, INotifyPropertyChanged {

        Account accountToEdit;
        Account editedAccount;

        public event PropertyChangedEventHandler PropertyChanged;
        private void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public Account EditedAccount {
            get {
                return editedAccount;
            }
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

        private string ColorKey {
            get { return SelectedColor.Key.ToString(); }
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
                editedAccount.ColorKey = ColorKey;
                RaisePropertyChanged("PrimaryButtonEnabling");
            }
        }

        private List<BankAccount> BankAccounts = AccountsDal.getAllBankAccounts();
        
        private Visibility BankAccountsComboBoxVisibility {
            get {
                return accountToEdit is CardAccount ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        private BankAccount selectedBankAccount;

        public BankAccount SelectedBankAccount {
            get {
                if (selectedBankAccount == null && BankAccounts.Count > 0)
                    selectedBankAccount = BankAccounts.ElementAt(0);
                return selectedBankAccount;
            }
            set {
                selectedBankAccount = value;
                if (editedAccount is CardAccount)
                    ((CardAccount)editedAccount).BankAccountId = selectedBankAccount.Id;
                RaisePropertyChanged("PrimaryButtonEnabling");
            }
        }

        private bool PrimaryButtonEnabling {
            get {
                return !(string.IsNullOrEmpty(NameValue.Text) || editedAccount.Equals(accountToEdit));
            }
        }

        public EditMoneyAccountContentDialog(Account accountToEdit) {
            this.InitializeComponent();
            this.accountToEdit = accountToEdit;
            editedAccount = accountToEdit.Clone();

            NameValue.Text = editedAccount.Name.ToString();
            SelectedColor = ColorBase.SingleOrDefault(i => i.Key.ToString() == accountToEdit.ColorKey);
            if (accountToEdit is CardAccount)
                SelectedBankAccount = BankAccounts.SingleOrDefault(i => i.Id == ((CardAccount)accountToEdit).BankAccountId);
        }

        private void NewCategory_AddButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args) {
            AccountsDal.updateAccount(editedAccount);
        }
        
        private void NameValue_TextChanging(TextBox sender, TextBoxTextChangingEventArgs args) {
            editedAccount.Name = sender.Text;
            RaisePropertyChanged("PrimaryButtonEnabling");
        }
        
        /*
        private void ContentDialog_Loaded(object sender, RoutedEventArgs e) {
            /*
            if (BankAccountsComboBox.Items.Count > 0) {
                if (accountToEdit is CardAccount)
                    BankAccountsComboBox.SelectedItem = BankAccounts.SingleOrDefault(i => i.Id == ((CardAccount)accountToEdit).BankAccountId);
                else
                    BankAccountsComboBox.SelectedIndex = 0;
            }
        }*/
    }
}
