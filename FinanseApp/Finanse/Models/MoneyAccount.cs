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

        public string GetActualMoneyValue() => Dal.GetAllOperationsOfThisMoneyAccount(this)
            .Sum(i => i.SignedCost)
            .ToString("C", Settings.GetActualCultureInfo());
    }
}
