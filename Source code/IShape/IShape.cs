using System.Windows;
using System.Windows.Media;

namespace Shapes
{
    public interface IShape : ICloneable
    {
        void AddFirst(Point point);
        void AddSecond(Point point);
        UIElement Convert(SolidColorBrush color, int thickness, DoubleCollection dash);
        string Name { get; }
        string ImageSource { get; }
        SolidColorBrush Color { get; set; }
        int Thickness { get; set; }
        DoubleCollection Dash { get; set; }
        Point Start { get; set; }
        Point End { get; set; }

    }

}
