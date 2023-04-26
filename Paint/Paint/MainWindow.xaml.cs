using PContract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Paint
{
    public partial class MainWindow : Window
    {

        private Dictionary<string, IShape> _abilities = new();
        private bool _isDrawing = false;
        private Point _start;
        private Point _end;
        private string _selectedType = null;
        private List<IShape> _shapes = new();
        private IShape? _prototype = null;
        private Color _selectedColor = Colors.Black;
        private double[] _selectedDashArray = null;
        private int _selectedThickness = 2;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadShapes();
        }

        private void LoadShapes()
        {
            var domain = AppDomain.CurrentDomain;
            var folder = domain.BaseDirectory;
            var folderInfo = new DirectoryInfo(folder);
            var dllFiles = folderInfo.GetFiles("*.dll");

            foreach (var dll in dllFiles)
            {
                Debug.WriteLine(dll.FullName);
                var assembly = Assembly.LoadFrom(dll.FullName);

                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass &&
                        typeof(IShape).IsAssignableFrom(type))
                    {
                        var shape = Activator.CreateInstance(type) as IShape;

                        if (_selectedType == null)
                        {
                            _selectedType = shape!.Name;
                        }

                        _abilities.Add(shape!.Name, shape);
                    }
                }
            }

            foreach (var ability in _abilities)
            {
                var button = new Button()
                {
                    Width = 80,
                    Height = 35,
                    Content = ability.Value.Name,
                    Tag = ability.Value.Name
                };

                button.Click += Ability_Click;
                shapePanel.Children.Add(button);
            }
        }

        private void Ability_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            string name = (string)button.Tag;
            _selectedType = name;
        }

        private void ResetPosition()
        {
            _start = new Point(0, 0);
            _end = new Point(0, 0);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                _isDrawing = true;

                _start = e.GetPosition(previewCanvas);

                _prototype = (IShape)
                _abilities[_selectedType].Clone();
                _prototype.UpdateStart(_start);
            }
            else if (e.ChangedButton == MouseButton.Right)
            {
                previewCanvas.Children.Clear();
                ResetPosition();
                _isDrawing = false;
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing && e.ChangedButton == MouseButton.Left)
            {
                _shapes.Add((IShape)_prototype.Clone());
                UIElement newShape = _prototype.Draw(_selectedColor, _selectedThickness, _selectedDashArray);
                primaryCanvas.Children.Add(newShape);
                _isDrawing = false;
            }
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                previewCanvas.Children.Clear();

                _end = e.GetPosition(previewCanvas);
                _prototype.UpdateEnd(_end);

                UIElement previewShape = _prototype.Draw(_selectedColor, _selectedThickness, _selectedDashArray);
                previewCanvas.Children.Add(previewShape);
            }
        }

        private void ColorComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            var selectedItem = (ComboBoxItem)ColorComboBox.SelectedItem;
            var colorName = (string)selectedItem.Tag;

            switch (colorName)
            {
                case "Red":
                    _selectedColor = Colors.Red;
                    break;
                case "Green":
                    _selectedColor = Colors.Green;
                    break;
                case "Blue":
                    _selectedColor = Colors.Blue;
                    break;
            }
        }

        private void ThicknessComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)ThicknessComboBox.SelectedItem;
            _selectedThickness = int.Parse((string)selectedItem.Tag);
        }

        private void StrokeTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedItem = (ComboBoxItem)StrokeTypeComboBox.SelectedItem;
            var strokeType = (string)selectedItem.Tag;

            switch (strokeType)
            {
                case "Solid":
                    _selectedDashArray = null;
                    break;
                case "Dash":
                    _selectedDashArray = new double[] { 4, 4 };
                    break;
                case "Dot":
                    _selectedDashArray = new double[] { 1, 2 };
                    break;
                case "DashDotDash":
                    _selectedDashArray = new double[] { 4, 2, 1, 2 };
                    break;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            string path = PromptSelectFolder();

            var selectedItem = (ComboBoxItem)SaveTypeComboBox.SelectedItem;
            var selectedType = (string)selectedItem.Tag;

            if (path != null)
            {
                if (selectedType == "png")
                {
                    SaveCanvasToPng(primaryCanvas, path + "\\image.png");
                }
                else if (selectedType == "jpg")
                {
                    SaveCanvasToJpg(primaryCanvas, path + "\\image.jpg");
                }
                else if (selectedType == "bmp")
                {
                    SaveCanvasToBmp(primaryCanvas, path + "\\image.bmp");
                }
            }
        }

        public string PromptSelectFolder()
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = dialog.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    return dialog.SelectedPath;
                }
            }

            return null;
        }

        public void SaveCanvasToPng(Canvas canvas, string filePath)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);

            PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
            pngEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using (Stream fileStream = File.Create(filePath))
            {
                pngEncoder.Save(fileStream);
            }
        }

        public void SaveCanvasToBmp(Canvas canvas, string filePath)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);

            BmpBitmapEncoder bmpEncoder = new BmpBitmapEncoder();
            bmpEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using (Stream fileStream = File.Create(filePath))
            {
                bmpEncoder.Save(fileStream);
            }
        }

        public void SaveCanvasToJpg(Canvas canvas, string filePath, int quality = 100)
        {
            RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                (int)canvas.ActualWidth, (int)canvas.ActualHeight, 96d, 96d, PixelFormats.Pbgra32);
            renderBitmap.Render(canvas);

            JpegBitmapEncoder jpgEncoder = new JpegBitmapEncoder();
            jpgEncoder.QualityLevel = quality;
            jpgEncoder.Frames.Add(BitmapFrame.Create(renderBitmap));
            using (Stream fileStream = File.Create(filePath))
            {
                jpgEncoder.Save(fileStream);
            }
        }
    }
}
