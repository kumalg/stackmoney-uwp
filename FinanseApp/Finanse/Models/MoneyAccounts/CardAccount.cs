using Finanse.DataAccessLayer;

namespace Finanse.Models.MoneyAccounts {
    class CardAccount : Account {
        public int BankAccountId { get; set; }

        public override string getActualMoneyValue() {
            return AccountsDal.CardAccountExpensesById(Id).ToString("C", Settings.getActualCultureInfo());
        }
    }
}
