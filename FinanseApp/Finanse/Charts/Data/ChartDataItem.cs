using Windows.UI;

namespace Finanse.Charts.Data
{
    public class ChartDataItem
    {
        public double Part { get; private set; }
        public Color Color { get; private set; }
        public string Name { get; private set; }
        public double Value { get; private set; }
        public string Unit { get; private set; }

        public ChartDataItem(double part, Color color = default(Color), string name = null, double value = default(double), string unit = null)
        {
            Part = part;
            Color = color;
            Name = name;
            Value = value;
            Unit = unit;
        }
    }
}