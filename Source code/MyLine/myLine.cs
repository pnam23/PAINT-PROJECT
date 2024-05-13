
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows;
using Shapes;

namespace MyLine
{
    public class myLine : IShape
    {

        public string Name => "Line";
        public string ImageSource => $"pack://application:,,,/Images/Line.png";
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
            return new Line()
            {
                X1 = Start.X,
                Y1 = Start.Y,
                X2 = End.X,
                Y2 = End.Y,
                StrokeThickness = thickness,
                Stroke = color,
                StrokeDashArray = dash
            };
        }
    }
}
