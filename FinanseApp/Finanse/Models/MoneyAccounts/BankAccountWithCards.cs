using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.MoneyAccounts {
    class BankAccountWithCards : BankAccount {

        public BankAccountWithCards() {}

        public override string ToString() {
            string name = base.ToString();
            foreach (var item in Cards)
                name += "\n    " + item.Name;
            return name;
        }
        
        public BankAccountWithCards(BankAccount bankAccount) {
            Id = bankAccount.Id;
            ColorKey = bankAccount.ColorKey;
            Name = bankAccount.Name;
        }

        private ObservableCollection<CardAccount> cards = new ObservableCollection<CardAccount>();
        public ObservableCollection<CardAccount> Cards {
            get {
                return cards;
            }
            set {
                cards = value;
            }
        }
    }
}
