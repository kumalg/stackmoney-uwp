using Finanse.DataAccessLayer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.MoneyAccounts {
    public class BankAccount : Account {
        public override string GetActualMoneyValue() => AccountsDal.BankAccountBalanceById(Id).ToString("C", Settings.GetActualCultureInfo());
    }
}
