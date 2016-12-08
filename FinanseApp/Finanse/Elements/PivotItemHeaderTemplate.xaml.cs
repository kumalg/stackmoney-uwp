using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Finanse.Elements {

    public sealed partial class PivotItemHeaderTemplate : UserControl {

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(PivotItemHeaderTemplate), null);

        public string Glyph {
            get {
                return GetValue(GlyphProperty) as string;
            }
            set {
                SetValue(GlyphProperty, value);
            }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(PivotItemHeaderTemplate), null);

        public string Label {
            get {
                return GetValue(LabelProperty) as string;
            }
            set {
                SetValue(LabelProperty, value);
            }
        }


        public PivotItemHeaderTemplate() {
            this.InitializeComponent();
            this.DataContext = this;
        }
    }
}
