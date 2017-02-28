using Finanse.DataAccessLayer;

namespace Finanse.Models.MoneyAccounts {
    class CashAccount : Account {
        public override string ActualMoneyValue => AccountsDal.CashAccountBalanceById(Id).ToString("C", Settings.ActualCultureInfo);
    }
}
