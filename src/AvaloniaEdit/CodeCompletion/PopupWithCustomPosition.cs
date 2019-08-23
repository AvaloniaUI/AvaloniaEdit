using Avalonia;
using Avalonia.Controls.Primitives;

namespace AvaloniaEdit.CodeCompletion
{
    internal class PopupWithCustomPosition : Popup
    {
        public static readonly AvaloniaProperty<Point> OffsetProperty =
            AvaloniaProperty.Register<PopupWithCustomPosition, Point>(nameof(Offset));

        public PixelPoint Offset
        {
            get
            {
                return new PixelPoint((int)HorizontalOffset, (int)VerticalOffset);
            }
            set
            {
                HorizontalOffset = value.X;
                VerticalOffset = value.Y;

                //this.Revalidate(VerticalOffsetProperty);
            }
        }            
    }
}