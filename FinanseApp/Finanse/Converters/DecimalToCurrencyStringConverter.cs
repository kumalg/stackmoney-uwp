using Finanse.Models;
using System;
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
