using Finanse.Models.MAccounts;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class MoneyAccountTemplate {

        private MAccount Account => DataContext as MAccount;

        public MoneyAccountTemplate() {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }

        private string Glyph {
            get {
                /*
                if (DataContext is CashAccount)
                    return "";
                if (DataContext is BankAccountWithCards)
                    return "";
                */return "";
            }
        }
    }
}
