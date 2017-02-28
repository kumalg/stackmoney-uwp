using System.Collections.ObjectModel;
using System.Linq;

namespace Finanse.Models.MoneyAccounts {
    class BankAccountWithCards : BankAccount {

        public BankAccountWithCards() {}

        public override string ToString() {
            var name = base.ToString();
            return Cards.Aggregate(name, (current, item) => current + ("\n    " + item.Name));
        }
        
        public BankAccountWithCards(Account bankAccount) {
            Id = bankAccount.Id;
            ColorKey = bankAccount.ColorKey;
            Name = bankAccount.Name;
        }

        public ObservableCollection<CardAccount> Cards { get; set; } = new ObservableCollection<CardAccount>();
    }
}
