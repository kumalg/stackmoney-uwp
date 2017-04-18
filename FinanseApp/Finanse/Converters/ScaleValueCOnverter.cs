using System;
using Windows.UI.Xaml.Data;

namespace Finanse.Converters {
    public class ScaleValueConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return (double?) value / 2;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotSupportedException();
        }
    }
}
