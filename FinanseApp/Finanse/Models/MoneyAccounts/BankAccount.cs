using Finanse.DataAccessLayer;

namespace Finanse.Models.MoneyAccounts {
    public class BankAccount : Account {
        public override string ActualMoneyValue => AccountsDal.BankAccountWithCardsBalanceById(Id).ToString("C", Settings.ActualCultureInfo);
    }
}
