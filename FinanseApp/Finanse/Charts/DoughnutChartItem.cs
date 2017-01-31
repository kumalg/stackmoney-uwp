using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Finanse.Charts
{
    public class DoughnutChartItem : FrameworkElement
    {
        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Color), typeof(DoughnutChartItem), new PropertyMetadata(null, (s, a) =>
            {
                var d = s as DoughnutChartItem;
                if (d == null) return;
                if (d.Color == default(Color) && (Color)a.OldValue != default(Color))
                    d.Color = (Color)a.OldValue;
                d.Fill = new SolidColorBrush(d.Color);
            }));

        public Color Color
        {
            get { return (Color)GetValue(ColorProperty); }
            set { SetValue(ColorProperty, value); }
        }

        internal static readonly DependencyProperty FillProperty =
            DependencyProperty.Register("Fill", typeof(SolidColorBrush), typeof(DoughnutChartItem), null);

        internal SolidColorBrush Fill
        {
            get { return (SolidColorBrush)GetValue(FillProperty); }
            private set { SetValue(FillProperty, value); }
        }

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(DoughnutChartItem), new PropertyMetadata(default(double),
                (s, a) =>
                {
                    var d = s as DoughnutChartItem;
                    if (d == null) return;
                    d.Angle = 2 * Math.PI * d.Value;
                }));

        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        internal static readonly DependencyProperty AngleProperty =
            DependencyProperty.Register("Angle", typeof(double), typeof(DoughnutChartItem), null);

        internal double Angle
        {
            get { return (double)GetValue(AngleProperty); }
            private set { SetValue(AngleProperty, value); }
        }
    }
}