// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this
// software and associated documentation files (the "Software"), to deal in the Software
// without restriction, including without limitation the rights to use, copy, modify, merge,
// publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
// to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or
// substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
// PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
// FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
// OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;

namespace AvaloniaEdit.CodeCompletion
{
    /// <summary>
    /// Base class for completion windows. Handles positioning the window at the caret.
    /// </summary>
    public class CompletionWindowBase : PopupRoot
    {
        static CompletionWindowBase()
        {
            BackgroundProperty.OverrideDefaultValue(typeof(CompletionWindowBase), Brushes.White);

            // TODO
            //ShowActivatedProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(Boxes.False));
            //ShowInTaskbarProperty.OverrideMetadata(typeof(CompletionWindowBase), new FrameworkPropertyMetadata(Boxes.False));
        }

        /// <summary>
        /// Gets the parent TextArea.
        /// </summary>
        public TextArea TextArea { get; }

        private readonly Window _parentWindow;
        private TextDocument _document;

        /// <summary>
        /// Gets/Sets the start of the text range in which the completion window stays open.
        /// This text portion is used to determine the text used to select an entry in the completion list by typing.
        /// </summary>
        public int StartOffset { get; set; }

        /// <summary>
        /// Gets/Sets the end of the text range in which the completion window stays open.
        /// This text portion is used to determine the text used to select an entry in the completion list by typing.
        /// </summary>
        public int EndOffset { get; set; }

        /// <summary>
        /// Gets whether the window was opened above the current line.
        /// </summary>
        protected bool IsUp { get; private set; }

        /// <summary>
        /// Creates a new CompletionWindowBase.
        /// </summary>
        public CompletionWindowBase(TextArea textArea)
        {
            TextArea = textArea ?? throw new ArgumentNullException(nameof(textArea));
            _parentWindow = textArea.GetVisualRoot() as Window;
            // TODO: owner
            //this.Owner = parentWindow;
            AddHandler(PointerReleasedEvent, OnMouseUp, handledEventsToo: true);

            StartOffset = EndOffset = TextArea.Caret.Offset;

            Deactivated += OnDeactivated;
            Closed += (sender, args) => DetachEvents();

            AttachEvents();

            Initailize();
        }

        private void Initailize()
        {
            if (_document != null && StartOffset != TextArea.Caret.Offset)
            {
                SetPosition(new TextViewPosition(_document.GetLocation(StartOffset)));
            }
            else
            {
                SetPosition(TextArea.Caret.Position);
            }
        }

        public new void Show()
        {
            using (BeginAutoSizing())
            {
                base.Show();
            }
        }

        public new void Hide()
        {
            using (BeginAutoSizing())
            {
                base.Hide();
            }
        }

        #region Event Handlers

        private void AttachEvents()
        {
            _document = TextArea.Document;
            if (_document != null)
            {
                _document.Changing += TextArea_Document_Changing;
            }
            // LostKeyboardFocus seems to be more reliable than PreviewLostKeyboardFocus - see SD-1729
            TextArea.LostFocus += TextAreaLostFocus;
            TextArea.TextView.ScrollOffsetChanged += TextViewScrollOffsetChanged;
            TextArea.DocumentChanged += TextAreaDocumentChanged;
            if (_parentWindow != null)
            {
                _parentWindow.PositionChanged += ParentWindow_LocationChanged;
            }

            // close previous completion windows of same type
            foreach (InputHandler x in TextArea.StackedInputHandlers.OfType<InputHandler>())
            {
                if (x.Window.GetType() == GetType())
                    TextArea.PopStackedInputHandler(x);
            }

            _myInputHandler = new InputHandler(this);
            TextArea.PushStackedInputHandler(_myInputHandler);
        }

        /// <summary>
        /// Detaches events from the text area.
        /// </summary>
        protected virtual void DetachEvents()
        {
            if (_document != null)
            {
                _document.Changing -= TextArea_Document_Changing;
            }
            TextArea.LostFocus -= TextAreaLostFocus;
            TextArea.TextView.ScrollOffsetChanged -= TextViewScrollOffsetChanged;
            TextArea.DocumentChanged -= TextAreaDocumentChanged;
            if (_parentWindow != null)
            {
                _parentWindow.PositionChanged -= ParentWindow_LocationChanged;
            }
            TextArea.PopStackedInputHandler(_myInputHandler);
        }

        #region InputHandler

        private InputHandler _myInputHandler;

        /// <summary>
        /// A dummy input handler (that justs invokes the default input handler).
        /// This is used to ensure the completion window closes when any other input handler
        /// becomes active.
        /// </summary>
        private sealed class InputHandler : TextAreaStackedInputHandler
        {
            internal readonly CompletionWindowBase Window;

            public InputHandler(CompletionWindowBase window)
                : base(window.TextArea)
            {
                Debug.Assert(window != null);
                Window = window;
            }

            public override void Detach()
            {
                base.Detach();
                Window.Hide();
            }

            private const Key KeyDeadCharProcessed = (Key)0xac; // Key.DeadCharProcessed; // new in .NET 4

            public override void OnPreviewKeyDown(KeyEventArgs e)
            {
                // prevents crash when typing deadchar while CC window is open
                if (e.Key == KeyDeadCharProcessed)
                    return;
                e.Handled = RaiseEventPair(Window, null, KeyDownEvent,
                                           new KeyEventArgs { Device = e.Device, Key = e.Key });
            }

            public override void OnPreviewKeyUp(KeyEventArgs e)
            {
                if (e.Key == KeyDeadCharProcessed)
                    return;
                e.Handled = RaiseEventPair(Window, null, KeyUpEvent,
                    new KeyEventArgs { Device = e.Device, Key = e.Key });
            }
        }
        #endregion

        private void TextViewScrollOffsetChanged(object sender, EventArgs e)
        {
            // TODO: handle scroll
            //IScrollInfo scrollInfo = TextArea.TextView;
            //Rect visibleRect = new Rect(scrollInfo.HorizontalOffset, scrollInfo.VerticalOffset, scrollInfo.ViewportWidth, scrollInfo.ViewportHeight);
            // close completion window when the user scrolls so far that the anchor position is leaving the visible area
            //if (visibleRect.Contains(visualLocation) || visibleRect.Contains(visualLocationTop))
            //	UpdatePosition();
            //else
            //	Close();
        }

        private void TextAreaDocumentChanged(object sender, EventArgs e)
        {
            Hide();
        }

        private void TextAreaLostFocus(object sender, RoutedEventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(CloseIfFocusLost, DispatcherPriority.Background);
        }

        private void ParentWindow_LocationChanged(object sender, EventArgs e)
        {
            UpdatePosition();
        }

        /// <inheritdoc/>
        private void OnDeactivated(object sender, EventArgs e)
        {
            Dispatcher.UIThread.InvokeAsync(CloseIfFocusLost, DispatcherPriority.Background);
        }
        #endregion

        /// <summary>
        /// Raises a tunnel/bubble event pair for a control.
        /// </summary>
        /// <param name="target">The control for which the event should be raised.</param>
        /// <param name="previewEvent">The tunneling event.</param>
        /// <param name="event">The bubbling event.</param>
        /// <param name="args">The event args to use.</param>
        /// <returns>The <see cref="RoutedEventArgs.Handled"/> value of the event args.</returns>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate")]
        protected static bool RaiseEventPair(Control target, RoutedEvent previewEvent, RoutedEvent @event, RoutedEventArgs args)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (args == null)
                throw new ArgumentNullException(nameof(args));
            if (previewEvent != null)
            {
                args.RoutedEvent = previewEvent;
                target.RaiseEvent(args);
            }
            args.RoutedEvent = @event ?? throw new ArgumentNullException(nameof(@event));
            target.RaiseEvent(args);
            return args.Handled;
        }

        // Special handler: handledEventsToo
        private void OnMouseUp(object sender, PointerReleasedEventArgs e)
        {
            ActivateParentWindow();
        }

        /// <summary>
        /// Activates the parent window.
        /// </summary>
        protected virtual void ActivateParentWindow()
        {
            _parentWindow?.Activate();
        }

        private void CloseIfFocusLost()
        {
            if (CloseOnFocusLost)
            {
                Debug.WriteLine("CloseIfFocusLost: this.IsActive=" + IsActive + " IsTextAreaFocused=" + IsTextAreaFocused);
                if (!IsActive && !IsTextAreaFocused)
                {
                    Hide();
                }
            }
        }

        /// <summary>
        /// Gets whether the completion window should automatically close when the text editor looses focus.
        /// </summary>
        protected virtual bool CloseOnFocusLost => true;

        private bool IsTextAreaFocused
        {
            get
            {
                if (_parentWindow != null && !_parentWindow.IsActive)
                    return false;
                return TextArea.IsFocused;
            }
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled && e.Key == Key.Escape)
            {
                e.Handled = true;
                Hide();
            }
        }

        private Point _visualLocation, _visualLocationTop;

        /// <summary>
        /// Positions the completion window at the specified position.
        /// </summary>
        protected void SetPosition(TextViewPosition position)
        {
            TextView textView = TextArea.TextView;

            _visualLocation = textView.GetVisualPosition(position, VisualYPosition.LineBottom);
            _visualLocationTop = textView.GetVisualPosition(position, VisualYPosition.LineTop);
            UpdatePosition();
        }

        /// <summary>
        /// Updates the position of the CompletionWindow based on the parent TextView position and the screen working area.
        /// It ensures that the CompletionWindow is completely visible on the screen.
        /// </summary>
        protected void UpdatePosition()
        {
            TextView textView = TextArea.TextView;
            // PointToScreen returns device dependent units (physical pixels)
            Point location = textView.PointToScreen(_visualLocation - textView.ScrollOffset);
            Point locationTop = textView.PointToScreen(_visualLocationTop - textView.ScrollOffset);

            // Let's use device dependent units for everything
            var completionWindowSize = this.PointToScreen(new Point(Bounds.Width, Bounds.Height));
            Rect bounds = new Rect(location, completionWindowSize);

            // TODO: position relative to working area
            //Rect workingScreen = System.Windows.Forms.Screen.GetWorkingArea(location.ToSystemDrawing()).ToWpf();
            //if (!workingScreen.Contains(bounds))
            //{
            //    if (bounds.Left < workingScreen.Left)
            //    {
            //        bounds.X = workingScreen.Left;
            //    }
            //    else if (bounds.Right > workingScreen.Right)
            //    {
            //        bounds.X = workingScreen.Right - bounds.Width;
            //    }
            //    if (bounds.Bottom > workingScreen.Bottom)
            //    {
            //        bounds.Y = locationTop.Y - bounds.Height;
            //        IsUp = true;
            //    }
            //    else
            //    {
            //        IsUp = false;
            //    }
            //    if (bounds.Y < workingScreen.Top)
            //    {
            //        bounds.Y = workingScreen.Top;
            //    }
            //}

            // Convert the window bounds to device independent units
            Position = this.PointToClient(new Point(bounds.X, bounds.Y));
        }

        // TODO: check if needed
        ///// <inheritdoc/>
        //protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        //{
        //    base.OnRenderSizeChanged(sizeInfo);
        //    if (sizeInfo.HeightChanged && IsUp)
        //    {
        //        this.Top += sizeInfo.PreviousSize.Height - sizeInfo.NewSize.Height;
        //    }
        //}

        /// <summary>
        /// Gets/sets whether the completion window should expect text insertion at the start offset,
        /// which not go into the completion region, but before it.
        /// </summary>
        /// <remarks>This property allows only a single insertion, it is reset to false
        /// when that insertion has occurred.</remarks>
        public bool ExpectInsertionBeforeStart { get; set; }

        private void TextArea_Document_Changing(object sender, DocumentChangeEventArgs e)
        {
            if (e.Offset + e.RemovalLength == StartOffset && e.RemovalLength > 0)
            {
                Hide(); // removal immediately in front of completion segment: close the window
                        // this is necessary when pressing backspace after dot-completion
            }
            if (e.Offset == StartOffset && e.RemovalLength == 0 && ExpectInsertionBeforeStart)
            {
                StartOffset = e.GetNewOffset(StartOffset, AnchorMovementType.AfterInsertion);
                ExpectInsertionBeforeStart = false;
            }
            else
            {
                StartOffset = e.GetNewOffset(StartOffset, AnchorMovementType.BeforeInsertion);
            }
            EndOffset = e.GetNewOffset(EndOffset, AnchorMovementType.AfterInsertion);
        }
    }
}
