using ShapeableAbility;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ExtendShapes
{
    public class PRoundedRectangle : IShape
    {
        public Point Start { get; set; }
        public Point End { get; set; }

        public string Name => "Rounded Rectangle";

        public BitmapImage Icon { get; }

        public PRoundedRectangle()
        {
            Icon = new BitmapImage(new Uri("pack://application:,,,/ExtendShapes;component/Resources/Images/rounded-rectangle.png"));
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
            var rect = new Rectangle()
            {
                Stroke = new SolidColorBrush(strokeColor),
                StrokeThickness = thickness,
                Fill = new SolidColorBrush(fillColor),
                Width = Math.Abs(End.X - Start.X),
                Height = Math.Abs(End.Y - Start.Y),
                RadiusX = 20,
                RadiusY = 20,
                RenderTransform = new ScaleTransform()
                {
                    ScaleX = End.X > Start.X ? 1 : -1,
                    ScaleY = End.Y > Start.Y ? 1 : -1,
                }
            };

            if (strokeDashArray != null)
            {
                rect.StrokeDashArray = new DoubleCollection(strokeDashArray);
            }

            Canvas.SetLeft(rect, Start.X);
            Canvas.SetTop(rect, Start.Y);

            return rect;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
