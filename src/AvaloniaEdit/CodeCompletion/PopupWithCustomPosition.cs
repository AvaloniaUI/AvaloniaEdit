using Avalonia;
using Avalonia.Controls.Primitives;

namespace AvaloniaEdit.CodeCompletion
{
    internal class PopupWithCustomPosition : Popup
    {
        public static readonly AvaloniaProperty<Point> PositionProperty =
            AvaloniaProperty.Register<PopupWithCustomPosition, Point>(nameof(Position));

        public Point Position { get; set; }

        protected override Point GetPosition()
        {
            return Position;
        }
    }
}