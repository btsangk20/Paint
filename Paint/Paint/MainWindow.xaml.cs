using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using Point = System.Windows.Point;
using Rectangle = System.Windows.Shapes.Rectangle;

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
        private int selectedOption = 2;
        List<UIElement> shapes = new();


        private void resetPosition()
        {
            _start = new Point(0, 0);
            _end = new Point(0, 0);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDrawing = true;

                _start = e.GetPosition(this);
            }


        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            UIElement renderShape = null;

            if (selectedOption == 1)
            {

                MyLine myLine = new MyLine();

                myLine.updateStart(_start);
                myLine.updateEnd(_end);

                renderShape = myLine.Draw();
            }
            else if (selectedOption == 2)
            {
                MyRectangle myRectangle = new MyRectangle();
                myRectangle.updateStart(_start);
                myRectangle.updateEnd(_end);
                renderShape = myRectangle.Draw();
                Canvas.SetLeft(renderShape, _start.X);
                Canvas.SetTop(renderShape, _start.Y);
            }
            else if (selectedOption == 3)
            {
                MyEllipse myEllipse = new MyEllipse();
                myEllipse.updateStart(_start);
                myEllipse.updateEnd(_end);
                renderShape = myEllipse.Draw();
                Canvas.SetLeft(renderShape, _start.X);
                Canvas.SetTop(renderShape, _start.Y);
            }

            if (renderShape != null && e.ChangedButton == MouseButton.Left)
            {
                eventCanvas.Children.Add(renderShape);

                shapes.Add(renderShape);
            }

            else if (e.ChangedButton == MouseButton.Right)
            {
                drawingCanvas.Children.Clear();
                resetPosition();
            }
            _isDrawing = false;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(this);
                drawingCanvas.Children.Clear();

                if (selectedOption == (int)Shape.Line)
                {
                    MyLine myLine = new MyLine();

                    myLine.updateStart(_start);
                    myLine.updateEnd(_end);

                    UIElement line = myLine.Draw();

                    drawingCanvas.Children.Add(line);
                    shapes.Add(line);
                }
                else if (selectedOption == (int)Shape.Rectangle)
                {
                    MyRectangle myRectangle = new MyRectangle();

                    myRectangle.updateStart(_start);
                    myRectangle.updateEnd(_end);

                    UIElement rectangle = myRectangle.Draw();

                    drawingCanvas.Children.Add(rectangle);
                    Canvas.SetLeft(rectangle, _start.X);
                    Canvas.SetTop(rectangle, _start.Y);
                    shapes.Add(rectangle);
                }
                else if (selectedOption == (int)Shape.Ellipse)
                {
                    MyEllipse myEllipse = new MyEllipse();

                    myEllipse.updateStart(_start);
                    myEllipse.updateEnd(_end);

                    UIElement ellipse = myEllipse.Draw();

                    drawingCanvas.Children.Add(ellipse);
                    Canvas.SetLeft(ellipse, _start.X);
                    Canvas.SetTop(ellipse, _start.Y);

                    shapes.Add(ellipse);
                }
            }
        }

        private void LineButton_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = (int)Shape.Line;
        }

        private void RectangleButton_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = (int)Shape.Rectangle;
        }

        private void EllipseButton_Click(object sender, RoutedEventArgs e)
        {
            selectedOption = (int)Shape.Ellipse;
        }
    }
}
