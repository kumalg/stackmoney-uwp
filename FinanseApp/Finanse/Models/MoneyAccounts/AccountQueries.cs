namespace Finanse.Models.MoneyAccounts {
    public class AccountQueries {
        private static string SeqTrigger(string tableName) {
            return "CREATE TRIGGER IF NOT EXISTS updateAccountsSeq" + tableName + " " +
                   "BEFORE INSERT ON " + tableName + " " +
                   "BEGIN " +
                   " UPDATE sqlite_sequence SET seq = seq + 1 WHERE name = 'Account'; " +
                   "END;";
        }

        public static string SeqTriggerCashAccount = SeqTrigger("CashAccount");
        public static string SeqTriggerBankAccount = SeqTrigger("BankAccount");
        public static string SeqTriggerCardAccount = SeqTrigger("CardAccount");
    }
}
