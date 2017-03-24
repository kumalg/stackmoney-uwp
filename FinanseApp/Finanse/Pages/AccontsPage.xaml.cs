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
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;

namespace Finanse.Pages {

    public sealed partial class AccountsPage : Page {
        
        public AccountsPage() {
            this.InitializeComponent();
        }

        private ObservableCollection<Account> accounts;
        private ObservableCollection<Account> Accounts => accounts ?? (accounts = new ObservableCollection<Account>(AccountsDal.GetAllAccounts()));

        private async void NewMoneyAccount_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            var contentDialogItem = new NewMoneyAccountContentDialog();
            var result = await contentDialogItem.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            Account newAccount = contentDialogItem.GetNewAccount();
            newAccount.Id = AccountsDal.GetHighestIdOfAccounts();

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

        private async void EditButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            Account oldAccound = (Account)datacontext;
            EditMoneyAccountContentDialog editMoneyAccountContentDialog = new EditMoneyAccountContentDialog(oldAccound);
            ContentDialogResult result = await editMoneyAccountContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            Account editedAccound = editMoneyAccountContentDialog.EditedAccount;
            int index = Accounts.IndexOf(oldAccound);
            Accounts.Remove(oldAccound);
            Accounts.Insert(index, editedAccound);
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            if (AccountsDal.CountBankAccouns() + AccountsDal.CountCashAccouns() > 1) {
                object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
                ShowDeleteAccountContentDialog((Account) datacontext);
            }
            else {
                MessageDialog message = new MessageDialog("To ostatnie konto. NIE USUWAJ GO ŚMIESZKU :(");
                await message.ShowAsync();
            }
        }

        private async void EditCard_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            CardAccount oldAccound = (CardAccount)datacontext;
            EditMoneyAccountContentDialog editMoneyAccountContentDialog = new EditMoneyAccountContentDialog((Account)datacontext);
            ContentDialogResult result = await editMoneyAccountContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            CardAccount editedAccound = (CardAccount)editMoneyAccountContentDialog.EditedAccount;
            BankAccountWithCards bankAccount = (BankAccountWithCards)Accounts.SingleOrDefault(i => i.Id == oldAccound.BankAccountId);
            bankAccount.Cards.Remove(oldAccound);

            bankAccount = (BankAccountWithCards)Accounts.SingleOrDefault(i => i.Id == editedAccound.BankAccountId);
            bankAccount.Cards.Insert(0, editedAccound);
        }

        private void DeleteCard_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement).DataContext;
            ShowDeleteCardContentDialog((CardAccount)datacontext);
        }

        private async void ShowDeleteAccountContentDialog(Account account) {
            string message = account is BankAccount ? 
                "Czy chcesz usunąć konto bankowe ze wszystkimi kartami? Zostaną usunięte wszystkie operacje skojażone z tym kontem." : 
                "Czy chcesz usunąć konto? Zostaną usunięte wszystkie operacje skojażone z tym kontem.";
            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog(message);
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                DeleteAccountWithOperations(account);
        }

        private async void ShowDeleteCardContentDialog(CardAccount account) {
            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć kartę płatniczą? Zostaną usunięte wszystkie operacje skojażone z tą kartą.");
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                DeleteAccountWithOperations(account);
        }

        private void DeleteAccountWithOperations(Account account) {
            if (account is CardAccount)
                DeleteCardWithOperations((CardAccount)account);
            else
                Accounts.Remove(account);

            if (account is BankAccount)
                AccountsDal.RemoveBankAccountWithCards(account.Id);
            else
                AccountsDal.RemoveSingleAccountWithOperations(account.Id);
        }
        private void DeleteCardWithOperations(CardAccount cardAccount) {
            BankAccountWithCards bankAccount = (BankAccountWithCards)Accounts.SingleOrDefault(i => i.Id == cardAccount.BankAccountId);
            bankAccount.Cards.Remove(cardAccount);
            AccountsDal.RemoveSingleAccountWithOperations(cardAccount.Id);
        }

        private void ListViewItem_Tapped(object sender, TappedRoutedEventArgs e) {
            ShowFlyout(sender as FrameworkElement);
        }

        private void ListViewItem_RightTapped(object sender, RightTappedRoutedEventArgs e) {
            ShowFlyout(sender as FrameworkElement);
        }

        private static void ShowFlyout(FrameworkElement senderElement) {
            FlyoutBase.GetAttachedFlyout(senderElement).ShowAt(senderElement);
        }
    }
}
