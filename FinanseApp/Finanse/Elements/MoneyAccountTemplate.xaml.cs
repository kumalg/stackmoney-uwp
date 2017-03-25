using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class MoneyAccountTemplate : UserControl {

        private Account Account => DataContext as Account;

        public MoneyAccountTemplate() {

            this.InitializeComponent();
            this.DataContextChanged += (s, e) => Bindings.Update();
        }

        private Visibility isBankAccount => DataContext is BankAccountWithCards ? Visibility.Visible : Visibility.Collapsed;

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
