using System;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Finanse.Converters {
    public class ObjectToSolidColorBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            return (SolidColorBrush) value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotSupportedException();
        }
    }
}
