
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;

namespace MyEllipse
{
    public class myEllipse : IShape
    {
        public string Name => "Ellipse";
        public string ImageSource => $"pack://application:,,,/Images/Ellipse.png";
        public SolidColorBrush Color { get; set; }
        public int Thickness { get; set; }
        public DoubleCollection Dash { get; set; }
        public Point Start { get; set; }
        public Point End { get; set; }
        public void AddFirst(Point point)
        {
            Start = point;
        }

        public void AddSecond(Point point)
        {
            End = point;
        }

        public object Clone()
        {
            return MemberwiseClone();
        }

        public UIElement Convert(SolidColorBrush color, int thickness, DoubleCollection dash)
        {
            var left = Math.Min(End.X, Start.X);
            var top = Math.Min(End.Y, Start.Y);
            var right = Math.Max(End.X, Start.X);
            var bottom = Math.Max(End.Y, Start.Y);

            var width = right - left;
            var height = bottom - top;

            var item = new Ellipse()
            {
                Width = width,
                Height = height,
                StrokeThickness = thickness,
                Stroke = color,
                StrokeDashArray = dash
            };
            InkCanvas.SetLeft(item, left);
            InkCanvas.SetTop(item, top);
            return item;
        }
    }

}
