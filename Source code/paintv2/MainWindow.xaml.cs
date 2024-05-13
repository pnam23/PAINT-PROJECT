using Fluent;
using Newtonsoft.Json;
using Shapes;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace paintv2
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        private List<IShape> _allShapes = new List<IShape>();
        Dictionary<string, IShape> _prototypes = new Dictionary<string, IShape>();
        IShape _painter = null;
        List<IShape> _painters = new List<IShape>();
        List<Stroke> _strokes = new List<Stroke>();
        Point _start;
        Point _end;

        private static SolidColorBrush _selectedColor = new SolidColorBrush(Colors.Black);
        private static SolidColorBrush _oldSelectedColor = new SolidColorBrush(Colors.Black);
        private static int _selectedThickness = 1;
        private static DoubleCollection _selectedDash = null;
        
        bool _isDrawing = false;
        bool _isSaved = false;


        public MainWindow()
        {
            InitializeComponent();
        }

        // Create new file
        private void btnCreate_Click(object sender, RoutedEventArgs e)
        {
            if (_isSaved)
            {
                DefaultCanvas();
                return;
            }
            else
            {
                var ans = MessageBox.Show("Do you want to save current work?", "Unsaved changes detected", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (ans == MessageBoxResult.Yes)
                {
                    btnSave_Click(sender, e);
                    DefaultCanvas();
                    _isSaved = true;
                }
                else if (ans == MessageBoxResult.No)
                {
                    DefaultCanvas();
                    return;
                }
                else
                {
                    return;
                }
            }
        }

        // default state
        private void DefaultCanvas()
        {
            myInkCanvas.Children.Clear();
            myInkCanvas.Strokes.Clear();
            myInkCanvas.Background = Brushes.White;
            _isSaved = false;
            _painters.Clear();
            _isDrawing = false;
            _selectedColor = new SolidColorBrush(Colors.Black);
            _selectedThickness = 1;
            cmbBrushSize.SelectedIndex = 0;
            cmbDash.SelectedIndex = 0;
            _selectedDash = null;
        }

        
        // Open file
        private void btnOpen_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "JSON (*.json)|*.json| PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = dialog.FileName;
                string extension = System.IO.Path.GetExtension(filePath).ToLower(); 

                OpenFile(myInkCanvas, filePath, extension);
            }
        }

        private void OpenFile(InkCanvas canvas, string filename, string extension)
        {
            try
            {
                if (extension == ".json")
                {

                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    };

                    string json = File.ReadAllText(filename);

                    Dictionary<string, object> data = JsonConvert.DeserializeObject<Dictionary<string, object>>(json, settings);

                    if (data.ContainsKey("Strokes"))
                    {
                        List<List<Point>> strokesData = JsonConvert.DeserializeObject<List<List<Point>>>(data["Strokes"].ToString());
                        foreach (List<Point> strokePoints in strokesData)
                        {
                            StylusPointCollection pointCollection = new StylusPointCollection();
                            foreach (Point point in strokePoints)
                            {
                                pointCollection.Add(new StylusPoint(point.X, point.Y));
                            }
                            Stroke stroke = new Stroke(pointCollection);
                            myInkCanvas.Strokes.Add(stroke);
                        }
                    }


                    if (data.ContainsKey("Shapes"))
                    {
                        List<IShape> shapesList = JsonConvert.DeserializeObject<List<IShape>>(data["Shapes"].ToString());

                        foreach (IShape shape in shapesList)
                        {

                            UIElement shapeElement = shape.Convert(shape.Color, shape.Thickness, shape.Dash);
                            if (shapeElement != null)
                            {
                                myInkCanvas.Children.Add(shapeElement);
                            }
                        }
                    }

                }
                else
                {
                    // Clear existing strokes on the canvas
                    canvas.Strokes.Clear();
                    ImageBrush brush = new ImageBrush();
                    brush.ImageSource = new BitmapImage(new Uri( filename, UriKind.Absolute));

                    canvas.Background = brush;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        // Save file
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "JSON (*.json)|*.json| PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = dialog.FileName;
                string extension = filePath.Substring(filePath.LastIndexOf('\\') + 1).Split('.')[1];
                SaveFile(myInkCanvas, filePath, extension);
                MessageBox.Show($"Saved to '{filePath}'");
            }
        }
      
        private void SaveFile(InkCanvas canvas, string filename, string extension)
        {
            try
            {
                if (extension == "json")
                {
                    var settings = new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.Objects
                    };

                    
                    List<List<Point>> strokesData = new List<List<Point>>();
                    foreach (Stroke stroke in myInkCanvas.Strokes)
                    {
                        List<Point> strokePoints = new List<Point>();
                        foreach (StylusPoint point in stroke.StylusPoints)
                        {
                            strokePoints.Add(new Point(point.X, point.Y));
                        }
                        strokesData.Add(strokePoints);
                    }

                    List<IShape> shapesList = _painters.ToList();
                    Dictionary<string, object> dataToSave = new Dictionary<string, object>
            {
                { "Strokes", strokesData },
                { "Shapes", shapesList }
            };
                    string json = JsonConvert.SerializeObject(dataToSave, settings);

                    File.WriteAllText(filename, json);
                }
                else
                {
                    // Render the InkCanvas to a bitmap
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap(
                        (int)canvas.ActualWidth, (int)canvas.ActualHeight,
                        96d, 96d, PixelFormats.Default);
                    renderBitmap.Render(canvas);

                    BitmapEncoder encoder = null;
                    switch (extension)
                    {
                        case "png":
                            encoder = new PngBitmapEncoder();
                            break;
                        case "jpeg":
                            encoder = new JpegBitmapEncoder();
                            break;
                        case "bmp":
                            encoder = new BmpBitmapEncoder();
                            break;
                        default:
                            break;
                    }

                    if (encoder != null)
                    {
                        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
                        using (FileStream file = File.Create(filename))
                        {
                            encoder.Save(file);
                        }
                    }
                }
                _isSaved = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }


        // Import an image
        private void btnImport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.OpenFileDialog();
            dialog.Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = dialog.FileName;
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = new BitmapImage(new Uri(filePath, UriKind.Absolute));
                myInkCanvas.Background = brush;
            }
        }

        // Export to image
        private void btnExport_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new System.Windows.Forms.SaveFileDialog();
            dialog.Filter = "PNG (*.png)|*.png| JPEG (*.jpeg)|*.jpeg| BMP (*.bmp)|*.bmp";

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string filePath = dialog.FileName;
                string extension = filePath.Substring(filePath.LastIndexOf('\\') + 1).Split('.')[1];
                SaveFile(myInkCanvas, filePath, extension);
                MessageBox.Show($"Saved to '{filePath}'");
            }
        }

        // Select object
        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            myInkCanvas.EditingMode = InkCanvasEditingMode.Select;
        }

        // Draw freestyle
        private void btnPencil_Click(object sender, RoutedEventArgs e)
        {
            myInkCanvas.EditingMode = InkCanvasEditingMode.Ink;
            if (myInkCanvas != null)
            {
                var drawingAttributes = new System.Windows.Ink.DrawingAttributes();
                drawingAttributes.Width = _selectedThickness;
                drawingAttributes.Height = _selectedThickness;
                drawingAttributes.Color = _selectedColor.Color;
                myInkCanvas.DefaultDrawingAttributes = drawingAttributes;
            }
        }


        // Erase 
        private void btnEraser_Click(object sender, RoutedEventArgs e)
        {
            //myInkCanvas.EditingMode = InkCanvasEditingMode.EraseByPoint;
            if (myInkCanvas != null)
            {
                var drawingAttributes = new System.Windows.Ink.DrawingAttributes();
                drawingAttributes.Width = 10;
                drawingAttributes.Height = 10;
                drawingAttributes.Color = Colors.White;
                myInkCanvas.DefaultDrawingAttributes = drawingAttributes;
            }
        }


        private Stack<Stroke> _stack = new Stack<Stroke>();

        //Undo command
        private void btnUndo_Click(object sender, RoutedEventArgs e)
        {
            if (myInkCanvas.Strokes.Count > 0)
            {
                Stroke lastStroke = myInkCanvas.Strokes[myInkCanvas.Strokes.Count - 1];
                myInkCanvas.Strokes.RemoveAt(myInkCanvas.Strokes.Count - 1);

                // Push the undone stroke to the redo stack
                _stack.Push(lastStroke);
            }
        }

        // Redo command
        private void btnRedo_Click(object sender, RoutedEventArgs e)
        {
            if (_stack.Count > 0)
            {
                Stroke redoStroke = _stack.Pop();

                myInkCanvas.Strokes.Add(redoStroke);
            }
        }


        // Copy object
        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            myInkCanvas.CopySelection();
        }

        //Paste object
        private void btnPaste_Click(object sender, RoutedEventArgs e)
        {
            myInkCanvas.Paste();
        }

        // Cut object
        private void btnCut_Click(object sender, RoutedEventArgs e)
        {
            myInkCanvas.CutSelection();
        }

        // Select shape to draw
        private void lvShapes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this._allShapes.Count == 0)
                return;

            var index = lvShapes.SelectedIndex;

            _painter = _allShapes[index];
            myInkCanvas.EditingMode = InkCanvasEditingMode.None;
        }


        private void myInkCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_painters.Count == 0) return;

            _isDrawing = true;
            _start = e.GetPosition(myInkCanvas);
        }

        private void myInkCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDrawing)
            {
                _end = e.GetPosition(myInkCanvas);
                myInkCanvas.Children.Clear();
                foreach (var painter in _painters)
                {
                    myInkCanvas.Children.Add(painter.Convert(painter.Color, painter.Thickness, painter.Dash));
                }

                // Drawing Shapes
                if (_painter != null)
                {
                    _painter.AddFirst(_start);
                    _painter.AddSecond(_end);
                    myInkCanvas.Children.Add(_painter.Convert(_selectedColor, _selectedThickness, _selectedDash));
                }

                _isSaved = false;
            }
        }

        private void myInkCanvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDrawing = false;

            if (_painter != null)
            {
                _painter.Color = _selectedColor;
                _painter.Thickness = _selectedThickness;
                _painter.Dash = _selectedDash;
                _painters.Add((IShape)_painter.Clone());    
            }
        }

        // Convert IShape to Stroke 
        private List<Point> SerializeStroke(Stroke stroke)
        {
            List<Point> points = new List<Point>();
            foreach (var point in stroke.StylusPoints)
            {
                points.Add(new Point(point.X, point.Y));
            }
            return points;
        }

        // Load program
        private void RibbonWindow_Loaded(object sender, RoutedEventArgs e)
        {

            // Single configuration
            string folder = AppDomain.CurrentDomain.BaseDirectory;
            var fis = new DirectoryInfo(folder).GetFiles("*.dll");

            foreach (var fi in fis)
            {
                // Get all data from dll
                var assembly = Assembly.LoadFrom(fi.FullName);
                var types = assembly.GetTypes();

                foreach (var type in types)
                {
                    if (type.IsClass && typeof(IShape).IsAssignableFrom(type))
                    {
                        IShape shape = (IShape)Activator.CreateInstance(type);
                        _prototypes.Add(shape.Name, shape);
                    }
                }
            }


            // ---------------------------------------------------
            // Create UI automatically
            foreach (var item in _prototypes)
            {
                var shape = item.Value as IShape;
                _allShapes.Add(shape);
            }
            lvShapes.ItemsSource = _allShapes;

            if (this._allShapes.Count == 0)
                return;

            lvShapes.SelectedItem = null;
        }

        // Close program
        private void RibbonWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_isSaved)
            {
                Application.Current.Shutdown();
            }
            else
            {
                var ans = MessageBox.Show("Do you want to save current work?", "Unsaved changes detected", MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (ans == MessageBoxResult.Yes)
                {
                    btnSave_Click(sender, null);
                }
                Application.Current.Shutdown();
            }
        }


        // Change Brush Thickness
        private void cmbBrushSize_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = cmbBrushSize.SelectedIndex;

            if (i == 0)
            {
                _selectedThickness = 1;
            }
            else if (i == 1)
            {
                _selectedThickness = 3;
            }
            else if (i == 2)
            {
                _selectedThickness = 5;
            }
            else if (i == 3)
            {
                _selectedThickness = 8;
            }

            if (myInkCanvas != null)
            {
                var drawingAttributes = new System.Windows.Ink.DrawingAttributes();
                drawingAttributes.Width = _selectedThickness;
                drawingAttributes.Height = _selectedThickness;
                drawingAttributes.Color = _selectedColor.Color;
                myInkCanvas.DefaultDrawingAttributes = drawingAttributes;
            }
        }

        // Change StrokeDash
        private void cmbDash_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int i = cmbDash.SelectedIndex;
            if (i == 0)
            {
                _selectedDash = null;
            }
            else if (i == 1)
            {
                _selectedDash = new DoubleCollection() { 5, 1 };
            }
            else if (i == 2)
            {
                _selectedDash = new DoubleCollection() { 1, 1 };
            }
            else if (i == 3)
            {
                _selectedDash = new DoubleCollection() { 5, 1, 1, 1 };
            }
        }

        // Pick color
        private void btnEditColors_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.ColorDialog picker = new System.Windows.Forms.ColorDialog();

            if (picker.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Color newColor = Color.FromArgb(picker.Color.A, picker.Color.R, picker.Color.G, picker.Color.B);
                ChangeInkColor(newColor);
                _selectedColor = new SolidColorBrush(Color.FromRgb(picker.Color.R, picker.Color.G, picker.Color.B));
            }
        }

        // Colors
        private void btnBlackColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Black);
            ChangeInkColor(Colors.Black);
        }

        private void btnGreyColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Gray);
            ChangeInkColor(Colors.Gray);
        }

        private void btnRedColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Red);
            ChangeInkColor(Colors.Red);
        }

        private void btnOrangeColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Orange);
            ChangeInkColor(Colors.Orange);            
        }

        private void btnYellowColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Yellow);
            ChangeInkColor(Colors.Yellow);           
        }

        private void btnGreenColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Green);
            ChangeInkColor(Colors.Green);            
        }

        private void btnBlueColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Blue);
            ChangeInkColor(Colors.Blue);
        }

        private void btnPurpleColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Purple);
            ChangeInkColor(Colors.Purple);            
        }

        private void btnBrownColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Brown);
            ChangeInkColor(Colors.Brown);            
        }

        private void btnPinkColor_Click(object sender, RoutedEventArgs e)
        {
            _selectedColor = new SolidColorBrush(Colors.Pink);
            ChangeInkColor(Colors.Pink);            
        }

        private void ChangeInkColor(Color newColor)
        {
            if (myInkCanvas != null)
            {
                DrawingAttributes attributes = new DrawingAttributes
                {
                    Color = newColor
                };

                myInkCanvas.DefaultDrawingAttributes = attributes;
            }
        }

        
    }
}