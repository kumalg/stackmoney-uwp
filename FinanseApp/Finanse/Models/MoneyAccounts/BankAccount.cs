using Finanse.DataAccessLayer;

namespace Finanse.Models.MoneyAccounts {
    public class BankAccount : Account {
        public override string ActualMoneyValue => AccountsDal.BankAccountBalanceById(Id).ToString("C", Settings.ActualCultureInfo);
    }
}
