using System;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Finanse.Converters {
    public class ColorToBrushConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (!(value is Color))
                return null;

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotSupportedException();
        }
    }
}
