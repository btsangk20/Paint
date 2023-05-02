using ShapeableAbility;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ExtendShapes
{
    public class PTriangle : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Triangle";

        public BitmapImage Icon { get; }

        public PTriangle()
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/ExtendShapes;component/Resources/Images/triangle.png"));
        }

        public void UpdateStart(Point p)
        {
            Start = p;
        }

        public void UpdateEnd(Point p)
        {
            End = p;
        }

        public UIElement Draw(Color strokeColor, Color fillColor, int thickness, double[]? strokeDashArray = null)
        {
            var triangle = new Polygon
            {
                Stroke = new SolidColorBrush(strokeColor),
                Fill = new SolidColorBrush(fillColor),
                StrokeThickness = thickness,
            };

            if (strokeDashArray != null)
            {
                triangle.StrokeDashArray = new DoubleCollection(strokeDashArray);
            }

            var pt1 = new Point(Start.X, End.Y);
            var pt2 = new Point(End.X, End.Y);
            var pt3 = new Point(End.X - (End.X - Start.X) / 2, Start.Y);

            var points = new PointCollection { pt1, pt2, pt3 };
            triangle.Points = points;

            return triangle;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
