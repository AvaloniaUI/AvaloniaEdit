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
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;

namespace AvaloniaEdit.Search
{
    /// <summary>
    /// A button that opens a drop-down menu when clicked.
    /// </summary>
    public class DropDownButton : Button
    {
        /// <summary>
        /// Identifies the <see cref="DropDownContentProperty" /> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty<Popup> DropDownContentProperty
            = AvaloniaProperty.Register<DropDownButton, Popup>(nameof(DropDownContent));

        /// <summary>
        /// Gets/Sets the popup that is used as drop-down content.
        /// </summary>
        public Popup DropDownContent
        {
            get => GetValue(DropDownContentProperty);
            set => SetValue(DropDownContentProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="IsDropDownContentOpenProperty" /> dependency property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsDropDownContentOpenProperty
            = AvaloniaProperty.Register<DropDownButton, bool>(nameof(IsDropDownContentOpen));
        
        /// <summary>
        /// Gets whether the drop-down is opened.
        /// </summary>
        public bool IsDropDownContentOpen
        {
            get => GetValue(IsDropDownContentOpenProperty);
            protected set => SetValue(IsDropDownContentOpenProperty, value);
        }

       /* protected override void OnClick(RoutedEventArgs e)
        {
            base.OnClick(e);

            if (DropDownContent != null && !IsDropDownContentOpen)
            {
                DropDownContent.PlacementMode = PlacementMode.Bottom;
                DropDownContent.PlacementTarget = this;
                DropDownContent.IsOpen = true;
                DropDownContent.Closed += DropDownContent_Closed;
                IsDropDownContentOpen = true;
            }
        }*/

        private void DropDownContent_Closed(object sender, EventArgs e)
        {
            ((Popup)sender).Closed -= DropDownContent_Closed;
            IsDropDownContentOpen = false;
        }
    }
}
