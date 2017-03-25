using Finanse.Charts.Extensions;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Numerics;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Charts {
    public class LineChart2 : UserControl {
        public DataTemplate ItemTemplate {
            get; set;
        }


        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(IList), typeof(LineChart), new PropertyMetadata(null, OnItemsSourceChanged));

        public IList ItemsSource {
            get {
                return (IList)GetValue(ItemsSourceProperty);
            }
            set {
                SetValue(ItemsSourceProperty, value);
            }
        }

        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e) {
            var chart = d as LineChart2;
            var value = e.NewValue as IList;
            if (chart == null || value == null)
                return;
            var observable = value as INotifyCollectionChanged;
            if (observable != null)
                observable.CollectionChanged += (s, a) => chart.Redraw();
            chart.Redraw();
        }

        public static readonly DependencyProperty ThicknessProperty =
            DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(LineChart), new PropertyMetadata(3d, PropertyChangedDelegate));

        public double Thickness {
            get {
                return (double)GetValue(ThicknessProperty);
            }
            set {
                SetValue(ThicknessProperty, value);
            }
        }

        public static readonly DependencyProperty FillProperty =
            DependencyProperty.Register(nameof(Fill), typeof(SolidColorBrush), typeof(LineChart), new PropertyMetadata(new SolidColorBrush(Colors.Green), PropertyChangedDelegate));

        public SolidColorBrush Fill {
            get {
                return (SolidColorBrush)GetValue(FillProperty);
            }
            set {
                SetValue(FillProperty, value);
            }
        }

        public YMode YAxis {
            get; set;
        }

        // Root of markup
        private Grid _root;
        // X axis captions
        private Grid _xCap;
        // Y axis captions
        private Grid _yCap;
        // Canvas for line drawing
        private CanvasControl _canvas;

        public LineChart2() {
            _root = new Grid();
            _root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Auto) });
            _root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            _root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Auto) });

            _xCap = new Grid();
            _root.Children.Add(_xCap);
            Grid.SetColumn(_xCap, 1);
            Grid.SetRow(_xCap, 1);

            _yCap = new Grid { VerticalAlignment = VerticalAlignment.Stretch };
            _root.Children.Add(_yCap);

            _canvas = new CanvasControl();
            _root.Children.Add(_canvas);
            Grid.SetColumn(_canvas, 1);

            Content = _root;
            _canvas.Draw += Canvas_Draw;
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args) {
            if (ItemTemplate == null)
                return;
            if (ItemsSource == null)
                return;
            var items = ItemsSource.Select(o => {
                var item = ItemTemplate.LoadContent() as LineChartItem;
                item.DataContext = o;
                return item;
            }).ToList();
            if (!items.Any())
                return;

            var availableWidth = (float)_canvas.ActualWidth;
            var availableHeight = (float)_canvas.ActualHeight - 32;
            var fill = Fill ?? new SolidColorBrush(DefaultColors.GetRandom());
            var elementWidth = (availableWidth - (Thickness * 2)) / (items.Count - 1);
            var radius = (float)(Thickness * 2);

            var min = items.Min(i => i.Value);
            var max = items.Max(i => i.Value);
            var diff = max - min;
            var d = diff * 0.01;

            #region Add X captions
            _xCap.Children.Clear();
            _xCap.Children.Add(new TextBlock {
                Text = items.First().Key.ToString(),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch
            });
            _xCap.Children.Add(new TextBlock {
                Text = items.Last().Key.ToString(),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Stretch
            });
            #endregion
            /*
            #region Add Y captions
            _yCap.Children.Clear();
            if (YAxis == YMode.FromMin) {
                _yCap.Children.Add(new TextBlock {
                    Text = min.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom
                });
            }
            else {
                _yCap.Children.Add(new TextBlock {
                    Text = "0",
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom
                });
                _yCap.Children.Add(new TextBlock {
                    Text = min.ToString(),
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(0, 0, 0, -10 + min * availableHeight / max)
                });
            }
            _yCap.Children.Add(new TextBlock {
                Text = max.ToString(),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            });
            #endregion

            */

            IList<Vector2> array = new List<Vector2>();
         //   IList<Vector2> controlPoints = new List<Vector2>();

         //   CanvasGeometry path;

            // Draw lines
            using (var builder = new CanvasPathBuilder(sender)) {

                for (var i = 0; i < items.Count; i++) {
                    var item = items[i];
                    var x = (float)(i * (elementWidth) + Thickness);
                    var y = 16 + availableHeight -
                        (YAxis == YMode.FromMin
                            ? (float)((item.Value - min) * availableHeight / diff)
                            : (float)(item.Value * availableHeight / max));
                    // Fixes for edge points
                    if (max - item.Value < d)
                        y += radius;
                    if (item.Value - min < d)
                        y -= radius;

                    if (i == 0)
                        builder.BeginFigure(x, y);
                    else
                        builder.AddLine(x,y);

                    array.Add(new Vector2(x, y));
                }
                builder.EndFigure(CanvasFigureLoop.Open);

                args.DrawingSession.DrawLine(0, availableHeight + 16, availableWidth, availableHeight + 16, Color.FromArgb(35,255,255,255), 1);
                using (var geometry = CanvasGeometry.CreatePath(builder)) {
                    CanvasStrokeStyle strokeStyle = new CanvasStrokeStyle {
                        EndCap = CanvasCapStyle.Round,
                        StartCap = CanvasCapStyle.Round
                    };

                    Color startColor = Fill.Color;
                    Color startColor2 = Fill.Color;
                    startColor2.A = 150;


                    CanvasLinearGradientBrush ee = new CanvasLinearGradientBrush(_canvas, startColor, startColor2) {
                        StartPoint = new Vector2(0, 0),
                        EndPoint = new Vector2(0, availableHeight)
                    };

                    args.DrawingSession.DrawGeometry(geometry, ee, (float)Thickness, strokeStyle);
                }
                // Draw axis
                var color = ForegroundColor;
                //     args.DrawingSession.DrawLine(0, 0, 0, availableHeight, Colors.Red, 1);
                 //    args.DrawingSession.DrawLine(0, availableHeight + 16, availableWidth, availableHeight + 16, Colors.DarkGray, 1);
            }

            using (var builder = new CanvasPathBuilder(sender)) {
                builder.BeginFigure(array[0].X, availableHeight);
                foreach (Vector2 item in array)
                    builder.AddLine(item);
                    
                builder.AddLine(array.Last().X, availableHeight);
                builder.EndFigure(CanvasFigureLoop.Closed);

                using (var geometry = CanvasGeometry.CreatePath(builder)) {
                    Color startColor = Fill.Color;
                    startColor.A = 50;

                    CanvasLinearGradientBrush ee = new CanvasLinearGradientBrush(_canvas, startColor, Colors.Transparent) {
                        StartPoint = new Vector2(0, 0),
                        EndPoint = new Vector2(0, availableHeight)
                    };
                    args.DrawingSession.FillGeometry(geometry, ee);
                }
            }
        }

        private Vector2[] CalculateControlPoints(Vector2 previousPoint, Vector2 actualPoint, Vector2 nextPoint) {

            float xLenght = (nextPoint.X - previousPoint.X) / 4;
            float yLenght = (nextPoint.Y - previousPoint.Y) / 4;

            float x1 = actualPoint.X - xLenght;
            float x2 = actualPoint.X + xLenght;
            float y1 = actualPoint.Y - yLenght;
            float y2 = actualPoint.Y + yLenght;

            return new Vector2[] { new Vector2(x1, y1), new Vector2(x2, y2) };
        }

        private Color ForegroundColor => GetBrush("SystemControlForegroundAltMediumHighBrush").Color;

        private SolidColorBrush GetBrush(string s)
            => Application.Current.Resources[s] as SolidColorBrush;

        private void Redraw() => _canvas.Invalidate();

        private static PropertyChangedCallback PropertyChangedDelegate = (s, a) => (s as LineChart2)?.Redraw();
    }
}