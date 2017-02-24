using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Finanse.Charts.Data
{
    public class ChartDataItem
    {
        public double Part { get; set; }
        public SolidColorBrush Brush { get; private set; }
        public string Name { get; private set; }
        public double Value { get; private set; }
        public string Unit { get; private set; }

        public ChartDataItem(double part, SolidColorBrush brush = default(SolidColorBrush), string name = null, double value = default(double), string unit = null)
        {
            Part = part;
            Brush = brush;
            Name = name;
            Value = value;
            Unit = unit;
        }
    }
}