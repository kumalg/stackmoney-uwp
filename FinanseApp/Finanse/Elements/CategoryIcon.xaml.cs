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

        public double HalfSize {
            get {
                return Width / 3.0;
            }
        }

        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(SolidColorBrush), typeof(CategoryIcon), null);

        public SolidColorBrush Color {

            get {
                return GetValue(ColorProperty) as SolidColorBrush;
            }

            set {
                SetValue(ColorProperty, value);
            }
        }

        public CategoryIcon() {
            this.InitializeComponent();
            this.DataContext = this;
        }
    }
}
