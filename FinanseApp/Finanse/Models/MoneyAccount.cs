using Finanse.DataAccessLayer;
using SQLite.Net.Attributes;
using System;

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
    }
}
