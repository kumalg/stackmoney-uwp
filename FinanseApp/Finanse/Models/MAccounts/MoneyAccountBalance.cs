namespace Finanse.Models.MAccounts {
    public class MoneyMAccountBalance {
        public MoneyMAccountBalance(MAccount account, decimal initialValue, decimal finalValue) {
            Account = account;
            InitialValue = initialValue;
            FinalValue = finalValue;
        }

        public void JoinBalance(MoneyMAccountBalance secondAccount) {
            FinalValue += secondAccount.FinalValue;
            InitialValue += secondAccount.InitialValue;
        }

        public void JoinBalance(decimal initialValue, decimal finalValue) {
            InitialValue += initialValue;
            FinalValue += finalValue;
        }

        public MAccount Account { get; }
        public decimal InitialValue { get; private set; }
        public decimal FinalValue { get; private set; }
    }
}
