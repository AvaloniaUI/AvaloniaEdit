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
using System.Collections.Generic;
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
        /// Gets/Sets whether to show a visible glyph for spaces. The glyph displayed can be set via <see cref="ShowSpacesGlyph" />
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
                    OnPropertyChanged(nameof(ShowSpaces));
                }
            }
        }

        private string _showSpacesGlyph = "\u00B7";

        /// <summary>
        /// Gets/Sets the char to show when ShowSpaces option is enabled
        /// </summary>
        /// <remarks>The default value is <c>·</c>.</remarks>
        [DefaultValue("\u00B7")]
        public virtual string ShowSpacesGlyph
        {
            get { return _showSpacesGlyph; }
            set
            {
                if (_showSpacesGlyph != value)
                {
                    _showSpacesGlyph = value;
                    OnPropertyChanged(nameof(ShowSpacesGlyph));
                }
            }
        }

        private bool _showTabs;

        /// <summary>
        /// Gets/Sets whether to show a visible glyph for tab. The glyph displayed can be set via <see cref="ShowTabsGlyph" />
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
                    OnPropertyChanged(nameof(ShowTabs));
                }
            }
        }

        private string _showTabsGlyph = "\u2192";

        /// <summary>
        /// Gets/Sets the char to show when ShowTabs option is enabled
        /// </summary>
        /// <remarks>The default value is <c>→</c>.</remarks>
        [DefaultValue("\u2192")]
        public virtual string ShowTabsGlyph
        {
            get { return _showTabsGlyph; }
            set
            {
                if (_showTabsGlyph != value)
                {
                    _showTabsGlyph = value;
                    OnPropertyChanged(nameof(ShowTabsGlyph));
                }
            }
        }

        private bool _showEndOfLine;

        /// <summary>
        /// Gets/Sets whether to show EOL char at the end of lines. The glyphs displayed can be set via <see cref="EndOfLineCRLFGlyph" />, <see cref="EndOfLineCRGlyph" /> and <see cref="EndOfLineLFGlyph" />.
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
                    OnPropertyChanged(nameof(ShowEndOfLine));
                }
            }
        }

        private string _endOfLineCRLFGlyph = "¶";

        /// <summary>
        /// Gets/Sets the char to show for CRLF (\r\n) when ShowEndOfLine option is enabled
        /// </summary>
        /// <remarks>The default value is <c>¶</c>.</remarks>
        [DefaultValue("¶")]
        public virtual string EndOfLineCRLFGlyph
        {
            get { return _endOfLineCRLFGlyph; }
            set
            {
                if (_endOfLineCRLFGlyph != value)
                {
                    _endOfLineCRLFGlyph = value;
                    OnPropertyChanged(nameof(EndOfLineCRLFGlyph));
                }
            }
        }

        private string _endOfLineCRGlyph = "\\r";

        /// <summary>
        /// Gets/Sets the char to show for CR (\r) when ShowEndOfLine option is enabled
        /// </summary>
        /// <remarks>The default value is <c>\r</c>.</remarks>
        [DefaultValue("\\r")]
        public virtual string EndOfLineCRGlyph
        {
            get { return _endOfLineCRGlyph; }
            set
            {
                if (_endOfLineCRGlyph != value)
                {
                    _endOfLineCRGlyph = value;
                    OnPropertyChanged(nameof(EndOfLineCRGlyph));
                }
            }
        }

        private string _endOfLineLFGlyph = "\\n";

        /// <summary>
        /// Gets/Sets the char to show for LF (\n) when ShowEndOfLine option is enabled
        /// </summary>
        /// <remarks>The default value is <c>\n</c>.</remarks>
        [DefaultValue("\\n")]
        public virtual string EndOfLineLFGlyph
        {
            get { return _endOfLineLFGlyph; }
            set
            {
                if (_endOfLineLFGlyph != value)
                {
                    _endOfLineLFGlyph = value;
                    OnPropertyChanged(nameof(EndOfLineLFGlyph));
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
                    OnPropertyChanged(nameof(ShowBoxForControlCharacters));
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
                    OnPropertyChanged(nameof(EnableHyperlinks));
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
                    OnPropertyChanged(nameof(EnableEmailHyperlinks));
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
                    OnPropertyChanged(nameof(RequireControlModifierForHyperlinkClick));
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
                    OnPropertyChanged(nameof(IndentationSize));
                    OnPropertyChanged(nameof(IndentationString));
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
                    OnPropertyChanged(nameof(ConvertTabsToSpaces));
                    OnPropertyChanged(nameof(IndentationString));
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
                    OnPropertyChanged(nameof(CutCopyWholeLine));
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
                    OnPropertyChanged(nameof(AllowScrollBelowDocument));
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
                    OnPropertyChanged(nameof(WordWrapIndentation));
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
                    OnPropertyChanged(nameof(InheritWordWrapIndentation));
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
                    OnPropertyChanged(nameof(EnableRectangularSelection));
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
                    OnPropertyChanged(nameof(EnableTextDragDrop));
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
                    OnPropertyChanged(nameof(EnableVirtualSpace));
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
                    OnPropertyChanged(nameof(EnableImeSupport));
                }
            }
        }

        private bool _showColumnRulers;

        /// <summary>
        /// Gets/Sets whether the column rulers should be shown.
        /// </summary>
        [DefaultValue(false)]
        public virtual bool ShowColumnRulers
        {
            get { return _showColumnRulers; }
            set
            {
                if (_showColumnRulers != value)
                {
                    _showColumnRulers = value;
                    OnPropertyChanged(nameof(ShowColumnRulers));
                }
            }
        }

        private IEnumerable<int> _columnRulerPositions = new List<int>() { 80 };

        /// <summary>
        /// Gets/Sets the positions the column rulers should be shown.
        /// </summary>
        public virtual IEnumerable<int> ColumnRulerPositions
        {
            get { return _columnRulerPositions; }
            set
            {
                if (_columnRulerPositions != value)
                {
                    _columnRulerPositions = value;
                    OnPropertyChanged(nameof(ColumnRulerPositions));
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
                    OnPropertyChanged(nameof(HighlightCurrentLine));
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
                    OnPropertyChanged(nameof(HideCursorWhileTyping));
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
                    OnPropertyChanged(nameof(AllowToggleOverstrikeMode));
                }
            }
        }

        private bool _extendSelectionOnMouseUp = true;

        /// <summary>
        /// Gets/Sets if the mouse up event should extend the editor selection to the mouse position.
        /// </summary>
        [DefaultValue(true)]
        public bool ExtendSelectionOnMouseUp
        {
            get { return _extendSelectionOnMouseUp; }
            set
            {
                if (_extendSelectionOnMouseUp != value)
                {
                    _extendSelectionOnMouseUp = value;
                    OnPropertyChanged(nameof(ExtendSelectionOnMouseUp));
                }
            }
        }
    }
}
