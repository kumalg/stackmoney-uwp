using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Finanse.Annotations;

namespace Finanse.Models.MoneyAccounts {
    class BankAccountWithCards : BankAccount, INotifyPropertyChanged {

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


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if (handler != null)
                handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
