using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ShapeableAbility
{
    public interface IShape : ICloneable
    {
        string Name { get; }
        BitmapImage Icon { get; }
        void UpdateStart(Point p);
        void UpdateEnd(Point p);
        UIElement Draw(Color strokeColor, Color fillColor, int thickness, double[]? strokeDashArray);
    }
}
