using System;
using Windows.UI.Xaml.Data;

namespace Finanse.Converters {
    public class ExpenseToggleEnablingByBossCategoryIdConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, string language) {
            if (value == null)
                return "false";

            return "false";//Dal.getCategoryById((int)value).VisibleInExpenses;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language) {
            throw new NotSupportedException();
        }
    }
}
