using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Elements {
    public sealed partial class CategoryIcon : UserControl {

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(CategoryIcon), null);

        public string Glyph {
            get {
                return GetValue(GlyphProperty) as string;
            }

            set {
                SetValue(GlyphProperty, value);
            }
        }

        public double HalfSize => Width / 3.0;

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(CategoryIcon), null);

        public SolidColorBrush Color {

            get {
                return GetValue(ColorProperty) as SolidColorBrush;
            }

            set {
                SetValue(ColorProperty, value);
            }
        }
        /*
        private Point SPOINT {
            get {
                double angle = 60 * Math.PI / 180;
                return new Point(23 * Math.Cos(angle) + 24, 23 * Math.Sin(angle) + 24);
            }
        }

        private Point SPOINT2 {
            get {
                double angle = 30 * Math.PI / 180;
                return new Point(23 * Math.Cos(angle) + 24, 23 * Math.Sin(angle) + 24);
            }
        }*/

        public CategoryIcon() {
            InitializeComponent();
            DataContext = this;
        }
    }
}
