using Windows.UI.Xaml.Media;

namespace Finanse.Models.Statistics {
    public class ChartPart {
        public double UnrelativeValue { get; set; }
        public double RelativeValue { get; set; }
        public string Name { get; set; }
        public int Tag { get; set; }
        public SolidColorBrush SolidColorBrush { get; set; }
    }
}
