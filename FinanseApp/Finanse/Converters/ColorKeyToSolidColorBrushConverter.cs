using System;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Finanse.Converters {
    class ColorKeyToSolidColorBrushConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {

            string valueString = value as string;

            return string.IsNullOrEmpty(valueString) ?
                (SolidColorBrush)Application.Current.Resources["DefaultEllipseColor"] :
                (SolidColorBrush)(((ResourceDictionary)Application.Current.Resources["ColorBase"]).FirstOrDefault(i => i.Key.Equals(valueString)).Value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotSupportedException();
        }
    }
}