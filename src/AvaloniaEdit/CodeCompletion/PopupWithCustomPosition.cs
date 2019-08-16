using Avalonia;
using Avalonia.Controls.Primitives;

namespace AvaloniaEdit.CodeCompletion
{
    internal class PopupWithCustomPosition : Popup
    {
        public static readonly AvaloniaProperty<Point> PositionProperty =
            AvaloniaProperty.Register<PopupWithCustomPosition, Point>(nameof(Position));

        public PixelPoint Position { get; set; }

        protected PixelPoint GetPosition()
        {
            return new PixelPoint((int)HorizontalOffset, (int)VerticalOffset);
        }
       
    }
}