using Finanse.Models.MAccounts;
using Finanse.Models.MoneyAccounts;

namespace Finanse.Elements {

    public sealed partial class CardAccountTemplate {

        private SubMAccount SubMAccount => DataContext as SubMAccount;

        public CardAccountTemplate() {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }
    }
}