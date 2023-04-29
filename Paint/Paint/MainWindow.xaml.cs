using PContract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
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
        private int _selectedThickness = 1;
        private List<Command> _commandList = new List<Command>();
        private int _currentCommandIndex = -1;
        private bool _isAddingText = false;
        private UIElement _selectedElement;
        private TextBox _textBox;

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
            _isAddingText = false;
        }

        private void ResetPosition()
        {
            _start = new Point(0, 0);
            _end = new Point(0, 0);
        }

        private void Canvas_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (_isAddingText)
            {
                _textBox = new TextBox();
                _textBox.Width = 50;
                _textBox.Focus();
                _textBox.AcceptsReturn = true;
                _textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
                primaryCanvas.Children.Add(_textBox);

                Keyboard.Focus(_textBox);

                Point mousePosition = e.GetPosition(primaryCanvas);
                Canvas.SetLeft(_textBox, mousePosition.X);
                Canvas.SetTop(_textBox, mousePosition.Y);

            }

            else if (e.ChangedButton == MouseButton.Left)
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

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBlock textBlock = new TextBlock();
                textBlock.Text = _textBox.Text;
                textBlock.FontSize = 12;
                textBlock.Foreground = new SolidColorBrush(Colors.Black);

                Canvas.SetLeft(textBlock, Canvas.GetLeft(_textBox));
                Canvas.SetTop(textBlock, Canvas.GetTop(_textBox));

                primaryCanvas.Children.Add(textBlock);

                primaryCanvas.Children.Remove(_textBox);
                _textBox = null;
            }
        }

        private void Canvas_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_isDrawing && e.ChangedButton == MouseButton.Left)
            {
                _shapes.Add((IShape)_prototype.Clone());
                UIElement newShape = _prototype.Draw(_selectedColor, _selectedThickness, _selectedDashArray);
                primaryCanvas.Children.Add(newShape);
                previewCanvas.Children.Clear();

                if (newShape != null && primaryCanvas.Children.Contains(newShape))
                {
                    _selectedElement = newShape;
                }

                if (_currentCommandIndex < _commandList.Count - 1)
                {
                    _commandList.RemoveRange(_currentCommandIndex + 1, _commandList.Count - _currentCommandIndex - 1);
                }

                AddCommand command = new AddCommand(primaryCanvas, newShape);
                _commandList.Add(command);
                _currentCommandIndex++;

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
                case "Black":
                    _selectedColor = Colors.Black;
                    break;
                case "White":
                    _selectedColor = Colors.White;
                    break;
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

        private void canvas_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                double zoomValue = primaryCanvas.LayoutTransform.Value.M11;
                double zoomFactor = e.Delta > 0 ? 1.1 : 0.9;
                zoomValue *= zoomFactor;

                if (zoomValue < 0.1) zoomValue = 0.1;
                if (zoomValue > 10) zoomValue = 10;

                ScaleTransform scaleTransform = new ScaleTransform(zoomValue, zoomValue);
                primaryCanvas.LayoutTransform = scaleTransform;

                e.Handled = true;
            }
        }

        private void canvas_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                primaryCanvas.Cursor = Cursors.Hand;
            }
        }

        private void primaryCanvas_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl || e.Key == Key.RightCtrl)
            {
                primaryCanvas.Cursor = Cursors.Arrow;
            }
        }

        public abstract class Command
        {
            protected Canvas canvas;
            protected UIElement element;

            public Command(Canvas canvas, UIElement element)
            {
                this.canvas = canvas;
                this.element = element;
            }

            public abstract void Undo();
            public abstract void Redo();
        }

        public class AddCommand : Command
        {
            public AddCommand(Canvas canvas, UIElement rectangle) : base(canvas, rectangle)
            {
            }

            public override void Undo()
            {
                canvas.Children.Remove(element);
            }

            public override void Redo()
            {
                canvas.Children.Add(element);
            }
        }

        private void Undo()
        {
            if (_currentCommandIndex >= 0)
            {
                Command command = _commandList[_currentCommandIndex];
                command.Undo();
                _currentCommandIndex--;
            }
        }

        private void Redo()
        {
            if (_currentCommandIndex < _commandList.Count - 1)
            {
                _currentCommandIndex++;
                Command command = _commandList[_currentCommandIndex];
                command.Redo();
            }
        }

        private void ButtonUndo_Click(object sender, RoutedEventArgs e)
        {
            Undo();
        }

        private void ButtonRedo_Click(object sender, RoutedEventArgs e)
        {
            Redo();
        }

        private UIElement copiedElement;

        private void Cut_Click(object sender, RoutedEventArgs e)
        {
            if (primaryCanvas.Children.Contains(_selectedElement))
            {
                copiedElement = CopyElement(_selectedElement);

                primaryCanvas.Children.Remove(_selectedElement);
                _selectedElement = null;
            }
        }

        private UIElement CopyElement(UIElement element)
        {
            string xaml = XamlWriter.Save(element);

            UIElement copy = XamlReader.Parse(xaml) as UIElement;

            return copy;
        }

        private void Paste_Click(object sender, RoutedEventArgs e)
        {
            if (copiedElement != null)
            {

                primaryCanvas.Children.Add(copiedElement);

                Canvas.SetLeft(copiedElement, Canvas.GetLeft(copiedElement) + 10);
                Canvas.SetTop(copiedElement, Canvas.GetTop(copiedElement) + 10);

                _selectedElement = copiedElement;
                copiedElement = CopyElement(copiedElement);
            }
        }


        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            if (primaryCanvas.Children.Contains(_selectedElement))
            {
                copiedElement = CopyElement(_selectedElement);

                _selectedElement = null;
            }
        }

        private void AddImage_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".png";
            dlg.Filter = "PNG files (*.png)|*.png|JPEG files (*.jpg)|*.jpg|Bitmap files (*.bmp)|*.bmp";

            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                BitmapImage bitmapImage = new BitmapImage(new Uri(filename, UriKind.Absolute));
                Image image = new Image();
                image.Source = bitmapImage;
                primaryCanvas.Children.Add(image);
            }
        }

        private void AddTextButton_Click(object sender, RoutedEventArgs e)
        {
            _isAddingText = true;
            _isDrawing = false;
        }
    }
}
