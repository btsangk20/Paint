using PContract;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RectangleAbility
{
    public class PRectangle : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Rectangle";

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
            double width = Math.Abs(End.X - Start.X);
            double height = Math.Abs(End.Y - Start.Y);

            var shape = new Rectangle()
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(color),
                StrokeThickness = thickness,
                RenderTransform = new ScaleTransform()
                {
                    ScaleX = End.X > Start.X ? 1 : -1,
                    ScaleY = End.Y > Start.Y ? 1 : -1,
                }
            };

            if (strokeDashArray != null)
            {
               shape.StrokeDashArray = new DoubleCollection(strokeDashArray);
            };

            Canvas.SetLeft(shape, Start.X);
            Canvas.SetTop(shape, Start.Y);

            return shape;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
