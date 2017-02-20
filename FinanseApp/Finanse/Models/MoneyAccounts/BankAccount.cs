using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.MoneyAccounts {
    class BankAccount : Account {
        public override string getActualMoneyValue() {
            return AccountsDal.BankAccountBalanceById(Id).ToString("C", Settings.getActualCultureInfo());
        }
    }
}
