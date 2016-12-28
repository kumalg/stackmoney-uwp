using Finanse.DataAccessLayer;
using SQLite.Net.Attributes;
using System;
using System.Linq;

namespace Finanse.Models {
    class MoneyAccount {

        [PrimaryKey, AutoIncrement]

        public int Id { get; set; } 
        public string Name { get; set; } 
        public string Color { get; set; }

        public string getActualMoneyValue() {
            decimal moneyValue = 0;
            foreach (Operation o in Dal.GetAllOperationsByMoneyAccount(this))
                moneyValue += o.isExpense ? -o.Cost : o.Cost;
            return moneyValue.ToString("C", Settings.GetActualCurrency());
        }

        public string getInitialBalance(DateTime actualMonth) {
            decimal value = 0;
            foreach (Operation o in Dal.GetAllOperationsByMoneyAccount(this).Where(i => !i.Date.Equals("") && Convert.ToDateTime(i.Date) < actualMonth))
                value += o.isExpense ? -o.Cost : o.Cost;
            return value.ToString("C", Settings.GetActualCurrency());
        }
        public string getFinalBalance(int year, int month) {
            decimal value = 0;
            foreach (Operation o in Dal.GetAllOperationsByMoneyAccount(this))
                value += o.isExpense ? -o.Cost : o.Cost;
            return value.ToString("C", Settings.GetActualCurrency());
        }
    }
}
