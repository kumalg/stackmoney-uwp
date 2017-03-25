using System;

namespace Finanse.Models.Helpers {
    public class DateHelper {
        public static DateTime FirstDayInMonth(DateTime date) {
            DateTime first = date.AddDays(1 - date.Day);
            return first;
        }

        public static DateTime LastDayInMonth(DateTime date) {
            int daysInMonth = DateTime.DaysInMonth(date.Year, date.Month);

            DateTime last = FirstDayInMonth(date).AddDays(daysInMonth - 1);
            return last;
        }
        
        public static string ActualTimeString => DateTime.Now.ToString("yyyy.MM.dd HH:mm:ss");
    }
}
