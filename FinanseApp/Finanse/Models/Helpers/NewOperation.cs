using System.Text.RegularExpressions;

namespace Finanse.Models.Helpers {
    internal class NewOperation {
        public static string ToCurrencyString(string value) {
            return decimal.Parse(value, Settings.ActualCultureInfo).ToString("C", Settings.ActualCultureInfo);
        }
        
        public static string ToCurrencyString(decimal value) {
            return value.ToString("C", Settings.ActualCultureInfo);
        }

        public static string ToCurrencyWithoutSymbolString(decimal value) {
            return value.ToString(Settings.ActualCultureInfo);
        }

        public static Regex GetRegex() {
            var numberFormatInfo = Settings.ActualCultureInfo.NumberFormat;
            var decimalDigits = numberFormatInfo.CurrencyDecimalDigits;
            var decimalSeparator = numberFormatInfo.CurrencyDecimalSeparator[0];

            var format = decimalDigits == 0 ?
                @"\d+" :
                @"\d+\" + decimalSeparator + @"\d{0," + decimalDigits + @"}|\d+|\" + decimalSeparator + @"\d{0," + decimalDigits + @"}";

            return new Regex(format);
        }

        public static Regex GetSignedRegex() {
            var numberFormatInfo = Settings.ActualCultureInfo.NumberFormat;
            var decimalDigits = numberFormatInfo.CurrencyDecimalDigits;
            var decimalSeparator = numberFormatInfo.CurrencyDecimalSeparator[0];

            var format = decimalDigits == 0 ?
                @"[+-]?\d+|[+-]" :
                @"[+-]?(\d+(\" + decimalSeparator + @"\d{0," + decimalDigits + @"})?)?";

            return new Regex(format);
        }
    }
}
