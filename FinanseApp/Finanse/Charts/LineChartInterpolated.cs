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
    public class LineChartInterpolated : UserControl {
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
            var chart = d as LineChartInterpolated;
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
        private readonly Grid _root;
        // X axis captions
        private readonly Grid _xCap;
        // Y axis captions
        private readonly Grid _yCap;
        // Canvas for line drawing
        private readonly CanvasControl _canvas;

        public LineChartInterpolated() {
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
            var items = ItemsSource.Cast<LineChartItem>().ToList();

            if (!items.Any())
                return;

            var availableWidth = (float)_canvas.ActualWidth;
            var availableHeight = (float)_canvas.ActualHeight;
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
              //  Text = items.First().Key.ToString(),
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Stretch
            });
            _xCap.Children.Add(new TextBlock {
              //  Text = items.Last().Key.ToString(),
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
            IList<Vector2> controlPoints = new List<Vector2>();

            for (var i = 0; i < items.Count; i++) {
                var item = items[i];
                var x = (float)(i * (elementWidth) + Thickness);
                var y = availableHeight -
                        (YAxis == YMode.FromMin
                            ? (float)((item.Value - min) * availableHeight / diff)
                            : (float)(item.Value * availableHeight / max));
                // Fixes for edge points
                if (max - item.Value < d)
                    y += radius;
                if (item.Value - min < d)
                    y -= radius;

                array.Add(new Vector2(x, y));
            }

            for (int i = 1; i < array.Count - 1; i++) {
                Vector2[] controlPoint2 = CalculateControlPoints(array[i - 1], array[i], array[i + 1]);
                controlPoints.Add(controlPoint2[0]);
                controlPoints.Add(controlPoint2[1]);
            }

            // Fill area
            using (var fillBuilder = new CanvasPathBuilder(sender)) {
                // Draw lines
                using (var drawBuilder = new CanvasPathBuilder(sender)) {

                    for (int i = 0; i < array.Count; i++) {
                        if (i == 0) {
                            drawBuilder.BeginFigure(array[i]);
                            fillBuilder.BeginFigure(array[i]);
                        }
                        else if (i == 1 && array.Count == 2) {
                            drawBuilder.AddCubicBezier(array[0], array[1], array[1]);
                            fillBuilder.AddCubicBezier(array[0], array[1], array[1]);
                        }
                        else if (i == 1 && array.Count > 2) {
                            drawBuilder.AddCubicBezier(array[0], controlPoints[0], array[1]);
                            fillBuilder.AddCubicBezier(array[0], controlPoints[0], array[1]);
                        }
                        else if (i < array.Count - 1) {
                            drawBuilder.AddCubicBezier(controlPoints[i * 2 - 3], controlPoints[i * 2 - 2], array[i]);
                            fillBuilder.AddCubicBezier(controlPoints[i * 2 - 3], controlPoints[i * 2 - 2], array[i]);
                        }
                        else if (array.Count > 1) {
                            drawBuilder.AddCubicBezier(controlPoints[i * 2 - 3], array[i], array[i]);
                            fillBuilder.AddCubicBezier(controlPoints[i * 2 - 3], array[i], array[i]);
                        }
                    }

                    drawBuilder.EndFigure(CanvasFigureLoop.Open);
                    using (var geometry = CanvasGeometry.CreatePath(drawBuilder)) {
                        CanvasStrokeStyle strokeStyle = new CanvasStrokeStyle {
                            EndCap = CanvasCapStyle.Round,
                            StartCap = CanvasCapStyle.Round
                        };

                        Color startColor = fill.Color;
                        Color startColor2 = fill.Color;
                        startColor2.A = 150;
                        
                        CanvasLinearGradientBrush ee = new CanvasLinearGradientBrush(_canvas, startColor, startColor2) {
                            StartPoint = new Vector2(0, 0),
                            EndPoint = new Vector2(0, availableHeight)
                        };

                        args.DrawingSession.DrawGeometry(geometry, ee, (float)Thickness, strokeStyle);
                    }
                    
                    fillBuilder.AddLine(array.Last().X, availableHeight);
                    fillBuilder.AddLine(array[0].X, availableHeight);
                    fillBuilder.EndFigure(CanvasFigureLoop.Closed);
                    using (var geometry = CanvasGeometry.CreatePath(fillBuilder)) {
                        Color startColor = fill.Color;
                        startColor.A = 50;

                        CanvasLinearGradientBrush ee = new CanvasLinearGradientBrush(_canvas, startColor, Colors.Transparent) {
                            StartPoint = new Vector2(0, 0),
                            EndPoint = new Vector2(0, availableHeight)
                        };
                        args.DrawingSession.FillGeometry(geometry, ee);
                    }
                }
            }


            args.DrawingSession.DrawLine(
                0, 
                availableHeight, 
                availableWidth, 
                availableHeight + 16,
                Color.FromArgb(35, 255, 255, 255), 0.5f);

            // Draw axis
            Color color = ((SolidColorBrush)Foreground).Color;
            color.A = 40;

            CanvasStrokeStyle strokeStyle2 = new CanvasStrokeStyle {
                CustomDashStyle = new float[] { 2, 4 },
            };

            var part = (availableHeight - 2 * radius) / 3;

            for (int i = 0; i < 4; i++) {
                var yPos = radius + i * part;
                args.DrawingSession.DrawLine(0, yPos, availableWidth, yPos, color, 1, strokeStyle2);
            }
        }

        private Vector2[] CalculateControlPoints(Vector2 previousPoint, Vector2 actualPoint, Vector2 nextPoint) {

            var availableHeight = (float)_canvas.ActualHeight;

            var xLenght = (nextPoint.X - previousPoint.X) / 4;
            var yLenght = (nextPoint.Y - previousPoint.Y) / 4;

            var x1 = actualPoint.X - xLenght;
            var x2 = actualPoint.X + xLenght;
            var y1 = actualPoint.Y - yLenght;
            var y2 = actualPoint.Y + yLenght;
            
            if (y1 > availableHeight) {
                y2 += y1 - availableHeight;
                y1 = availableHeight - (float)(Thickness * 2);
            }

            else if (y2 > availableHeight) {
                y1 += y2 - availableHeight;
                y2 = availableHeight - (float)(Thickness * 2);
            }

            return new[] { new Vector2(x1, y1), new Vector2(x2, y2) };
        }

        private Color ForegroundColor => GetBrush("SystemControlForegroundAltMediumHighBrush").Color;

        private static SolidColorBrush GetBrush(string s) => Application.Current.Resources[s] as SolidColorBrush;

        private void Redraw() => _canvas.Invalidate();

        private static readonly PropertyChangedCallback PropertyChangedDelegate = (s, a) => (s as LineChartInterpolated)?.Redraw();
    }
}