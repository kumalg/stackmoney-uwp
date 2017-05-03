using Finanse.Models.MAccounts;

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
