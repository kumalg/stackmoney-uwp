using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Finanse.DataAccessLayer;
using Finanse.Models;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class MoneyAccountTemplate : UserControl {

        private Models.MoneyAccounts.Account Account {

            get {
                return this.DataContext as Account;
            }
        }

        public MoneyAccountTemplate() {

            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        private Visibility isBankAccount {
            get {
                return DataContext is BankAccountWithCards ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private string Glyph {
            get {
                if (DataContext is CashAccount)
                    return "";
                else if (DataContext is BankAccountWithCards)
                    return "";
                else
                    return "";
            }
        }
    }
}
