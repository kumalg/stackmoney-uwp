using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class MoneyAccountTemplate {

        private Account Account => DataContext as Account;

        public MoneyAccountTemplate() {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }

        private string Glyph {
            get {
                if (DataContext is CashAccount)
                    return "";
                if (DataContext is BankAccountWithCards)
                    return "";
                return "";
            }
        }
    }
}
