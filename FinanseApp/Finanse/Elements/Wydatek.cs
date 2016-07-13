using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Elements {

    public class Wydatek {

        public string Title;
        public string Category;
        public string CostString;
        public decimal Cost;
        public DateTimeOffset? Date;
        public string ExpenseOrIncome;
    }
}
