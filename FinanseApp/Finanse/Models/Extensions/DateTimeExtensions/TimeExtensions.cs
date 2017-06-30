using System;

namespace Finanse.Models.Extensions.DateTimeExtensions {
    public static class TimeExtensions {
        public static DateTime Midnight(this DateTime current) {
            DateTime midnight = new DateTime(current.Year, current.Month, current.Day);
            return midnight;
        }

        public static DateTime Noon(this DateTime current) {
            DateTime noon = new DateTime(current.Year, current.Month, current.Day, 12, 0, 0);
            return noon;
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute) {
            return SetTime(current, hour, minute, 0, 0);
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute, int second) {
            return SetTime(current, hour, minute, second, 0);
        }

        public static DateTime SetTime(this DateTime current, int hour, int minute, int second, int millisecond) {
            DateTime atTime = new DateTime(current.Year, current.Month, current.Day, hour, minute, second, millisecond);
            return atTime;
        }
    }
}
