using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class CardAccountTemplate {

        private CardAccount CardAccount => DataContext as CardAccount;

        public CardAccountTemplate() {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}