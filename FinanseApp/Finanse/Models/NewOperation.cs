using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models {
    class NewOperation {
        public static string valueToCurrencyString(string value) {
            return Decimal.Parse(value/*, new CultureInfo("pl-PL")*/).ToString("C", Settings.GetActualCurrency());
        }
    }
}
