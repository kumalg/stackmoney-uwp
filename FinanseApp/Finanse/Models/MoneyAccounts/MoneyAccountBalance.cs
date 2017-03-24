namespace Finanse.Models.MoneyAccounts {
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
