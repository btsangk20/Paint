using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Paint.MainWindow;

namespace Paint
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public interface IShape
        {
            Point _start { get; set; }
            Point _end { get; set; }
            void updateStart(Point point);
            void updateEnd(Point point);
            UIElement Draw();
        }

        public class MyLine : IShape
        {
            public Point _start { get; set; }
            public Point _end { get; set; }

            public void updateStart(Point point)
            {
                _start = point;
            }

            public void updateEnd(Point point)
            {
                _end = point;
            }

            public UIElement Draw()
            {
                var line = new Line
                {
                    X1 = _start.X,
                    Y1 = _start.Y,
                    X2 = _end.X,
                    Y2 = _end.Y,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };
                return line;
            }
        }

        public class MyRectangle : IShape
        {
            public Point _start { get; set; }
            public Point _end { get; set; }

            public void updateStart(Point point)
            {
                _start = point;
            }

            public void updateEnd(Point point)
            {
                _end = point;
            }

            public UIElement Draw()
            {
                var rectangle = new Rectangle
                {
                    Width = Math.Abs(_end.X - _start.X),
                    Height = Math.Abs(_end.Y - _start.Y),
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent
                };

                return rectangle;
            }
        }

        public class MyEllipse : IShape
        {
            public Point _start { get; set; }
            public Point _end { get; set; }

            public void updateStart(Point point)
            {
                _start = point;
            }

            public void updateEnd(Point point)
            {
                _end = point;
            }

            public UIElement Draw()
            {
                var ellipse = new Ellipse
                {
                    Width = Math.Abs(_end.X - _start.X),
                    Height = Math.Abs(_end.Y - _start.Y),
                    Stroke = Brushes.Black,
                    StrokeThickness = 2,
                    Fill = Brushes.Transparent
                };

                return ellipse;
            }
        }

        enum Shape
        {
            Line = 1,
            Rectangle = 2,
            Ellipse = 3,
        }

        private bool _isDrawing = false;
        private Point _start;
        private Point _end;
        private int selectedOption = 1;
        List<UIElement> shapes = new List<UIElement>();

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDrawing= true;

            _start = e.GetPosition(this);
            
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MyLine myLine = new MyLine();

            myLine.updateStart(_start);
            myLine.updateEnd(_end);

            UIElement renderLine = myLine.Draw();

            drawingCanvas.Children.Add(renderLine);

            shapes.Add(renderLine);

            _isDrawing = false;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(this);
                drawingCanvas.Children.Clear();

                if (selectedOption == (int) Shape.Line)
                {
                    MyLine myLine = new MyLine();

                    myLine.updateStart(_start);
                    myLine.updateEnd(_end);

                    UIElement line = myLine.Draw();

                    eventCanvas.Children.Add(line);
                    shapes.Add(line);
                } else if (selectedOption == (int) Shape.Rectangle)  {
                    MyRectangle myRectangle = new MyRectangle();

                    myRectangle.updateStart(_start);
                    myRectangle.updateEnd(_end);

                    UIElement rectangle = myRectangle.Draw();

                    eventCanvas.Children.Add(rectangle);
                    shapes.Add(rectangle);
                } else if (selectedOption == (int)Shape.Ellipse)
                {
                    MyEllipse myEllipse = new MyEllipse();

                    myEllipse.updateStart(_start);
                    myEllipse.updateEnd(_end);

                    UIElement ellipse = myEllipse.Draw();

                    eventCanvas.Children.Add(ellipse);
                    shapes.Add(ellipse);
                }
            }
        }
    }
}
