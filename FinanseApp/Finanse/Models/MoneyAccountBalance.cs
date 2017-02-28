using Finanse.DataAccessLayer;
using Finanse.Models.MoneyAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models {
    class MoneyAccountBalance {
        decimal initialValue;
        decimal finalValue;

        public MoneyAccountBalance(Account account, decimal initialValue, decimal finalValue) {
            this.Account = account;
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

        public Account Account { get; }

        public string InitialValue => initialValue.ToString("C", Settings.ActualCultureInfo);

        public string FinalValue => finalValue.ToString("C", Settings.ActualCultureInfo);
    }
}
