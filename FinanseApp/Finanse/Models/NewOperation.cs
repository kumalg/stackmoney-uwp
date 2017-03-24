using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Finanse.Models {
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
            NumberFormatInfo numberFormatInfo = Settings.ActualCultureInfo.NumberFormat;
            int decimalDigits = numberFormatInfo.CurrencyDecimalDigits;
            char decimalSeparator = numberFormatInfo.CurrencyDecimalSeparator[0];

            string format = decimalDigits == 0 ?
                @"\d+" :
                @"\d+\" + decimalSeparator + @"\d{0," + decimalDigits + @"}|\d+|\" + decimalSeparator + @"\d{0," + decimalDigits + @"}";

            return new Regex(format);
        }

        public static Regex GetSignedRegex() {
            NumberFormatInfo numberFormatInfo = Settings.ActualCultureInfo.NumberFormat;
            int decimalDigits = numberFormatInfo.CurrencyDecimalDigits;
            char decimalSeparator = numberFormatInfo.CurrencyDecimalSeparator[0];

            string format = decimalDigits == 0 ?
                @"[+-]?\d+|[+-]" :
                @"[+-]?(\d+(\" + decimalSeparator + @"\d{0," + decimalDigits + @"})?)?";
            /*
             * \((\d+\,\d{0,2}|\d+|\,\d{0,2})\)?|\(|(\d+\,\d{0,2}|\d+|\,\d{0,2})
             * 
            string format2 = decimalDigits == 0 ?
                @"[+-]?\d+|[+-]" :
                @"\(?(\d+\" + decimalSeparator + @"\d{0," + decimalDigits + @"}|\d+|\" + decimalSeparator + @"\d{0," + decimalDigits + @"})|[+-]";
                */
            return new Regex(format);
        }
    }
}
