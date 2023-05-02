using ShapeableAbility;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BaseShapes
{
    public class PLine : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Line";

        public BitmapImage Icon { get; }

        public PLine()
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/BaseShapes;component/Resources/Images/line.png"));
        }

        public void UpdateStart(Point p)
        {
            Start = p;
        }

        public void UpdateEnd(Point p)
        {
            End = p;
        }

        public UIElement Draw(Color strokeColor, Color fillColor, int thickness, double[]? strokeDashArray)
        {
            var shape = new Line()
            {
                X1 = Start.X,
                Y1 = Start.Y,
                X2 = End.X,
                Y2 = End.Y,
                Stroke = new SolidColorBrush(strokeColor),
                Fill = new SolidColorBrush(fillColor),
                StrokeThickness = thickness
            };

            if (strokeDashArray != null)
            {
                shape.StrokeDashArray = new DoubleCollection(strokeDashArray);
            }

            return shape;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
