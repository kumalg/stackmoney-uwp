using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finanse.Models.Helpers {
    public class Date {
        public static DateTime FirstDayInMonth(DateTime date) {
            DateTime first = date.AddDays(1 - date.Day);
            return first;
        }

        public static DateTime LastDayInMonth(DateTime date) {
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            DateTime last = FirstDayInMonth(date).AddDays(daysInMonth - 1);
            return last;
        }
    }
}
