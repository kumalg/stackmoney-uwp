using System;

/*
 * http://datetimeextensions.codeplex.com/SourceControl/latest#DayExtensions.cs
 */

namespace Finanse.Models.Extensions.DateTimeExtensions {
    public static class DateExtensions {

        public static DateTime First(this DateTime current) {
            DateTime first = current.AddDays(1 - current.Day);
            return first;
        }

        public static DateTime First(this DateTime current, DayOfWeek dayOfWeek) {
            DateTime first = current.First();

            if (first.DayOfWeek != dayOfWeek)
                first = first.Next(dayOfWeek);

            return first;
        }

        public static DateTime Last(this DateTime current) {
            int daysInMonth = DateTime.DaysInMonth(current.Year, current.Month);

            DateTime last = current.First().AddDays(daysInMonth - 1);
            return last;
        }

        public static DateTime Last(this DateTime current, DayOfWeek dayOfWeek) {
            DateTime last = current.Last();

            last = last.AddDays(Math.Abs(dayOfWeek - last.DayOfWeek) * -1);
            return last;
        }

        public static DateTime Next(this DateTime current, DayOfWeek dayOfWeek) {
            int offsetDays = dayOfWeek - current.DayOfWeek;

            if (offsetDays <= 0)
                offsetDays += 7;

            DateTime result = current.AddDays(offsetDays);
            return result;
        }

        public static String GetTimestamp(this DateTime value) {
            return value.ToString("yyyyMMddHHmmssfff");
        }
    }
}
