using Finanse.DataAccessLayer;
using Finanse.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Finanse.Models.MAccounts;

namespace Finanse.Pages {

    public sealed partial class AccountsPage {
        
        public AccountsPage() {
            InitializeComponent();
        }

        private ObservableCollection<MAccountWithSubMAccounts> _accounts;
        private ObservableCollection<MAccountWithSubMAccounts> Accounts => _accounts ?? (_accounts = new ObservableCollection<MAccountWithSubMAccounts>(MAccountsDal.GetAllAccountsWithSubAccounts()));

        private async void NewMoneyAccount_Click(object sender, RoutedEventArgs e) {
            var contentDialogItem = new NewMoneyAccountContentDialog();
            var result = await contentDialogItem.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            MAccount newAccount = contentDialogItem.AddedAccount; //TODO Id i GlobalID bo są 0 albo null
            if (newAccount is SubMAccount) {
                MAccountWithSubMAccounts mAccountWithSubMAccounts =
                    Accounts.SingleOrDefault(i => i.MAccount.GlobalId == ((SubMAccount) newAccount).BossAccountGlobalId);
                mAccountWithSubMAccounts?.SubMAccounts.Insert(0, (SubMAccount)newAccount); //TODO wali NULLem jeżeli kontoi nie ma podkategorii
            }
            else {
                Accounts.Insert(0, new MAccountWithSubMAccounts {
                    MAccount = newAccount
                });
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            MAccountWithSubMAccounts oldAccound = ((MAccountWithSubMAccounts)datacontext);
            EditMAccountContentDialog editMoneyAccountContentDialog = new EditMAccountContentDialog(oldAccound?.MAccount);
            ContentDialogResult result = await editMoneyAccountContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;
            
            MAccount editedAccound = editMoneyAccountContentDialog.EditedAccount;
            if (editedAccound is SubMAccount)
                AccountToSubAccount(oldAccound, editedAccound as SubMAccount);
            else
                AccountToAccount(oldAccound, editedAccound);

            /*
            int index = Accounts.IndexOf(oldAccound); //TODO gdyby edytować z karty na konto i na odwró
            Accounts.Remove(oldAccound);
            oldAccound.MAccount = editedAccound;
            Accounts.Insert(index, oldAccound);*/
        }

        private void AccountToAccount(MAccountWithSubMAccounts oldAccount, MAccount newAccount) {
           // var accountWithSubMAccounts = Accounts.FirstOrDefault(i => Equals(i.MAccount, oldAccount.MAccount));
           // accountWithSubMAccounts.MAccount = newAccount;

            int index = Accounts.IndexOf(oldAccount);
            var accountWithSubMAccounts = Accounts.ElementAt(index);
            accountWithSubMAccounts.MAccount = newAccount;

            Accounts.RemoveAt(index);
            Accounts.Insert(index, accountWithSubMAccounts);
        }

        private void AccountToSubAccount(MAccountWithSubMAccounts oldAccount, SubMAccount newSubMAccount) {
            var accountWithSubMAccounts = Accounts.FirstOrDefault(i => Equals(i.MAccount, oldAccount.MAccount));
            Accounts.Remove(accountWithSubMAccounts);

            var newAccountWithSubMAccounts = Accounts.FirstOrDefault(i => i.MAccount.GlobalId == newSubMAccount.BossAccountGlobalId);
            newAccountWithSubMAccounts.SubMAccounts.Insert(0, newSubMAccount);
        }

        private void SubAccountToSubAccount(SubMAccount oldSubMAccount, SubMAccount newSubMAccount) {
            var accountWithSubMAccounts = Accounts.FirstOrDefault(i => i.SubMAccounts.Contains(oldSubMAccount));
            accountWithSubMAccounts.SubMAccounts.Remove(oldSubMAccount);
            
            var newAccountWithSubMAccounts = Accounts.FirstOrDefault(i => i.MAccount.GlobalId == newSubMAccount.BossAccountGlobalId);
            newAccountWithSubMAccounts.SubMAccounts.Insert(0, newSubMAccount);
        }

        private void SubAccountToAccount(SubMAccount oldSubMAccount, MAccount newMAccount) {
            var accountWithSubMAccounts = Accounts.FirstOrDefault(i => i.SubMAccounts.Contains(oldSubMAccount));
            accountWithSubMAccounts.SubMAccounts.Remove(oldSubMAccount);

            Accounts.Insert(0, new MAccountWithSubMAccounts{ MAccount = newMAccount});
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e) {
            //if (AccountsDal.CountBankAccouns() + AccountsDal.CountCashAccouns() > 1) {
            if (MAccountsDal.CountAccounts() > 1) {
                object datacontext = ( e.OriginalSource as FrameworkElement )?.DataContext;
                ShowDeleteAccountContentDialog(((MAccountWithSubMAccounts)datacontext)?.MAccount);
            }
            else {
                MessageDialog message = new MessageDialog("To ostatnie konto. NIE USUWAJ GO ŚMIESZKU :(");
                await message.ShowAsync();
            }
        }

        private async void EditCard_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            SubMAccount oldAccound = (SubMAccount)datacontext;
            if (oldAccound == null)
                return;

            EditMAccountContentDialog editMoneyAccountContentDialog = new EditMAccountContentDialog(oldAccound);
            ContentDialogResult result = await editMoneyAccountContentDialog.ShowAsync();

            if (result != ContentDialogResult.Primary)
                return;

            MAccount editedAccound = editMoneyAccountContentDialog.EditedAccount;

            if (editedAccound is SubMAccount)
                SubAccountToSubAccount(oldAccound, editedAccound as SubMAccount);
            else
                SubAccountToAccount(oldAccound, editedAccound);

            /*
            MAccountWithSubMAccounts bankAccount = Accounts.SingleOrDefault(i => i.MAccount.GlobalId == oldAccound.BossAccountGlobalId);
            bankAccount.SubMAccounts.Remove(oldAccound);

            bankAccount = Accounts.SingleOrDefault(i => i.MAccount.GlobalId == ((SubMAccount)editedAccound).BossAccountGlobalId);
            bankAccount.SubMAccounts.Insert(0, (SubMAccount)editedAccound);*/
        }

        private void DeleteCard_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            ShowDeleteAccountContentDialog((SubMAccount)datacontext);
        }

        private async void ShowDeleteAccountContentDialog(MAccount account) {

            string message = account is SubMAccount
                ? "Czy chcesz usunąć to konto ze wszystkimi subkontami? Zostaną usunięte wszystkie operacje skojażone z tym kontem."
                : "Czy chcesz usunąć to konto? Zostaną usunięte wszystkie operacje skojażone z tym kontem.";

            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog(message);
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                DeleteAccountWithOperations(account);
        }
        /*
        private async void ShowDeleteCardContentDialog(SubMAccount account) {
            AcceptContentDialog acceptDeleteOperationContentDialog = new AcceptContentDialog("Czy chcesz usunąć kartę płatniczą? Zostaną usunięte wszystkie operacje skojażone z tą kartą.");
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
                DeleteAccountWithOperations(account);
        }
        */
        private void DeleteAccountWithOperations(MAccount account) {
            if (account is SubMAccount)
                DeleteCardWithOperations((SubMAccount)account);
            else
                Accounts.Remove(Accounts.SingleOrDefault(i => i.MAccount.Id == account.Id));// /Remove(account);

            MAccountsDal.RemoveAccountWithSubAccountsAndOperations(account);
        }
        private void DeleteCardWithOperations(SubMAccount cardAccount) {
            MAccountWithSubMAccounts bankAccount = Accounts.SingleOrDefault(i => i.MAccount.GlobalId == cardAccount.BossAccountGlobalId);
            bankAccount.SubMAccounts.Remove(cardAccount);

            MAccountsDal.RemoveSubAccountWithOperations(cardAccount);
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

        private void NewAccountBalance_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            MAccountWithSubMAccounts accountWithSubMAccounts = ((MAccountWithSubMAccounts)datacontext);
            ShowNewBalanceContentDialog(accountWithSubMAccounts?.MAccount);
        }

        private void NewSubAccountBalance_Click(object sender, RoutedEventArgs e) {
            object datacontext = (e.OriginalSource as FrameworkElement)?.DataContext;
            SubMAccount subMAccount = (SubMAccount)datacontext;
            ShowNewBalanceContentDialog(subMAccount);
        }

        private async void ShowNewBalanceContentDialog(MAccount account) {
            BalanceAccountContentDialog acceptDeleteOperationContentDialog = new BalanceAccountContentDialog(account);
            ContentDialogResult result = await acceptDeleteOperationContentDialog.ShowAsync();
        }
    }
}
