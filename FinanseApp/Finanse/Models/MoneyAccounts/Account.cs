using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.MoneyAccounts {
    abstract class Account {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Color { get; set; }

        public abstract string getActualMoneyValue();
    }
}
