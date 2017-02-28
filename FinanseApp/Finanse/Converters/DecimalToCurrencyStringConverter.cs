using Finanse.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;

namespace Finanse.Converters {
    public class DecimalToCurrencyStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return value == null 
                ? null 
                : (double.Parse(value.ToString())).ToString("C", Settings.ActualCultureInfo);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotSupportedException();
        }
    }
}
