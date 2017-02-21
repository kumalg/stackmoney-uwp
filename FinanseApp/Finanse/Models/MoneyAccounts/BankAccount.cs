using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.MoneyAccounts {
    public class BankAccount : Account {
        public override string getActualMoneyValue() {
            return AccountsDal.BankAccountBalanceById(Id).ToString("C", Settings.getActualCultureInfo());
        }

        public override int GetHashCode() {
            return Id.GetHashCode() & Name.GetHashCode();
        }
        public override bool Equals(object obj) {
            BankAccount secondAccount = obj as BankAccount;

            return
                secondAccount.Id == Id &&
                secondAccount.Name == Name &&
                secondAccount.ColorKey == ColorKey;
        }
    }
}
