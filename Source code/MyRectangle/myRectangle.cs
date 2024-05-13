
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using Shapes;
using System.Windows.Shapes;

namespace MyRectangle
{
    public class myRectangle : IShape
    {

        public string Name => "Rectangle";
        public string ImageSource => $"pack://application:,,,/Images/Rectangle.png";
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

            var item = new Rectangle()
            {   // TODO: end luon luon lon hon start
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
