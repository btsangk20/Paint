using ShapeableAbility;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BaseShapes
{
    public class PRectangle : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Rectangle";

        public BitmapImage Icon { get; }

        public PRectangle()
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/BaseShapes;component/Resources/Images/rectangle.png"));
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
            var width = Math.Abs(End.X - Start.X);
            var height = Math.Abs(End.Y - Start.Y);

            var shape = new Rectangle()
            {
                Width = width,
                Height = height,
                Stroke = new SolidColorBrush(strokeColor),
                Fill = new SolidColorBrush(fillColor),
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
