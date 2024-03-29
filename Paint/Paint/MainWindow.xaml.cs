﻿using BaseShapes;
using Microsoft.Win32;
using Paint.Commands;
using ShapeableAbility;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WK.Libraries.SharpClipboardNS;
using static WK.Libraries.SharpClipboardNS.SharpClipboard;
using Color = System.Windows.Media.Color;
using Cursors = System.Windows.Input.Cursors;
using Image = System.Windows.Controls.Image;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Windows.Point;
using TextBox = Fluent.TextBox;

namespace Paint
{
    public partial class MainWindow : Window
    {
        private bool _isDrawing = false;
        private Point _start;
        private Point _end;
        private string? _selectedShapeControl = null;
        private IShape? _prototype = null;
        public Color CurrentColor = Colors.Black;
        public Color CurrentFillColor = Colors.Black;
        private double[]? _currentStrokeType = null;
        private int _currentStrokeThickness = 1;
        private TextBox? _textBox;
        private DrawingType _currentDrawingType = DrawingType.Pencil;
        private readonly Dictionary<string, IShape> _shapeControls = new();
        private UIElement? _currentElement = null;
        private readonly List<Command> _commands = new();
        private int _currentCommandIndex = -1;
        private readonly SharpClipboard _clipboard = new();
        private readonly Rectangle _eraser = new()
        {
            Width = 20,
            Height = 20,
            Fill = Brushes.White,
            Stroke = Brushes.Black,
            StrokeThickness = 1,
            Visibility = Visibility.Hidden
        };

        public List<IShape> ShapeControls
        {
            get
            {
                return _shapeControls.Select(shape => shape.Value).ToList();
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            LoadShapes();
        }

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            EventCanvas.Cursor = Cursors.Pen;
            DataContext = this;
            RestoreStates();
            _clipboard.ClipboardChanged += OnClipboardChanged;
            EventCanvas.Children.Add(_eraser);
        }

        private void LoadShapes()
        {
            var domain = AppDomain.CurrentDomain;
            var folder = domain.BaseDirectory;
            var folderInfo = new DirectoryInfo(folder);
            var dllFiles = folderInfo.GetFiles("*.dll");

            foreach (var dll in dllFiles)
            {
                var assembly = Assembly.LoadFrom(dll.FullName);

                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (!type.IsClass || !typeof(IShape).IsAssignableFrom(type))
                    {
                        continue;
                    }

                    var shape = Activator.CreateInstance(type) as IShape;

                    _shapeControls.Add(shape!.Name, shape);
                }
            }
        }

        private void EventCanvas_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (_currentDrawingType == DrawingType.Eraser)
            {
                var position = e.GetPosition(EventCanvas);

                if (position.X < 0 || position.Y < 0 || position.X > EventCanvas.ActualWidth ||
                    position.Y > EventCanvas.ActualHeight)
                {
                    _eraser.Visibility = Visibility.Hidden;
                    return;
                }

                _eraser.Visibility = Visibility.Visible;

                Canvas.SetLeft(_eraser, position.X - _eraser.Width / 2);
                Canvas.SetTop(_eraser, position.Y - _eraser.Height / 2);
            }

            if (!_isDrawing) return;

            var fillColor = CheckBoxApplyFillColor.IsChecked == true ? CurrentFillColor : Colors.Transparent;

            if (_currentDrawingType == DrawingType.Shape)
            {
                PreviewCanvas.Children.Clear();
                _end = e.GetPosition(PreviewCanvas);
                _prototype.UpdateEnd(_end);

                var previewShape = _prototype.Draw(CurrentColor, fillColor, _currentStrokeThickness, _currentStrokeType);
                PreviewCanvas.Children.Add(previewShape);
            }
            else if (_currentDrawingType == DrawingType.Pencil)
            {
                var position = e.GetPosition(PreviewCanvas);
                _prototype = new PRectangle();
                _prototype.UpdateStart(position);
                _prototype.UpdateEnd(new Point(position.X + _currentStrokeThickness * 2, position.Y + _currentStrokeThickness * 2));

                var previewShape = _prototype.Draw(CurrentColor, CurrentColor, _currentStrokeThickness, null);
                PreviewCanvas.Children.Add(previewShape);
            }
            else if (_currentDrawingType == DrawingType.Eraser)
            {
                var newShape = new Rectangle()
                {
                    Width = _eraser.Width,
                    Height = _eraser.Height,
                    Fill = Brushes.White,
                    Stroke = Brushes.White,
                    StrokeThickness = 1
                };

                Canvas.SetLeft(newShape, Canvas.GetLeft(_eraser));
                Canvas.SetTop(newShape, Canvas.GetTop(_eraser));

                PreviewCanvas.Children.Add(newShape);
            }
        }

        private void EventCanvas_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDrawing || e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            UIElement? newElement = null;
            var fillColor = CheckBoxApplyFillColor.IsChecked == true ? CurrentFillColor : Colors.Transparent;

            if (_currentDrawingType == DrawingType.Shape)
            {
                if (_start == _end)
                {
                    _isDrawing = false;
                    return;
                }

                var shape = _prototype.Draw(CurrentColor, fillColor, _currentStrokeThickness, _currentStrokeType);

                PreviewCanvas.Children.Clear();

                newElement = shape;
            }
            else if (_currentDrawingType is DrawingType.Pencil or DrawingType.Eraser)
            {
                var canvas = new Canvas()
                {
                    Width = PreviewCanvas.Width,
                    Height = PreviewCanvas.Height,
                    Background = Brushes.Transparent
                };

                Canvas.SetLeft(canvas, 0);
                Canvas.SetTop(canvas, 0);

                var elements = PreviewCanvas.Children.OfType<UIElement>().ToList();
                foreach (var element in elements)
                {
                    PreviewCanvas.Children.Remove(element);
                    canvas.Children.Add(element);
                }

                newElement = canvas;
            }

            if (newElement == null)
            {
                _isDrawing = false;
                return;
            }

            AddUiElementToCanvas(newElement, _currentDrawingType is DrawingType.Shape);
            _isDrawing = false;
        }

        private void EventCanvas_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDrawing = true;

                _start = e.GetPosition(PreviewCanvas);
                _end = _start;

                if (_currentDrawingType == DrawingType.Shape)
                {
                    _prototype = (IShape)_shapeControls[_selectedShapeControl].Clone();
                    _prototype.UpdateStart(_start);
                }
                else if (_currentDrawingType == DrawingType.Text)
                {
                    if (_textBox != null)
                    {
                        AddTextToCanvas();
                        return;
                    }

                    _textBox = new TextBox
                    {
                        Width = 50
                    };
                    _textBox.Focus();
                    _textBox.AcceptsReturn = true;
                    _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                    PreviewCanvas.Children.Add(_textBox);

                    Keyboard.Focus(_textBox);

                    var mousePosition = e.GetPosition(PrimaryCanvas);
                    Canvas.SetLeft(_textBox, mousePosition.X);
                    Canvas.SetTop(_textBox, mousePosition.Y);
                }
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                ResetDrawing();
            }
        }

        private void AddTextToCanvas()
        {
            if (_textBox == null)
            {
                return;
            }

            if (_textBox.Text == string.Empty)
            {
                _textBox = null;
                ResetDrawing();
                return;
            }

            var textBlock = new TextBlock
            {
                Text = _textBox.Text,
                FontSize = 12,
                Foreground = new SolidColorBrush(Colors.Black)
            };

            Canvas.SetLeft(textBlock, Canvas.GetLeft(_textBox));
            Canvas.SetTop(textBlock, Canvas.GetTop(_textBox));

            AddUiElementToCanvas(textBlock);

            ResetDrawing();
            _textBox = null;
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                AddTextToCanvas();
            }
        }

        private void EventCanvas_OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers != ModifierKeys.Control)
            {
                return;
            }

            var zoomValue = PrimaryCanvas.LayoutTransform.Value.M11;
            var zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
            zoomValue *= zoomFactor;

            if (zoomValue < 0.1) zoomValue = 0.1;
            if (zoomValue > 10) zoomValue = 10;

            var scaleTransform = new ScaleTransform(zoomValue, zoomValue);
            PrimaryCanvas.LayoutTransform = scaleTransform;
            PreviewCanvas.LayoutTransform = scaleTransform;

            e.Handled = true;
        }

        private void ResetDrawing()
        {
            PreviewCanvas.Children.Clear();
            _start = new Point();
            _end = new Point();
            _isDrawing = false;
        }

        private void ButtonShape_OnClick(object sender, RoutedEventArgs e)
        {
            var button = (Fluent.ToggleButton)sender;
            _selectedShapeControl = (string)button.Tag;
            ResetDrawing();

            _currentDrawingType = DrawingType.Shape;
            EventCanvas.Cursor = Cursors.Cross;
        }

        private void ButtonPencil_OnClick(object sender, RoutedEventArgs e)
        {
            _currentDrawingType = DrawingType.Pencil;
            EventCanvas.Cursor = Cursors.Pen;
        }

        private void ButtonUndo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentCommandIndex < 0)
            {
                return;
            }

            var command = _commands[_currentCommandIndex];
            command.Undo();
            _currentCommandIndex--;

            ButtonRedo.IsEnabled = true;

            if (_currentCommandIndex < 0)
            {
                ButtonUndo.IsEnabled = false;
            }

            ButtonCopy.IsEnabled = false;
            ButtonCut.IsEnabled = false;
        }

        private void ButtonRedo_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentCommandIndex >= _commands.Count - 1)
            {
                return;
            }

            _currentCommandIndex++;
            var command = _commands[_currentCommandIndex];
            command.Redo();

            ButtonUndo.IsEnabled = true;

            if (_currentCommandIndex >= _commands.Count - 1)
            {
                ButtonRedo.IsEnabled = false;
            }

            ButtonCopy.IsEnabled = false;
            ButtonCut.IsEnabled = false;
        }

        private void ColorGallery_OnSelectedColorChanged(object sender, RoutedEventArgs e)
        {
            var selectedColor = ColorPicker.SelectedColor;

            if (selectedColor != null)
            {
                CurrentColor = (Color)selectedColor;
                ColorPickerCurrentColor.Fill = new SolidColorBrush(CurrentColor);
            }
        }

        private void ButtonOpen_OnClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                DefaultExt = ".png",
                Filter = "PNG files (*.png)|*.png|JPEG files (*.jpg)|*.jpg|Bitmap files (*.bmp)|*.bmp"
            };

            var result = dlg.ShowDialog();

            if (result != true) return;

            var filename = dlg.FileName;
            var bitmapImage = new BitmapImage();

            using (var stream = new FileStream(filename, FileMode.Open))
            {
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
            }

            var image = new Image
            {
                Source = bitmapImage
            };

            AddUiElementToCanvas(image);
        }

        private void ButtonText_OnClick(object sender, RoutedEventArgs e)
        {
            _currentDrawingType = DrawingType.Text;
            EventCanvas.Cursor = Cursors.IBeam;
        }

        private void ButtonSaveAsPNG_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                DefaultExt = ".png",
                Filter = "PNG files (*.png)|*.png"
            };

            var result = dialog.ShowDialog();
            if (result != true) return;
            var filename = dialog.FileName;

            var renderBitmap = new RenderTargetBitmap(
                (int)PrimaryCanvas.ActualWidth, (int)PrimaryCanvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(PrimaryCanvas);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

            using Stream fileStream = File.Create(filename);
            encoder.Save(fileStream);
        }

        private void ButtonSaveAsBMP_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                DefaultExt = ".bmp",
                Filter = "Bitmap files (*.bmp)|*.bmp"
            };

            var result = dialog.ShowDialog();
            if (result != true) return;
            var filename = dialog.FileName;

            var renderBitmap = new RenderTargetBitmap(
                (int)PrimaryCanvas.ActualWidth, (int)PrimaryCanvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(PrimaryCanvas);

            var encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using Stream fileStream = File.Create(filename);
            encoder.Save(fileStream);
        }

        private void ButtonSaveAsJPG_OnClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                DefaultExt = ".jpg",
                Filter = "JPEG files (*.jpg)|*.jpg"
            };

            var result = dialog.ShowDialog();
            if (result != true) return;
            var filename = dialog.FileName;

            var renderBitmap = new RenderTargetBitmap(
                (int)PrimaryCanvas.ActualWidth, (int)PrimaryCanvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(PrimaryCanvas);

            var encoder = new JpegBitmapEncoder
            {
                QualityLevel = 100
            };
            encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using Stream fileStream = File.Create(filename);
            encoder.Save(fileStream);
        }

        private void ComboBoxStrokeThickness_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = (TextBlock)ComboBoxStrokeThickness.SelectedValue;
            var thickness = int.Parse(selectedValue.Text);
            _currentStrokeThickness = thickness;
        }

        private void ComboBoxStrokeType_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedValue = (TextBlock)ComboBoxStrokeType.SelectedValue;
            var strokeType = selectedValue.Text;

            if (strokeType == "Solid")
            {
                _currentStrokeType = null;
            }
            else if (strokeType == "Dash")
            {
                _currentStrokeType = new double[] { 4, 4 };
            }
            else if (strokeType == "Dot")
            {
                _currentStrokeType = new double[] { 1, 2 };
            }
        }

        private void ButtonNew_OnClick(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("All current drawings will be lost. Are you sure you want to clear the canvas and start a new drawing?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                PrimaryCanvas.Children.Clear();
                ResetDrawing();
                _commands.Clear();
                _currentCommandIndex = -1;
                _textBox = null;
                _isDrawing = false;
                ButtonUndo.IsEnabled = false;
                ButtonRedo.IsEnabled = false;
            }
        }

        private void OnClipboardChanged(object? sender, ClipboardChangedEventArgs e)
        {
            ButtonPaste.IsEnabled = e.ContentType == ContentTypes.Image;

            if (e.ContentType != ContentTypes.Text)
            {
                return;
            }

            var tokens = ((string)e.Content).Split('|');

            if (tokens.Length != 2)
            {
                return;
            }

            ButtonPaste.IsEnabled = tokens[0] == "image" || tokens[0] == "xaml";
        }

        private void ButtonPaste_OnClick(object sender, RoutedEventArgs e)
        {
            if (Clipboard.ContainsImage())
            {
                var bitmapSource = Clipboard.GetImage();
                var image = new Image
                {
                    Source = bitmapSource
                };

                AddUiElementToCanvas(image);
            }
            else if (Clipboard.ContainsText())
            {
                var tokens = Clipboard.GetText().Split('|');

                if (tokens.Length != 2)
                {
                    return;
                }

                if (tokens[0] == "image")
                {
                    var base64Data = tokens[1];
                    var imageBytes = Convert.FromBase64String(base64Data);
                    var ms = new MemoryStream(imageBytes);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = ms;
                    bitmap.EndInit();
                    var image = new Image { Source = bitmap };
                    Canvas.SetLeft(image, 0);
                    Canvas.SetTop(image, 0);
                    AddUiElementToCanvas(image);
                }
                else if (tokens[0] == "xaml")
                {
                    var element = XamlReader.Parse(tokens[1]) as UIElement;
                    Canvas.SetLeft(element!, 0);
                    Canvas.SetTop(element!, 0);
                    AddUiElementToCanvas(element!, true);
                }
            }
        }

        private void ColorGalleryFill_OnSelectedColorChanged(object sender, RoutedEventArgs e)
        {
            var fillColor = ColorGalleryFill.SelectedColor;

            if (fillColor == null)
            {
                return;
            }

            CurrentFillColor = (Color)fillColor;
            EllipseColorGalleryFill.Fill = new SolidColorBrush(CurrentFillColor);
        }

        private void ButtonEraser_OnClick(object sender, RoutedEventArgs e)
        {
            _currentDrawingType = DrawingType.Eraser;
            EventCanvas.Cursor = Cursors.None;
        }

        private void ButtonCut_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentElement == null || !PrimaryCanvas.Children.Contains(_currentElement))
            {
                return;
            }

            var element = CopyElement(_currentElement);
            AddUiElementToClipboard(element);
            PrimaryCanvas.Children.Remove(_currentElement);
            _currentElement = null;
        }

        private void ButtonCopy_OnClick(object sender, RoutedEventArgs e)
        {
            if (_currentElement == null || !PrimaryCanvas.Children.Contains(_currentElement)) return;

            var element = CopyElement(_currentElement);
            AddUiElementToClipboard(element);
            _currentElement = null;
        }

        private static UIElement? CopyElement(UIElement element)
        {
            var xaml = XamlWriter.Save(element);
            return XamlReader.Parse(xaml) as UIElement;
        }

        private void AddUiElementToCanvas(UIElement element, bool isEnableCopyCut = false)
        {
            if (_currentCommandIndex < _commands.Count - 1)
            {
                _commands.RemoveRange(_currentCommandIndex + 1, _commands.Count - _currentCommandIndex - 1);
            }

            PrimaryCanvas.Children.Add(element);
            _commands.Add(new AddCommand(PrimaryCanvas, element));
            _currentCommandIndex++;

            ButtonUndo.IsEnabled = true;
            ButtonRedo.IsEnabled = false;

            if (isEnableCopyCut)
            {
                _currentElement = element;
                ButtonCopy.IsEnabled = true;
                ButtonCut.IsEnabled = true;
            }
            else
            {
                ButtonCopy.IsEnabled = false;
                ButtonCut.IsEnabled = false;
            }
        }

        public void AddUiElementToClipboard(UIElement? element)
        {
            if (element == null)
            {
                return;
            }

            if (element is Image image)
            {
                var bitmap = image.Source as BitmapSource;
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));

                var ms = new MemoryStream();
                encoder.Save(ms);
                var base64 = Convert.ToBase64String(ms.ToArray());

                Clipboard.SetText($"image|{base64}");
            }
            else
            {
                var xaml = XamlWriter.Save(element);
                Clipboard.SetText($"xaml|{xaml}");
            }
        }

        public void SaveStates()
        {
            var elements = PrimaryCanvas.Children.Cast<UIElement>().ToList();

            using var file = new StreamWriter("states.txt");

            foreach (var element in elements)
            {
                if (element is Image image)
                {
                    var bitmap = image.Source as BitmapSource;
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(bitmap));

                    var ms = new MemoryStream();
                    encoder.Save(ms);
                    var base64 = Convert.ToBase64String(ms.ToArray());

                    file.WriteLine($"image|{base64}");
                }
                else
                {
                    var xaml = XamlWriter.Save(element);
                    file.WriteLine($"xaml|{xaml}");
                }
            }
        }

        public void RestoreStates()
        {
            var elements = new List<UIElement>();

            try
            {
                using var file = new StreamReader("states.txt");

                while (!file.EndOfStream)
                {
                    var line = file.ReadLine();

                    if (line == null)
                    {
                        continue;
                    }

                    var tokens = line.Split('|');

                    if (tokens.Length != 2)
                    {
                        continue;
                    }

                    if (tokens[0] == "image")
                    {
                        var base64Data = tokens[1];
                        var imageBytes = Convert.FromBase64String(base64Data);
                        var ms = new MemoryStream(imageBytes);
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.StreamSource = ms;
                        bitmap.EndInit();
                        var image = new Image { Source = bitmap };
                        elements.Add(image);
                    }
                    else if (tokens[0] == "xaml")
                    {
                        var xaml = tokens[1];
                        var element = XamlReader.Parse(xaml) as UIElement;
                        elements.Add(element!);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return;
            }

            foreach (var element in elements)
            {
                AddUiElementToCanvas(element);
            }
        }

        private void MainWindow_OnClosing(object? sender, CancelEventArgs e)
        {
            SaveStates();
        }
    }
}
