using System;
using Windows.UI.Xaml.Data;

namespace Finanse.Converters {
    public class DoubleToPercentStringConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value == null)
                return null;

            double doubleValue = double.Parse(value.ToString()) * 100;

            string stringValue = doubleValue > 0.1 ?
                    doubleValue.ToString("0.##") : "<0.01";

            return stringValue + "%";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotSupportedException();
        }
    }
}
