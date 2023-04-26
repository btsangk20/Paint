using PContract;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace LineAbility
{
    public class PLine : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Line";

        public void UpdateStart(Point p)
        {
            Start = p;
        }
        public void UpdateEnd(Point p)
        {
            End = p;
        }

        public UIElement Draw(Color color, int thickness, double[] strokeDashArray)
        {
            var shape = new Line()
            {
                X1 = Start.X,
                Y1 = Start.Y,
                X2 = End.X,
                Y2 = End.Y,
                Stroke = new SolidColorBrush(color),
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
