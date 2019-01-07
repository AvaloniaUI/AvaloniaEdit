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
using AvaloniaEdit.Text;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace AvaloniaEdit.Rendering
{
    /// <summary>
    /// VisualLineElement that represents a piece of text and is a clickable link.
    /// </summary>
    public class VisualLineLinkText : VisualLineText
    {

        /// <summary>
        /// Routed event that should navigate to uri when the link is clicked.
        /// </summary>
        public static RoutedEvent<OpenUriRoutedEventArgs> OpenUriEvent { get; } = RoutedEvent.Register<VisualLineText,OpenUriRoutedEventArgs>(nameof(OpenUriEvent), RoutingStrategies.Bubble);

        /// <summary>
        /// Gets/Sets the URL that is navigated to when the link is clicked.
        /// </summary>
        public Uri NavigateUri { get; set; }

        /// <summary>
        /// Gets/Sets the window name where the URL will be opened.
        /// </summary>
        public string TargetName { get; set; }

        /// <summary>
        /// Gets/Sets whether the user needs to press Control to click the link.
        /// The default value is true.
        /// </summary>
        public bool RequireControlModifierForClick { get; set; }

        /// <summary>
        /// Creates a visual line text element with the specified length.
        /// It uses the <see cref="ITextRunConstructionContext.VisualLine"/> and its
        /// <see cref="VisualLineElement.RelativeTextOffset"/> to find the actual text string.
        /// </summary>
        public VisualLineLinkText(VisualLine parentVisualLine, int length) : base(parentVisualLine, length)
        {
            RequireControlModifierForClick = true;
        }

        /// <inheritdoc/>
        public override TextRun CreateTextRun(int startVisualColumn, ITextRunConstructionContext context)
        {
            TextRunProperties.ForegroundBrush = context.TextView.LinkTextForegroundBrush;
            TextRunProperties.BackgroundBrush = context.TextView.LinkTextBackgroundBrush;
            return base.CreateTextRun(startVisualColumn, context);
        }

        /// <summary>
        /// Gets whether the link is currently clickable.
        /// </summary>
        /// <remarks>Returns true when control is pressed; or when
        /// <see cref="RequireControlModifierForClick"/> is disabled.</remarks>
        protected virtual bool LinkIsClickable(InputModifiers modifiers)
        {
            if (NavigateUri == null)
                return false;
            if (RequireControlModifierForClick)
                return modifiers.HasFlag(InputModifiers.Control);
            return true;
        }

        /// <inheritdoc/>
        protected internal override void OnQueryCursor(PointerEventArgs e)
        {
            if (LinkIsClickable(e.InputModifiers))
            {
                if(e.Source is InputElement inputElement)
                {
                    inputElement.Cursor = new Cursor(StandardCursorType.Hand);
                }
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        protected internal override void OnPointerPressed(PointerPressedEventArgs e)
        {
            if (e.MouseButton == MouseButton.Left && !e.Handled && LinkIsClickable(e.InputModifiers))
            {
                var eventArgs = new OpenUriRoutedEventArgs(NavigateUri) { RoutedEvent = OpenUriEvent };
                e.Source.RaiseEvent(eventArgs);
                e.Handled = true;
            }
        }

        /// <inheritdoc/>
        protected override VisualLineText CreateInstance(int length)
        {
            return new VisualLineLinkText(ParentVisualLine, length)
            {
                NavigateUri = NavigateUri,
                TargetName = TargetName,
                RequireControlModifierForClick = RequireControlModifierForClick
            };
        }
    }

    /// <summary>
    /// Holds arguments for a <see cref="VisualLineLinkText.OpenUriEvent"/>.
    /// </summary>
    /// <example>
    /// This sample shows how to open link using system's web brower
    /// <code>
    /// VisualLineLinkText.OpenUriEvent.AddClassHandler<Window>(args => {
    ///     var link = args.Uri.ToString();
    ///     try
    ///     {
    ///         if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    ///         {
    ///             Process.Start(link);
    ///         }
    ///         else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    ///         {
    ///             Process.Start("open", link);
    ///         }
    ///         else
    ///         {
    ///             Process.Start("xdg-open", link);
    ///         }
    ///         }
    ///         catch (Exception)
    ///         {
    ///             // Process.Start can throw several errors (not all of them documented),
    ///             // just ignore all of them.
    ///         }
    ///     }
    /// });
    /// </code>
    /// </example>
    public sealed class OpenUriRoutedEventArgs : RoutedEventArgs
    {
        public Uri Uri { get; }

        public OpenUriRoutedEventArgs(Uri uri)
        {
            Uri = uri ?? throw new ArgumentNullException(nameof(uri));
        }
    }
}
