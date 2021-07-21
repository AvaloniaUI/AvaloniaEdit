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
using System.ComponentModel;
using System.Reflection;

namespace AvaloniaEdit
{
    /// <summary>
    /// A container for the text editor options.
    /// </summary>
    public class TextEditorOptions : INotifyPropertyChanged
    {
        #region ctor
        /// <summary>
        /// Initializes an empty instance of TextEditorOptions.
        /// </summary>
        public TextEditorOptions()
        {
        }

        /// <summary>
        /// Initializes a new instance of TextEditorOptions by copying all values
        /// from <paramref name="options"/> to the new instance.
        /// </summary>
        public TextEditorOptions(TextEditorOptions options)
        {
            // get all the fields in the class
            var fields = typeof(TextEditorOptions).GetRuntimeFields();

            // copy each value over to 'this'
            foreach (FieldInfo fi in fields)
            {
                if (!fi.IsStatic)
                    fi.SetValue(this, fi.GetValue(options));
            }
        }
        #endregion

        #region PropertyChanged handling
        /// <inheritdoc/>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            var args = new PropertyChangedEventArgs(propertyName);
            OnPropertyChanged(args);
        }

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }
        #endregion

        #region ShowSpaces / ShowTabs / ShowEndOfLine / ShowBoxForControlCharacters

        private bool _showSpaces;

        /// <summary>
        /// Gets/Sets whether to show · for spaces.
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        [DefaultValue(false)]
        public virtual bool ShowSpaces
        {
            get { return _showSpaces; }
            set
            {
                if (_showSpaces != value)
                {
                    _showSpaces = value;
                    OnPropertyChanged("ShowSpaces");
                }
            }
        }

        private bool _showTabs;

        /// <summary>
        /// Gets/Sets whether to show » for tabs.
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        [DefaultValue(false)]
        public virtual bool ShowTabs
        {
            get { return _showTabs; }
            set
            {
                if (_showTabs != value)
                {
                    _showTabs = value;
                    OnPropertyChanged("ShowTabs");
                }
            }
        }

        private bool _showEndOfLine;

        /// <summary>
        /// Gets/Sets whether to show ¶ at the end of lines.
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        [DefaultValue(false)]
        public virtual bool ShowEndOfLine
        {
            get { return _showEndOfLine; }
            set
            {
                if (_showEndOfLine != value)
                {
                    _showEndOfLine = value;
                    OnPropertyChanged("ShowEndOfLine");
                }
            }
        }

        private bool _showBoxForControlCharacters = true;

        /// <summary>
        /// Gets/Sets whether to show a box with the hex code for control characters.
        /// </summary>
        /// <remarks>The default value is <c>true</c>.</remarks>
        [DefaultValue(true)]
        public virtual bool ShowBoxForControlCharacters
        {
            get { return _showBoxForControlCharacters; }
            set
            {
                if (_showBoxForControlCharacters != value)
                {
                    _showBoxForControlCharacters = value;
                    OnPropertyChanged("ShowBoxForControlCharacters");
                }
            }
        }
        #endregion

        #region EnableHyperlinks

        private bool _enableHyperlinks = true;

        /// <summary>
        /// Gets/Sets whether to enable clickable hyperlinks in the editor.
        /// </summary>
        /// <remarks>The default value is <c>true</c>.</remarks>
        [DefaultValue(true)]
        public virtual bool EnableHyperlinks
        {
            get { return _enableHyperlinks; }
            set
            {
                if (_enableHyperlinks != value)
                {
                    _enableHyperlinks = value;
                    OnPropertyChanged("EnableHyperlinks");
                }
            }
        }

        private bool _enableEmailHyperlinks = true;

        /// <summary>
        /// Gets/Sets whether to enable clickable hyperlinks for e-mail addresses in the editor.
        /// </summary>
        /// <remarks>The default value is <c>true</c>.</remarks>
        [DefaultValue(true)]
        public virtual bool EnableEmailHyperlinks
        {
            get { return _enableEmailHyperlinks; }
            set
            {
                if (_enableEmailHyperlinks != value)
                {
                    _enableEmailHyperlinks = value;
                    OnPropertyChanged("EnableEMailHyperlinks");
                }
            }
        }

        private bool _requireControlModifierForHyperlinkClick = true;

        /// <summary>
        /// Gets/Sets whether the user needs to press Control to click hyperlinks.
        /// The default value is true.
        /// </summary>
        /// <remarks>The default value is <c>true</c>.</remarks>
        [DefaultValue(true)]
        public virtual bool RequireControlModifierForHyperlinkClick
        {
            get { return _requireControlModifierForHyperlinkClick; }
            set
            {
                if (_requireControlModifierForHyperlinkClick != value)
                {
                    _requireControlModifierForHyperlinkClick = value;
                    OnPropertyChanged("RequireControlModifierForHyperlinkClick");
                }
            }
        }
        #endregion

        #region TabSize / IndentationSize / ConvertTabsToSpaces / GetIndentationString
        // I'm using '_' prefixes for the fields here to avoid confusion with the local variables
        // in the methods below.
        // The fields should be accessed only by their property - the fields might not be used
        // if someone overrides the property.

        private int _indentationSize = 4;

        /// <summary>
        /// Gets/Sets the width of one indentation unit.
        /// </summary>
        /// <remarks>The default value is 4.</remarks>
        [DefaultValue(4)]
        public virtual int IndentationSize
        {
            get => _indentationSize;
            set
            {
                if (value < 1)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "value must be positive");
                // sanity check; a too large value might cause a crash internally much later
                // (it only crashed in the hundred thousands for me; but might crash earlier with larger fonts)
                if (value > 1000)
                    throw new ArgumentOutOfRangeException(nameof(value), value, "indentation size is too large");
                if (_indentationSize != value)
                {
                    _indentationSize = value;
                    OnPropertyChanged("IndentationSize");
                    OnPropertyChanged("IndentationString");
                }
            }
        }

        private bool _convertTabsToSpaces;

        /// <summary>
        /// Gets/Sets whether to use spaces for indentation instead of tabs.
        /// </summary>
        /// <remarks>The default value is <c>false</c>.</remarks>
        [DefaultValue(false)]
        public virtual bool ConvertTabsToSpaces
        {
            get { return _convertTabsToSpaces; }
            set
            {
                if (_convertTabsToSpaces != value)
                {
                    _convertTabsToSpaces = value;
                    OnPropertyChanged("ConvertTabsToSpaces");
                    OnPropertyChanged("IndentationString");
                }
            }
        }

        /// <summary>
        /// Gets the text used for indentation.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1721:PropertyNamesShouldNotMatchGetMethods")]
        public string IndentationString => GetIndentationString(1);

        /// <summary>
        /// Gets text required to indent from the specified <paramref name="column"/> to the next indentation level.
        /// </summary>
        public virtual string GetIndentationString(int column)
        {
            if (column < 1)
                throw new ArgumentOutOfRangeException(nameof(column), column, "Value must be at least 1.");
            int indentationSize = IndentationSize;
            if (ConvertTabsToSpaces)
            {
                return new string(' ', indentationSize - ((column - 1) % indentationSize));
            }
            else
            {
                return "\t";
            }
        }
        #endregion

        private bool _cutCopyWholeLine = true;

        /// <summary>
        /// Gets/Sets whether copying without a selection copies the whole current line.
        /// </summary>
        [DefaultValue(true)]
        public virtual bool CutCopyWholeLine
        {
            get { return _cutCopyWholeLine; }
            set
            {
                if (_cutCopyWholeLine != value)
                {
                    _cutCopyWholeLine = value;
                    OnPropertyChanged("CutCopyWholeLine");
                }
            }
        }

        private bool _allowScrollBelowDocument = true;

        /// <summary>
        /// Gets/Sets whether the user can scroll below the bottom of the document.
        /// The default value is true; but it a good idea to set this property to true when using folding.
        /// </summary>
        [DefaultValue(true)]
        public virtual bool AllowScrollBelowDocument
        {
            get { return _allowScrollBelowDocument; }
            set
            {
                if (_allowScrollBelowDocument != value)
                {
                    _allowScrollBelowDocument = value;
                    OnPropertyChanged("AllowScrollBelowDocument");
                }
            }
        }

        private double _wordWrapIndentation;

        /// <summary>
        /// Gets/Sets the indentation used for all lines except the first when word-wrapping.
        /// The default value is 0.
        /// </summary>
        [DefaultValue(0.0)]
        public virtual double WordWrapIndentation
        {
            get => _wordWrapIndentation;
            set
            {
                if (double.IsNaN(value) || double.IsInfinity(value))
                    throw new ArgumentOutOfRangeException(nameof(value), value, "value must not be NaN/infinity");
                if (value != _wordWrapIndentation)
                {
                    _wordWrapIndentation = value;
                    OnPropertyChanged("WordWrapIndentation");
                }
            }
        }

        private bool _inheritWordWrapIndentation = true;

        /// <summary>
        /// Gets/Sets whether the indentation is inherited from the first line when word-wrapping.
        /// The default value is true.
        /// </summary>
        /// <remarks>When combined with <see cref="WordWrapIndentation"/>, the inherited indentation is added to the word wrap indentation.</remarks>
        [DefaultValue(true)]
        public virtual bool InheritWordWrapIndentation
        {
            get { return _inheritWordWrapIndentation; }
            set
            {
                if (value != _inheritWordWrapIndentation)
                {
                    _inheritWordWrapIndentation = value;
                    OnPropertyChanged("InheritWordWrapIndentation");
                }
            }
        }

        private bool _enableRectangularSelection = true;

        /// <summary>
        /// Enables rectangular selection (press ALT and select a rectangle)
        /// </summary>
        [DefaultValue(true)]
        public bool EnableRectangularSelection
        {
            get { return _enableRectangularSelection; }
            set
            {
                if (_enableRectangularSelection != value)
                {
                    _enableRectangularSelection = value;
                    OnPropertyChanged("EnableRectangularSelection");
                }
            }
        }

        private bool _enableTextDragDrop = true;

        /// <summary>
        /// Enable dragging text within the text area.
        /// </summary>
        [DefaultValue(true)]
        public bool EnableTextDragDrop
        {
            get { return _enableTextDragDrop; }
            set
            {
                if (_enableTextDragDrop != value)
                {
                    _enableTextDragDrop = value;
                    OnPropertyChanged("EnableTextDragDrop");
                }
            }
        }

        private bool _enableVirtualSpace;

        /// <summary>
        /// Gets/Sets whether the user can set the caret behind the line ending
        /// (into "virtual space").
        /// Note that virtual space is always used (independent from this setting)
        /// when doing rectangle selections.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool EnableVirtualSpace
        {
            get { return _enableVirtualSpace; }
            set
            {
                if (_enableVirtualSpace != value)
                {
                    _enableVirtualSpace = value;
                    OnPropertyChanged("EnableVirtualSpace");
                }
            }
        }

        private bool _enableImeSupport = true;

        /// <summary>
        /// Gets/Sets whether the support for Input Method Editors (IME)
        /// for non-alphanumeric scripts (Chinese, Japanese, Korean, ...) is enabled.
        /// </summary>
        [DefaultValue(true)]
        public virtual bool EnableImeSupport
        {
            get { return _enableImeSupport; }
            set
            {
                if (_enableImeSupport != value)
                {
                    _enableImeSupport = value;
                    OnPropertyChanged("EnableImeSupport");
                }
            }
        }

        private bool _showColumnRuler;

        /// <summary>
        /// Gets/Sets whether the column ruler should be shown.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool ShowColumnRuler
        {
            get { return _showColumnRuler; }
            set
            {
                if (_showColumnRuler != value)
                {
                    _showColumnRuler = value;
                    OnPropertyChanged("ShowColumnRuler");
                }
            }
        }

        private int _columnRulerPosition = 80;

        /// <summary>
        /// Gets/Sets where the column ruler should be shown.
        /// </summary>
        [DefaultValue(80)]
        public virtual int ColumnRulerPosition
        {
            get { return _columnRulerPosition; }
            set
            {
                if (_columnRulerPosition != value)
                {
                    _columnRulerPosition = value;
                    OnPropertyChanged("ColumnRulerPosition");
                }
            }
        }

        private bool _highlightCurrentLine;

        /// <summary>
        /// Gets/Sets if current line should be shown.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool HighlightCurrentLine
        {
            get { return _highlightCurrentLine; }
            set
            {
                if (_highlightCurrentLine != value)
                {
                    _highlightCurrentLine = value;
                    OnPropertyChanged("HighlightCurrentLine");
                }
            }
        }

        private bool _hideCursorWhileTyping = true;

        /// <summary>
        /// Gets/Sets if mouse cursor should be hidden while user is typing.
        /// </summary>
        [DefaultValue(true)]
        public bool HideCursorWhileTyping
        {
            get { return _hideCursorWhileTyping; }
            set
            {
                if (_hideCursorWhileTyping != value)
                {
                    _hideCursorWhileTyping = value;
                    OnPropertyChanged("HideCursorWhileTyping");
                }
            }
        }

        private bool _allowToggleOverstrikeMode;

        /// <summary>
        /// Gets/Sets if the user is allowed to enable/disable overstrike mode.
        /// </summary>
        [DefaultValue(false)]
        public bool AllowToggleOverstrikeMode
        {
            get { return _allowToggleOverstrikeMode; }
            set
            {
                if (_allowToggleOverstrikeMode != value)
                {
                    _allowToggleOverstrikeMode = value;
                    OnPropertyChanged("AllowToggleOverstrikeMode");
                }
            }
        }
    }
}
