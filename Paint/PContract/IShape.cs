using System;
using System.Windows;
using System.Windows.Media;

namespace PContract
{
    public interface IShape : ICloneable
    {
        string Name { get; }
        void UpdateStart(Point p);
        void UpdateEnd(Point p);
        UIElement Draw(Color color, int thickness, double[] strokeDashArray);
    }
}
