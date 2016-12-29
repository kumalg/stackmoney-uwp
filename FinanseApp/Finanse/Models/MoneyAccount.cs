using Finanse.DataAccessLayer;
using SQLite.Net.Attributes;
using System;
using System.Linq;

namespace Finanse.Models {
    class MoneyAccount {

        [PrimaryKey, AutoIncrement]

        public int Id {
            get; set;
        }
        public string Name {
            get; set;
        }
        public string Color {
            get; set;
        }

        private DateTime _actualYearAndMonth = DateTime.Today;

        public DateTime actualYearAndMonth {
            set {
                _actualYearAndMonth = value;
            }
        }

        private decimal[] balance = null;

        public string getActualMoneyValue() {
            decimal moneyValue = 0;
            foreach (Operation o in Dal.GetAllOperationsByMoneyAccount(this))
                moneyValue += o.isExpense ? -o.Cost : o.Cost;
            return moneyValue.ToString("C", Settings.GetActualCurrency());
        }

        public string getInitialBalance() {
            if (balance == null)
                balance = Dal.getBalanceFromSingleAccountToDate(_actualYearAndMonth, this.Id);

            return balance.ElementAt(0).ToString("C", Settings.GetActualCurrency());
            //return ((decimal)9).ToString("C", Settings.GetActualCurrency());
        }
        public string getFinalBalance() {
            if (balance == null)
                balance = Dal.getBalanceFromSingleAccountToDate(_actualYearAndMonth, this.Id);

            return balance.ElementAt(1).ToString("C", Settings.GetActualCurrency());
        }
    }
}
