using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models {
    class MoneyAccountBalance {
        MoneyAccount moneyAccount;
        decimal initialValue;
        decimal finalValue;

        public MoneyAccountBalance(MoneyAccount moneyAccount, decimal initialValue, decimal finalValue) {
            this.moneyAccount = moneyAccount;
            this.initialValue = initialValue;
            this.finalValue = finalValue;
        }

        public MoneyAccount MoneyAccount {
            get {
                return moneyAccount;
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
