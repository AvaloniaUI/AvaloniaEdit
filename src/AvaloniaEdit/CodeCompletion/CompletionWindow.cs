﻿// Copyright (c) 2014 AlphaSierraPapa for the SharpDevelop Team
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
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;

namespace AvaloniaEdit.CodeCompletion
{
    /// <summary>
    /// The code completion window.
    /// </summary>
    public class CompletionWindow : CompletionWindowBase
    {
        private PopupWithCustomPosition _toolTip;

        /// <summary>
        /// Gets the completion list used in this completion window.
        /// </summary>
        public CompletionList CompletionList { get; }

        /// <summary>
        /// Creates a new code completion window.
        /// </summary>
        public CompletionWindow(TextArea textArea) : base(textArea)
        {
            CompletionList = new CompletionList();
            // keep height automatic
            CloseAutomatically = true;
            MaxHeight = 300;
            Width = 175;
            Content = CompletionList;
            // prevent user from resizing window to 0x0
            MinHeight = 15;
            MinWidth = 30;

            _toolTip = new PopupWithCustomPosition
            {
                StaysOpen = false,
                PlacementTarget = this,
                PlacementMode = PlacementMode.Right,
            };

            LogicalChildren.Add(_toolTip);

            _toolTip.Closed += (o, e) => ((Popup)o).Child = null;

            AttachEvents();
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            if (_toolTip != null)
            {
                _toolTip.IsOpen = false;
                _toolTip = null;
            }
        }

        #region ToolTip handling

        private void CompletionList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_toolTip == null) return;

            var item = CompletionList.SelectedItem;
            var description = item?.Description;
            if (description != null)
            {
                if (description is string descriptionText)
                {
                    _toolTip.Child = new TextBlock
                    {
                        Text = descriptionText,
                        TextWrapping = TextWrapping.Wrap
                    };
                }
                else
                {
                    _toolTip.Child = new ContentPresenter {Content = description};
                }

                _toolTip.Position = this.PointToScreen(Bounds.TopRight);
                _toolTip.IsOpen = true;
            }
            else
            {
                _toolTip.IsOpen = false;
            }
        }

        #endregion

        private void CompletionList_InsertionRequested(object sender, EventArgs e)
        {
            Hide();
            // The window must close before Complete() is called.
            // If the Complete callback pushes stacked input handlers, we don't want to pop those when the CC window closes.
            var item = CompletionList.SelectedItem;
            item?.Complete(TextArea, new AnchorSegment(TextArea.Document, StartOffset, EndOffset - StartOffset), e);
        }

        private void AttachEvents()
        {
            CompletionList.InsertionRequested += CompletionList_InsertionRequested;
            CompletionList.SelectionChanged += CompletionList_SelectionChanged;
            TextArea.Caret.PositionChanged += CaretPositionChanged;
            TextArea.PointerWheelChanged += TextArea_MouseWheel;
            TextArea.TextInput += TextArea_PreviewTextInput;
        }

        /// <inheritdoc/>
        protected override void DetachEvents()
        {
            CompletionList.InsertionRequested -= CompletionList_InsertionRequested;
            CompletionList.SelectionChanged -= CompletionList_SelectionChanged;
            TextArea.Caret.PositionChanged -= CaretPositionChanged;
            TextArea.PointerWheelChanged -= TextArea_MouseWheel;
            TextArea.TextInput -= TextArea_PreviewTextInput;
            base.DetachEvents();
        }

        /// <inheritdoc/>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                CompletionList.HandleKey(e);
            }
        }

        private void TextArea_PreviewTextInput(object sender, TextInputEventArgs e)
        {
            e.Handled = RaiseEventPair(this, null, TextInputEvent,
                                       new TextInputEventArgs { Device = e.Device, Text = e.Text });
        }

        private void TextArea_MouseWheel(object sender, PointerWheelEventArgs e)
        {
            e.Handled = RaiseEventPair(GetScrollEventTarget(),
                                       null, PointerWheelChangedEvent,
                                       new PointerWheelEventArgs { Device = e.Device, Delta = e.Delta, InputModifiers = e.InputModifiers });
        }

        private Control GetScrollEventTarget()
        {
            if (CompletionList == null)
                return this;
            return CompletionList.ScrollViewer ?? CompletionList.ListBox ?? (Control)CompletionList;
        }

        /// <summary>
        /// Gets/Sets whether the completion window should close automatically.
        /// The default value is true.
        /// </summary>
        public bool CloseAutomatically { get; set; }

        /// <inheritdoc/>
        protected override bool CloseOnFocusLost => CloseAutomatically;

        /// <summary>
        /// When this flag is set, code completion closes if the caret moves to the
        /// beginning of the allowed range. This is useful in Ctrl+Space and "complete when typing",
        /// but not in dot-completion.
        /// Has no effect if CloseAutomatically is false.
        /// </summary>
        public bool CloseWhenCaretAtBeginning { get; set; }

        private void CaretPositionChanged(object sender, EventArgs e)
        {
            var offset = TextArea.Caret.Offset;
            if (offset == StartOffset)
            {
                if (CloseAutomatically && CloseWhenCaretAtBeginning)
                {
                    Hide();
                }
                else
                {
                    CompletionList.SelectItem(string.Empty);
                }
                return;
            }
            if (offset < StartOffset || offset > EndOffset)
            {
                if (CloseAutomatically)
                {
                    Hide();
                }
            }
            else
            {
                var document = TextArea.Document;
                if (document != null)
                {
                    CompletionList.SelectItem(document.GetText(StartOffset, offset - StartOffset));
                }
            }
        }
    }
}
