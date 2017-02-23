using Finanse.DataAccessLayer;

namespace Finanse.Models.MoneyAccounts {
    class CardAccount : Account {
        public int BankAccountId { get; set; }

        public override string GetActualMoneyValue() {
            return AccountsDal.CardAccountExpensesById(Id).ToString("C", Settings.GetActualCultureInfo());
        }

        public override int GetHashCode() {
            return Name.GetHashCode() * Id;
        }
        public override bool Equals(object o) {
            if (!(o is CardAccount))
                return false;

            return
                base.Equals((Account)o) &&
                ((CardAccount)o).BankAccountId == BankAccountId;
        }
    }
}
