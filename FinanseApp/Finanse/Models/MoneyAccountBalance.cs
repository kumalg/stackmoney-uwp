using Finanse.DataAccessLayer;
using Finanse.Models.MoneyAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models {
    class MoneyAccountBalance {
        Account account;
        decimal initialValue;
        decimal finalValue;

        public MoneyAccountBalance(Account account, decimal initialValue, decimal finalValue) {
            this.account = account;
            this.initialValue = initialValue;
            this.finalValue = finalValue;
        }

        public void JoinBalance(MoneyAccountBalance secondAccount) {
            finalValue += secondAccount.finalValue;
            initialValue += secondAccount.initialValue;
        }

        public void JoinBalance(decimal initialValue, decimal finalValue) {
            this.initialValue += initialValue;
            this.finalValue += finalValue;
        }

        public Account Account {
            get {
                return account;
            }
        }

        public string InitialValue {
            get {
                return initialValue.ToString("C", Settings.getActualCultureInfo());
            }
        }

        public string FinalValue {
            get {
                return finalValue.ToString("C", Settings.getActualCultureInfo());
            }
        }
    }
}
