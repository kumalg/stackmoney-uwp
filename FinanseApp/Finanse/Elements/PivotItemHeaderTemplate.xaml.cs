using Windows.UI.Xaml;

namespace Finanse.Elements {

    public sealed partial class PivotItemHeaderTemplate {

        public static readonly DependencyProperty GlyphProperty = DependencyProperty.Register("Glyph", typeof(string), typeof(PivotItemHeaderTemplate), null);

        public string Glyph {
            get { return GetValue(GlyphProperty) as string; }
            set { SetValue(GlyphProperty, value); }
        }

        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(PivotItemHeaderTemplate), null);

        public string Label {
            get { return GetValue(LabelProperty) as string; }
            set { SetValue(LabelProperty, value); }
        }


        public PivotItemHeaderTemplate() {
            InitializeComponent();
            DataContext = this;
        }
    }
}
