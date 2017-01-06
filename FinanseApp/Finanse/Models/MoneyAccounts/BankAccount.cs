using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.MoneyAccounts {
    class BankAccount : Account {
        public override string getActualMoneyValue() {
            decimal moneyValue = 0;
            foreach (Operation o in Dal.getAllOperationsOfThisMoneyAccount(this))
                moneyValue += o.isExpense ? -o.Cost : o.Cost;
            return moneyValue.ToString("C", Settings.getActualCultureInfo());
        }
    }
}
