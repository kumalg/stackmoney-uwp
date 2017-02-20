using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using Finanse.Models;
using Finanse.Models.MoneyAccounts;
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

namespace Finanse.Pages {

    public sealed partial class Konta : Page {

        private ObservableCollection<Account> MoneyAccounts = new ObservableCollection<Account>(AccountsDal.getAllMoneyAccounts());

        public Konta() {

            this.InitializeComponent();
        }

        private ObservableCollection<Account> accounts;
        private ObservableCollection<Account> Accounts {
            get {
                if (accounts == null)
                    accounts = new ObservableCollection<Account>(AccountsDal.getAllAccounts());
                return accounts;
            }
        }

        private async void NewMoneyAccount_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            var ContentDialogItem = new NewMoneyAccountContentDialog();
            var result = await ContentDialogItem.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                Account newAccount = ContentDialogItem.getNewAccount();

                if (newAccount is BankAccount)
                    Accounts.Insert(0, new BankAccountWithCards((BankAccount)newAccount));
                else if (newAccount is CardAccount) {
                    CardAccount newCardAccount = (CardAccount)newAccount;
                    BankAccountWithCards bankAccount = (BankAccountWithCards)Accounts.SingleOrDefault(i => i.Id == ((CardAccount)newAccount).BankAccountId);
                    bankAccount.Cards.Insert(0, newCardAccount);
                }
                else
                    Accounts.Insert(0, newAccount);
            }
        }

        private void Grid_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e) {

        }

        private void EditButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {

        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            showDeleteAccountContentDialog((Account)datacontext);
        }
        private async void showDeleteAccountContentDialog(Account account) {

            string message = account is BankAccount ? "Czy chcesz usunąć konto bankowe ze wszystkimi kartami? Zostaną usunięte wszystkie operacje skojażone z tym kontem." : "Czy chcesz usunąć konto? Zostaną usunięte wszystkie operacje skojażone z tym kontem.";

            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog(message);

            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();

            if (result == ContentDialogResult.Primary) {
                deleteAccountWithOperations(account);
            }
        }

        private void deleteAccountWithOperations(Account account) {
            AccountsDal.removeSingleAccountWithOperations(account.Id);
            Accounts.Remove(account);
        }
    }
}
