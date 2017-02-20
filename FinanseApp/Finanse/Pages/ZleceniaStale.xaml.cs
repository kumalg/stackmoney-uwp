using Windows.Foundation;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Finanse.Pages {

    public sealed partial class ZleceniaStale : Page {

        public ZleceniaStale() {

            this.InitializeComponent();
        }

        private PathFigureCollection points;
        private PathFigureCollection Points {
            get {
                if (points == null) {
                    points = new PathFigureCollection();
                    PathFigure pathFigure = new PathFigure() { StartPoint = new Point(50,50)};
                    PathSegment pathSegment = new BezierSegment();
                    pathFigure.Segments.Add(new BezierSegment() {
                        Point1 = new Point(60,60),
                        Point2 = new Point(70, 70),
                        Point3 = new Point(80, 50),
                    });
                    pathFigure.Segments.Add(new BezierSegment() {
                        Point1 = new Point(90, 60),
                        Point2 = new Point(100, 70),
                        Point3 = new Point(110, 50),
                    });
                    pathFigure.Segments.Add(new BezierSegment() {
                        Point1 = new Point(120, 60),
                        Point2 = new Point(130, 70),
                        Point3 = new Point(140, 50),
                    });
                    pathFigure.Segments.Add(new BezierSegment() {
                        Point1 = new Point(150, 60),
                        Point2 = new Point(160, 70),
                        Point3 = new Point(170, 50),
                    });
                    points.Add(pathFigure);
                }
                return points;
            }
        }
    }
}
