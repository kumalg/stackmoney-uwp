using Finanse.DataAccessLayer;
using Finanse.Models.MoneyAccounts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models {
    class MoneyAccountBalance {
        public MoneyAccountBalance(Account account, decimal initialValue, decimal finalValue) {
            this.Account = account;
            this.InitialValue = initialValue;
            this.FinalValue = finalValue;
        }

        public void JoinBalance(MoneyAccountBalance secondAccount) {
            FinalValue += secondAccount.FinalValue;
            InitialValue += secondAccount.InitialValue;
        }

        public void JoinBalance(decimal initialValue, decimal finalValue) {
            this.InitialValue += initialValue;
            this.FinalValue += finalValue;
        }

        public Account Account { get; }
        public decimal InitialValue { get; private set; }
        public decimal FinalValue { get; private set; }
    }
}
