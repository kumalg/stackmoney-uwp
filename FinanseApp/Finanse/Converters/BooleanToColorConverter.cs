using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Finanse.Converters {
    public class BooleanToColorConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value == null)
                return null;

            return (bool)value ? (SolidColorBrush)Application.Current.Resources["RedColorStyle"] : (SolidColorBrush)Application.Current.Resources["GreenColorStyle"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotSupportedException();
        }
    }
}
