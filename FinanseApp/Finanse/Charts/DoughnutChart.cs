using System;
using System.Collections;
using System.Collections.Specialized;
using System.Numerics;
using System.Linq;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Finanse.Charts.Data;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace Finanse.Charts {
    public class DoughnutChart : UserControl {
        public DataTemplate ItemTemplate {
            get; set;
        }

        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(DoughnutChart), new PropertyMetadata(null, OnItemsSourceChanged));

        public IList ItemsSource {
            get {
                return (IList)GetValue(ItemsSourceProperty);
            }
            set {
                SetValue(ItemsSourceProperty, value);
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var chart = d as DoughnutChart;
            var value = e.NewValue as IList;
            if (chart == null || value == null)
                return;
            var observable = value as INotifyCollectionChanged;
            if (observable != null)
                observable.CollectionChanged += (s, a) => chart.Redraw();
            chart.Redraw();
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(DoughnutChart), new PropertyMetadata(10d, PropertyChangedDelegate));

        public double Thickness {
            get {
                return (double)GetValue(ThicknessProperty);
            }
            set {
                SetValue(ThicknessProperty, value);
            }
        }

        public static readonly DependencyProperty DistanceProperty =
            DependencyProperty.Register(nameof(Distance), typeof(double), typeof(DoughnutChart), new PropertyMetadata(5d, PropertyChangedDelegate));

        public double Distance {
            get {
                return (double)GetValue(DistanceProperty);
            }
            set {
                SetValue(DistanceProperty, value);
            }
        }

        private readonly Grid _root;
        private readonly CanvasControl canvas;

        public DoughnutChart() {
            _root = new Grid();
            Content = _root;
            canvas = new CanvasControl();
            _root.Children.Add(canvas);
            canvas.Draw += Canvas_Draw;
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args) {
            if (ItemTemplate == null)
                return;
            if (ItemsSource == null)
                return;


            var repairedItemsSource = ItemsSource.Cast<ChartDataItem>().Where(item => item.Part > 0.01).ToList();
            var newSum = repairedItemsSource.Sum(i => i.Value);
            foreach (var item in repairedItemsSource)
                item.Part = item.Value / newSum;


            var d = 0d;
            foreach (var item in repairedItemsSource) {
                 var chartItem = (DoughnutChartItem)ItemTemplate.LoadContent();
                 if (chartItem == null)
                     return;

                chartItem.DataContext = item;

                if (chartItem.Color == default(Color))
                    chartItem.Color = DefaultColors.GetRandom();

                var sweepAngle = chartItem.Angle;


                var startAngle = (float)(d - Math.PI / 2);
                d += sweepAngle;

                var center = new Vector2((float)(ActualWidth / 2), (float)(ActualHeight / 2));
                var radius = center.X > center.Y
                    ? new Vector2(center.Y)
                    : new Vector2(center.X);
                var startPoint = center + Vector2.Transform(Vector2.UnitX, Matrix3x2.CreateRotation(startAngle)) * radius;
                
                var relativeDistance = Distance / radius.X;
                var relativeSecondDistance = Distance / (radius.X - Thickness / 2);
                var repairSweepAngle = sweepAngle > relativeDistance ? sweepAngle - relativeDistance : 0;
                var repairSecondSweepAngle = sweepAngle > relativeSecondDistance / 2 ? sweepAngle - relativeSecondDistance / 2 : 0;

                using (var builder = new CanvasPathBuilder(canvas)) {
                    builder.BeginFigure(startPoint);
                    builder.AddArc(center, radius.X, radius.Y, (float)(startAngle + relativeDistance / 2), (float)repairSweepAngle);
                    builder.AddArc(center, radius.X - (float)Thickness / 2, (float)(radius.Y - Thickness / 2), (float)(startAngle + sweepAngle - relativeSecondDistance / 2), -(float)repairSecondSweepAngle);
                    builder.EndFigure(CanvasFigureLoop.Closed);

                    args.DrawingSession.FillGeometry(
                        CanvasGeometry.CreatePath(builder), 
                        chartItem.Color);
                }
            }
        }

        private void Redraw() => canvas.Invalidate();

        private static readonly PropertyChangedCallback PropertyChangedDelegate = (s, a) => (s as DoughnutChart)?.Redraw();
        
    }
}