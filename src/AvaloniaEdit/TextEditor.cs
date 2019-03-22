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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using Avalonia;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.Utils;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Data;

namespace AvaloniaEdit
{
    /// <summary>
    /// The text editor control.
    /// Contains a scrollable TextArea.
    /// </summary>
    public class TextEditor : TemplatedControl, ITextEditorComponent
    {
        #region Constructors
        static TextEditor()
        {
            FocusableProperty.OverrideDefaultValue<TextEditor>(true);
            HorizontalScrollBarVisibilityProperty.OverrideDefaultValue<TextEditor>(ScrollBarVisibility.Auto);
            VerticalScrollBarVisibilityProperty.OverrideDefaultValue<TextEditor>(ScrollBarVisibility.Auto);

            OptionsProperty.Changed.Subscribe(OnOptionsChanged);
            DocumentProperty.Changed.Subscribe(OnDocumentChanged);
            SyntaxHighlightingProperty.Changed.Subscribe(OnSyntaxHighlightingChanged);
            IsReadOnlyProperty.Changed.Subscribe(OnIsReadOnlyChanged);
            IsModifiedProperty.Changed.Subscribe(OnIsModifiedChanged);
            ShowLineNumbersProperty.Changed.Subscribe(OnShowLineNumbersChanged);
            LineNumbersForegroundProperty.Changed.Subscribe(OnLineNumbersForegroundChanged);
        }

        /// <summary>
        /// Creates a new TextEditor instance.
        /// </summary>
        public TextEditor() : this(new TextArea())
        {
        }

        /// <summary>
        /// Creates a new TextEditor instance.
        /// </summary>
        protected TextEditor(TextArea textArea) : this(textArea, new TextDocument())
        {
            
        }

        protected TextEditor(TextArea textArea, TextDocument document)
        {
            TextArea = textArea ?? throw new ArgumentNullException(nameof(textArea));

            textArea.TextView.Services.AddService(this);

            SetValue(OptionsProperty, textArea.Options);
            SetValue(DocumentProperty, document);

            textArea[!BackgroundProperty] = this[!BackgroundProperty];
        }

        #endregion

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            base.OnGotFocus(e);
            TextArea.Focus();
            e.Handled = true;
        }

        #region Document property
        /// <summary>
        /// Document property.
        /// </summary>
        public static readonly AvaloniaProperty<TextDocument> DocumentProperty
            = TextView.DocumentProperty.AddOwner<TextEditor>();

        /// <summary>
        /// Gets/Sets the document displayed by the text editor.
        /// This is a dependency property.
        /// </summary>
        public TextDocument Document
        {
            get => GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        /// <summary>
        /// Occurs when the document property has changed.
        /// </summary>
        public event EventHandler DocumentChanged;

        /// <summary>
        /// Raises the <see cref="DocumentChanged"/> event.
        /// </summary>
        protected virtual void OnDocumentChanged(EventArgs e)
        {
            DocumentChanged?.Invoke(this, e);
        }

        private static void OnDocumentChanged(AvaloniaPropertyChangedEventArgs e)
        {
            (e.Sender as TextEditor)?.OnDocumentChanged((TextDocument)e.OldValue, (TextDocument)e.NewValue);
        }

        private void OnDocumentChanged(TextDocument oldValue, TextDocument newValue)
        {
            if (oldValue != null)
            {
                TextDocumentWeakEventManager.TextChanged.RemoveHandler(oldValue, OnTextChanged);
                PropertyChangedWeakEventManager.RemoveHandler(oldValue.UndoStack, OnUndoStackPropertyChangedHandler);
            }
            TextArea.Document = newValue;
            if (newValue != null)
            {
                TextDocumentWeakEventManager.TextChanged.AddHandler(newValue, OnTextChanged);
                PropertyChangedWeakEventManager.AddHandler(newValue.UndoStack, OnUndoStackPropertyChangedHandler);
            }
            OnDocumentChanged(EventArgs.Empty);
            OnTextChanged(EventArgs.Empty);
        }
        #endregion

        #region Options property

        /// <summary>
        /// Options property.
        /// </summary>
        public static readonly AvaloniaProperty<TextEditorOptions> OptionsProperty
            = TextView.OptionsProperty.AddOwner<TextEditor>();

        /// <summary>
        /// Gets/Sets the options currently used by the text editor.
        /// </summary>
        public TextEditorOptions Options
        {
            get => GetValue(OptionsProperty);
            set => SetValue(OptionsProperty, value);
        }

        /// <summary>
        /// Occurs when a text editor option has changed.
        /// </summary>
        public event PropertyChangedEventHandler OptionChanged;

        /// <summary>
        /// Raises the <see cref="OptionChanged"/> event.
        /// </summary>
        protected virtual void OnOptionChanged(PropertyChangedEventArgs e)
        {
            OptionChanged?.Invoke(this, e);
        }

        private static void OnOptionsChanged(AvaloniaPropertyChangedEventArgs e)
        {
            (e.Sender as TextEditor)?.OnOptionsChanged((TextEditorOptions)e.OldValue, (TextEditorOptions)e.NewValue);
        }

        private void OnOptionsChanged(TextEditorOptions oldValue, TextEditorOptions newValue)
        {
            if (oldValue != null)
            {
                PropertyChangedWeakEventManager.RemoveHandler(oldValue, OnPropertyChangedHandler);
            }
            TextArea.Options = newValue;
            if (newValue != null)
            {
                PropertyChangedWeakEventManager.AddHandler(newValue, OnPropertyChangedHandler);
            }
            OnOptionChanged(new PropertyChangedEventArgs(null));
        }

        private void OnPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            OnOptionChanged(e);

        }
        private void OnUndoStackPropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsOriginalFile")
            {
                HandleIsOriginalChanged(e);
            }
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            OnTextChanged(e);
        }

        #endregion

        #region Text property
        /// <summary>
        /// Gets/Sets the text of the current document.
        /// </summary>
        public string Text
        {
            get
            {
                var document = Document;
                return document != null ? document.Text : string.Empty;
            }
            set
            {
                var document = GetDocument();
                document.Text = value ?? string.Empty;
                // after replacing the full text, the caret is positioned at the end of the document
                // - reset it to the beginning.
                CaretOffset = 0;
                document.UndoStack.ClearAll();
            }
        }

        private TextDocument GetDocument()
        {
            var document = Document;
            if (document == null)
                throw ThrowUtil.NoDocumentAssigned();
            return document;
        }

        /// <summary>
        /// Occurs when the Text property changes.
        /// </summary>
        public event EventHandler TextChanged;

        /// <summary>
        /// Raises the <see cref="TextChanged"/> event.
        /// </summary>
        protected virtual void OnTextChanged(EventArgs e)
        {
            TextChanged?.Invoke(this, e);
        }
        #endregion

        #region TextArea / ScrollViewer properties

        protected override void OnTemplateApplied(TemplateAppliedEventArgs e)
        {
            base.OnTemplateApplied(e);
            ScrollViewer = (ScrollViewer)e.NameScope.Find("PART_ScrollViewer");
            ScrollViewer.Content = TextArea;
        }

        /// <summary>
        /// Gets the text area.
        /// </summary>
        public TextArea TextArea { get; }

        /// <summary>
        /// Gets the scroll viewer used by the text editor.
        /// This property can return null if the template has not been applied / does not contain a scroll viewer.
        /// </summary>
        internal ScrollViewer ScrollViewer { get; private set; }

        #endregion

        #region Syntax highlighting
        /// <summary>
        /// The <see cref="SyntaxHighlighting"/> property.
        /// </summary>
        public static readonly AvaloniaProperty<IHighlightingDefinition> SyntaxHighlightingProperty =
            AvaloniaProperty.Register<TextEditor, IHighlightingDefinition>("SyntaxHighlighting");


        /// <summary>
        /// Gets/sets the syntax highlighting definition used to colorize the text.
        /// </summary>
        public IHighlightingDefinition SyntaxHighlighting
        {
            get => GetValue(SyntaxHighlightingProperty);
            set => SetValue(SyntaxHighlightingProperty, value);
        }

        private IVisualLineTransformer _colorizer;

        private static void OnSyntaxHighlightingChanged(AvaloniaPropertyChangedEventArgs e)
        {
            (e.Sender as TextEditor)?.OnSyntaxHighlightingChanged(e.NewValue as IHighlightingDefinition);
        }

        private void OnSyntaxHighlightingChanged(IHighlightingDefinition newValue)
        {
            if (_colorizer != null)
            {
                TextArea.TextView.LineTransformers.Remove(_colorizer);
                _colorizer = null;
            }

            if (newValue != null)
            {
                _colorizer = CreateColorizer(newValue);
                if (_colorizer != null)
                {
                    TextArea.TextView.LineTransformers.Insert(0, _colorizer);
                }
            }
        }

        /// <summary>
        /// Creates the highlighting colorizer for the specified highlighting definition.
        /// Allows derived classes to provide custom colorizer implementations for special highlighting definitions.
        /// </summary>
        /// <returns></returns>
        protected virtual IVisualLineTransformer CreateColorizer(IHighlightingDefinition highlightingDefinition)
        {
            if (highlightingDefinition == null)
                throw new ArgumentNullException(nameof(highlightingDefinition));
            return new HighlightingColorizer(highlightingDefinition);
        }
        #endregion

        #region WordWrap
        /// <summary>
        /// Word wrap dependency property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> WordWrapProperty =
            AvaloniaProperty.Register<TextEditor, bool>("WordWrap");

        /// <summary>
        /// Specifies whether the text editor uses word wrapping.
        /// </summary>
        /// <remarks>
        /// Setting WordWrap=true has the same effect as setting HorizontalScrollBarVisibility=Disabled and will override the
        /// HorizontalScrollBarVisibility setting.
        /// </remarks>
        public bool WordWrap
        {
            get => GetValue(WordWrapProperty);
            set => SetValue(WordWrapProperty, value);
        }
        #endregion

        #region IsReadOnly
        /// <summary>
        /// IsReadOnly dependency property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<TextEditor, bool>("IsReadOnly");

        /// <summary>
        /// Specifies whether the user can change the text editor content.
        /// Setting this property will replace the
        /// <see cref="Editing.TextArea.ReadOnlySectionProvider">TextArea.ReadOnlySectionProvider</see>.
        /// </summary>
        public bool IsReadOnly
        {
            get => GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        private static void OnIsReadOnlyChanged(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is TextEditor editor)
            {
                if ((bool)e.NewValue)
                    editor.TextArea.ReadOnlySectionProvider = ReadOnlySectionDocument.Instance;
                else
                    editor.TextArea.ReadOnlySectionProvider = NoReadOnlySections.Instance;
            }
        }
        #endregion

        #region IsModified
        /// <summary>
        /// Dependency property for <see cref="IsModified"/>
        /// </summary>
        public static readonly AvaloniaProperty<bool> IsModifiedProperty =
            AvaloniaProperty.Register<TextEditor, bool>("IsModified");

        /// <summary>
        /// Gets/Sets the 'modified' flag.
        /// </summary>
        public bool IsModified
        {
            get => GetValue(IsModifiedProperty);
            set => SetValue(IsModifiedProperty, value);
        }

        private static void OnIsModifiedChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var editor = e.Sender as TextEditor;
            var document = editor?.Document;
            if (document != null)
            {
                var undoStack = document.UndoStack;
                if ((bool)e.NewValue)
                {
                    if (undoStack.IsOriginalFile)
                        undoStack.DiscardOriginalFileMarker();
                }
                else
                {
                    undoStack.MarkAsOriginalFile();
                }
            }
        }

        private void HandleIsOriginalChanged(PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsOriginalFile")
            {
                var document = Document;
                if (document != null)
                {
                    SetValue(IsModifiedProperty, (object)!document.UndoStack.IsOriginalFile);
                }
            }
        }
        #endregion

        #region ShowLineNumbers
        /// <summary>
        /// ShowLineNumbers dependency property.
        /// </summary>
        public static readonly AvaloniaProperty<bool> ShowLineNumbersProperty =
            AvaloniaProperty.Register<TextEditor, bool>("ShowLineNumbers");

        /// <summary>
        /// Specifies whether line numbers are shown on the left to the text view.
        /// </summary>
        public bool ShowLineNumbers
        {
            get => GetValue(ShowLineNumbersProperty);
            set => SetValue(ShowLineNumbersProperty, value);
        }

        private static void OnShowLineNumbersChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var editor = e.Sender as TextEditor;
            if (editor == null) return;

            var leftMargins = editor.TextArea.LeftMargins;
            if ((bool)e.NewValue)
            {
                var lineNumbers = new LineNumberMargin();
                var line = (Line)DottedLineMargin.Create();
                leftMargins.Insert(0, lineNumbers);
                leftMargins.Insert(1, line);
                var lineNumbersForeground = new Binding("LineNumbersForeground") { Source = editor };
                line.Bind(Shape.StrokeProperty, lineNumbersForeground);
                lineNumbers.Bind(ForegroundProperty, lineNumbersForeground);
            }
            else
            {
                for (var i = 0; i < leftMargins.Count; i++)
                {
                    if (leftMargins[i] is LineNumberMargin)
                    {
                        leftMargins.RemoveAt(i);
                        if (i < leftMargins.Count && DottedLineMargin.IsDottedLineMargin(leftMargins[i]))
                        {
                            leftMargins.RemoveAt(i);
                        }
                        break;
                    }
                }
            }
        }
        #endregion

        #region LineNumbersForeground
        /// <summary>
        /// LineNumbersForeground dependency property.
        /// </summary>
        public static readonly AvaloniaProperty<IBrush> LineNumbersForegroundProperty =
            AvaloniaProperty.Register<TextEditor, IBrush>("LineNumbersForeground", Brushes.Gray);

        /// <summary>
        /// Gets/sets the Brush used for displaying the foreground color of line numbers.
        /// </summary>
        public IBrush LineNumbersForeground
        {
            get => GetValue(LineNumbersForegroundProperty);
            set => SetValue(LineNumbersForegroundProperty, value);
        }

        private static void OnLineNumbersForegroundChanged(AvaloniaPropertyChangedEventArgs e)
        {
            var editor = e.Sender as TextEditor;
            var lineNumberMargin = editor?.TextArea.LeftMargins.FirstOrDefault(margin => margin is LineNumberMargin) as LineNumberMargin;

            lineNumberMargin?.SetValue(ForegroundProperty, e.NewValue);
        }
        #endregion

        #region TextBoxBase-like methods
        /// <summary>
        /// Appends text to the end of the document.
        /// </summary>
        public void AppendText(string textData)
        {
            var document = GetDocument();
            document.Insert(document.TextLength, textData);
        }

        /// <summary>
        /// Begins a group of document changes.
        /// </summary>
        public void BeginChange()
        {
            GetDocument().BeginUpdate();
        }

        /// <summary>
        /// Copies the current selection to the clipboard.
        /// </summary>
        public void Copy()
        {
            //Execute(ApplicationCommands.Copy);
        }

        /// <summary>
        /// Removes the current selection and copies it to the clipboard.
        /// </summary>
        public void Cut()
        {
            //Execute(ApplicationCommands.Cut);
        }

        /// <summary>
        /// Begins a group of document changes and returns an object that ends the group of document
        /// changes when it is disposed.
        /// </summary>
        public IDisposable DeclareChangeBlock()
        {
            return GetDocument().RunUpdate();
        }

        /// <summary>
        /// Removes the current selection without copying it to the clipboard.
        /// </summary>
        public void Delete()
        {
            //Execute(ApplicationCommands.Delete);
        }

        /// <summary>
        /// Ends the current group of document changes.
        /// </summary>
        public void EndChange()
        {
            GetDocument().EndUpdate();
        }

        /// <summary>
        /// Scrolls one line down.
        /// </summary>
        public void LineDown()
        {
            //if (scrollViewer != null)
            //    scrollViewer.LineDown();
        }

        /// <summary>
        /// Scrolls to the left.
        /// </summary>
        public void LineLeft()
        {
            //if (scrollViewer != null)
            //    scrollViewer.LineLeft();
        }

        /// <summary>
        /// Scrolls to the right.
        /// </summary>
        public void LineRight()
        {
            //if (scrollViewer != null)
            //    scrollViewer.LineRight();
        }

        /// <summary>
        /// Scrolls one line up.
        /// </summary>
        public void LineUp()
        {
            //if (scrollViewer != null)
            //    scrollViewer.LineUp();
        }

        /// <summary>
        /// Scrolls one page down.
        /// </summary>
        public void PageDown()
        {
            //if (scrollViewer != null)
            //    scrollViewer.PageDown();
        }

        /// <summary>
        /// Scrolls one page up.
        /// </summary>
        public void PageUp()
        {
            //if (scrollViewer != null)
            //    scrollViewer.PageUp();
        }

        /// <summary>
        /// Scrolls one page left.
        /// </summary>
        public void PageLeft()
        {
            //if (scrollViewer != null)
            //    scrollViewer.PageLeft();
        }

        /// <summary>
        /// Scrolls one page right.
        /// </summary>
        public void PageRight()
        {
            //if (scrollViewer != null)
            //    scrollViewer.PageRight();
        }

        /// <summary>
        /// Pastes the clipboard content.
        /// </summary>
        public void Paste()
        {
            //Execute(ApplicationCommands.Paste);
        }

        /// <summary>
        /// Redoes the most recent undone command.
        /// </summary>
        /// <returns>True is the redo operation was successful, false is the redo stack is empty.</returns>
        public bool Redo()
        {
            //if (CanExecute(ApplicationCommands.Redo))
            //{
            //    Execute(ApplicationCommands.Redo);
            //    return true;
            //}
            return false;
        }

        /// <summary>
        /// Scrolls to the end of the document.
        /// </summary>
        public void ScrollToEnd()
        {
            ApplyTemplate(); // ensure scrollViewer is created
            //if (scrollViewer != null)
            //    scrollViewer.ScrollToEnd();
        }

        /// <summary>
        /// Scrolls to the start of the document.
        /// </summary>
        public void ScrollToHome()
        {
            ApplyTemplate(); // ensure scrollViewer is created
            //if (scrollViewer != null)
            //    scrollViewer.ScrollToHome();
        }

        /// <summary>
        /// Scrolls to the specified position in the document.
        /// </summary>
        public void ScrollToHorizontalOffset(double offset)
        {
            ApplyTemplate(); // ensure scrollViewer is created
            //if (scrollViewer != null)
            //    scrollViewer.ScrollToHorizontalOffset(offset);
        }

        /// <summary>
        /// Scrolls to the specified position in the document.
        /// </summary>
        public void ScrollToVerticalOffset(double offset)
        {
            ApplyTemplate(); // ensure scrollViewer is created
            //if (scrollViewer != null)
            //    scrollViewer.ScrollToVerticalOffset(offset);
        }

        /// <summary>
        /// Selects the entire text.
        /// </summary>
        public void SelectAll()
        {
            //Execute(ApplicationCommands.SelectAll);
        }

        /// <summary>
        /// Undoes the most recent command.
        /// </summary>
        /// <returns>True is the undo operation was successful, false is the undo stack is empty.</returns>
        public bool Undo()
        {
            //if (CanExecute(ApplicationCommands.Undo))
            //{
            //    Execute(ApplicationCommands.Undo);
            //    return true;
            //}
            return false;
        }

        /// <summary>
        /// Gets if the most recent undone command can be redone.
        /// </summary>
        public bool CanRedo => false;
        //{
        //    get { return CanExecute(ApplicationCommands.Redo); }
        //}

        /// <summary>
        /// Gets if the most recent command can be undone.
        /// </summary>
        public bool CanUndo => false;
        //{
        //    get { return CanExecute(ApplicationCommands.Undo); }
        //}

        /// <summary>
        /// Gets the vertical size of the document.
        /// </summary>
        public double ExtentHeight => ScrollViewer?.Extent.Height ?? 0;

        /// <summary>
        /// Gets the horizontal size of the current document region.
        /// </summary>
        public double ExtentWidth => ScrollViewer?.Extent.Width ?? 0;

        /// <summary>
        /// Gets the horizontal size of the viewport.
        /// </summary>
        public double ViewportHeight => ScrollViewer?.Viewport.Height ?? 0;

        /// <summary>
        /// Gets the horizontal size of the viewport.
        /// </summary>
        public double ViewportWidth => ScrollViewer?.Viewport.Width ?? 0;

        /// <summary>
        /// Gets the vertical scroll position.
        /// </summary>
        public double VerticalOffset => ScrollViewer?.Offset.Y ?? 0;

        /// <summary>
        /// Gets the horizontal scroll position.
        /// </summary>
        public double HorizontalOffset => ScrollViewer?.Offset.X ?? 0;

        #endregion

        #region TextBox methods
        /// <summary>
        /// Gets/Sets the selected text.
        /// </summary>
        public string SelectedText
        {
            get
            {
                var textArea = TextArea;
                // We'll get the text from the whole surrounding segment.
                // This is done to ensure that SelectedText.Length == SelectionLength.
                if (textArea?.Document != null && !textArea.Selection.IsEmpty)
                    return textArea.Document.GetText(textArea.Selection.SurroundingSegment);
                return string.Empty;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));
                var textArea = TextArea;
                if (textArea?.Document != null)
                {
                    var offset = SelectionStart;
                    var length = SelectionLength;
                    textArea.Document.Replace(offset, length, value);
                    // keep inserted text selected
                    textArea.Selection = Selection.Create(textArea, offset, offset + value.Length);
                }
            }
        }

        /// <summary>
        /// Gets/sets the caret position.
        /// </summary>
        public int CaretOffset
        {
            get
            {
                var textArea = TextArea;
                if (textArea != null)
                    return textArea.Caret.Offset;
                return 0;
            }
            set
            {
                var textArea = TextArea;
                if (textArea != null && textArea.Caret.Offset != value)
                    textArea.Caret.Offset = value;
            }
        }

        /// <summary>
        /// Gets/sets the start position of the selection.
        /// </summary>
        public int SelectionStart
        {
            get
            {
                var textArea = TextArea;
                if (textArea != null)
                {
                    if (textArea.Selection.IsEmpty)
                        return textArea.Caret.Offset;
                    return textArea.Selection.SurroundingSegment.Offset;
                }
                return 0;
            }
            set => Select(value, SelectionLength);
        }

        /// <summary>
        /// Gets/sets the length of the selection.
        /// </summary>
        public int SelectionLength
        {
            get
            {
                var textArea = TextArea;
                if (textArea != null && !textArea.Selection.IsEmpty)
                    return textArea.Selection.SurroundingSegment.Length;
                return 0;
            }
            set => Select(SelectionStart, value);
        }

        /// <summary>
        /// Selects the specified text section.
        /// </summary>
        public void Select(int start, int length)
        {
            var documentLength = Document?.TextLength ?? 0;
            if (start < 0 || start > documentLength)
                throw new ArgumentOutOfRangeException(nameof(start), start, "Value must be between 0 and " + documentLength);
            if (length < 0 || start + length > documentLength)
                throw new ArgumentOutOfRangeException(nameof(length), length, "Value must be between 0 and " + (documentLength - start));
            TextArea.Selection = Selection.Create(TextArea, start, start + length);
            TextArea.Caret.Offset = start + length;
        }

        /// <summary>
        /// Gets the number of lines in the document.
        /// </summary>
        public int LineCount
        {
            get
            {
                var document = Document;
                if (document != null)
                    return document.LineCount;
                return 1;
            }
        }

        /// <summary>
        /// Clears the text.
        /// </summary>
        public void Clear()
        {
            Text = string.Empty;
        }
        #endregion

        #region Loading from stream
        /// <summary>
        /// Loads the text from the stream, auto-detecting the encoding.
        /// </summary>
        /// <remarks>
        /// This method sets <see cref="IsModified"/> to false.
        /// </remarks>
        public void Load(Stream stream)
        {
            using (var reader = FileReader.OpenStream(stream, Encoding ?? Encoding.UTF8))
            {
                Text = reader.ReadToEnd();
                SetValue(EncodingProperty, (object)reader.CurrentEncoding);
            }
            SetValue(IsModifiedProperty, (object)false);
        }

        /// <summary>
        /// Loads the text from the stream, auto-detecting the encoding.
        /// </summary>
        public void Load(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));

            // TODO:load
            //using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            //{
            //    Load(fs);
            //}
        }

        /// <summary>
        /// Encoding dependency property.
        /// </summary>
        public static readonly AvaloniaProperty<Encoding> EncodingProperty =
            AvaloniaProperty.Register<TextEditor, Encoding>("Encoding");

        /// <summary>
        /// Gets/sets the encoding used when the file is saved.
        /// </summary>
        /// <remarks>
        /// The <see cref="Load(Stream)"/> method autodetects the encoding of the file and sets this property accordingly.
        /// The <see cref="Save(Stream)"/> method uses the encoding specified in this property.
        /// </remarks>
        public Encoding Encoding
        {
            get => GetValue(EncodingProperty);
            set => SetValue(EncodingProperty, value);
        }

        /// <summary>
        /// Saves the text to the stream.
        /// </summary>
        /// <remarks>
        /// This method sets <see cref="IsModified"/> to false.
        /// </remarks>
        public void Save(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));
            var encoding = Encoding;
            var document = Document;
            var writer = encoding != null ? new StreamWriter(stream, encoding) : new StreamWriter(stream);
            document?.WriteTextTo(writer);
            writer.Flush();
            // do not close the stream
            SetValue(IsModifiedProperty, (object)false);
        }

        /// <summary>
        /// Saves the text to the file.
        /// </summary>
        public void Save(string fileName)
        {
            if (fileName == null)
                throw new ArgumentNullException(nameof(fileName));
            // TODO: save
            //using (FileStream fs = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None))
            //{
            //    Save(fs);
            //}
        }
        #endregion

        #region PointerHover events
        /// <summary>
        /// The PreviewPointerHover event.
        /// </summary>
        public static readonly RoutedEvent<PointerEventArgs> PreviewPointerHoverEvent =
            TextView.PreviewPointerHoverEvent;

        /// <summary>
        /// the pointerHover event.
        /// </summary>
        public static readonly RoutedEvent<PointerEventArgs> PointerHoverEvent =
            TextView.PointerHoverEvent;


        /// <summary>
        /// The PreviewPointerHoverStopped event.
        /// </summary>
        public static readonly RoutedEvent<PointerEventArgs> PreviewPointerHoverStoppedEvent =
            TextView.PreviewPointerHoverStoppedEvent;

        /// <summary>
        /// the pointerHoverStopped event.
        /// </summary>
        public static readonly RoutedEvent<PointerEventArgs> PointerHoverStoppedEvent =
            TextView.PointerHoverStoppedEvent;


        /// <summary>
        /// Occurs when the pointer has hovered over a fixed location for some time.
        /// </summary>
        public event EventHandler<PointerEventArgs> PreviewPointerHover
        {
            add => AddHandler(PreviewPointerHoverEvent, value);
            remove => RemoveHandler(PreviewPointerHoverEvent, value);
        }

        /// <summary>
        /// Occurs when the pointer has hovered over a fixed location for some time.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerHover
        {
            add => AddHandler(PointerHoverEvent, value);
            remove => RemoveHandler(PointerHoverEvent, value);
        }

        /// <summary>
        /// Occurs when the pointer had previously hovered but now started moving again.
        /// </summary>
        public event EventHandler<PointerEventArgs> PreviewPointerHoverStopped
        {
            add => AddHandler(PreviewPointerHoverStoppedEvent, value);
            remove => RemoveHandler(PreviewPointerHoverStoppedEvent, value);
        }

        /// <summary>
        /// Occurs when the pointer had previously hovered but now started moving again.
        /// </summary>
        public event EventHandler<PointerEventArgs> PointerHoverStopped
        {
            add => AddHandler(PointerHoverStoppedEvent, value);
            remove => RemoveHandler(PointerHoverStoppedEvent, value);
        }

        #endregion

        #region ScrollBarVisibility
        /// <summary>
        /// Dependency property for <see cref="HorizontalScrollBarVisibility"/>
        /// </summary>
        public static readonly AttachedProperty<ScrollBarVisibility> HorizontalScrollBarVisibilityProperty = ScrollViewer.HorizontalScrollBarVisibilityProperty.AddOwner<TextEditor>();

        /// <summary>
        /// Gets/Sets the horizontal scroll bar visibility.
        /// </summary>
        public ScrollBarVisibility HorizontalScrollBarVisibility
        {
            get => GetValue(HorizontalScrollBarVisibilityProperty);
            set => SetValue(HorizontalScrollBarVisibilityProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="VerticalScrollBarVisibility"/>
        /// </summary>
        public static readonly AttachedProperty<ScrollBarVisibility> VerticalScrollBarVisibilityProperty = ScrollViewer.VerticalScrollBarVisibilityProperty.AddOwner<TextEditor>();

        /// <summary>
        /// Gets/Sets the vertical scroll bar visibility.
        /// </summary>
        public ScrollBarVisibility VerticalScrollBarVisibility
        {
            get => GetValue(VerticalScrollBarVisibilityProperty);
            set => SetValue(VerticalScrollBarVisibilityProperty, value);
        }
        #endregion

        object IServiceProvider.GetService(Type serviceType)
        {
            return TextArea.GetService(serviceType);
        }

        /// <summary>
        /// Gets the text view position from a point inside the editor.
        /// </summary>
        /// <param name="point">The position, relative to top left
        /// corner of TextEditor control</param>
        /// <returns>The text view position, or null if the point is outside the document.</returns>
        public TextViewPosition? GetPositionFromPoint(Point point)
        {
            if (Document == null)
                return null;
            var textView = TextArea.TextView;
            return textView.GetPosition(this.TranslatePoint(point + new Point(textView.ScrollOffset.X, Math.Floor(textView.ScrollOffset.Y)), textView));
        }

        /// <summary>
        /// Scrolls to the specified line.
        /// This method requires that the TextEditor was already assigned a size (layout engine must have run prior).
        /// </summary>
        public void ScrollToLine(int line)
        {
            ScrollTo(line, -1);
        }

        /// <summary>
        /// Scrolls to the specified line/column.
        /// This method requires that the TextEditor was already assigned a size (layout engine must have run prior).
        /// </summary>
        public void ScrollTo(int line, int column)
        {
            //const double MinimumScrollPercentage = 0.3;

            //TextView textView = textArea.TextView;
            //TextDocument document = textView.Document;
            //if (scrollViewer != null && document != null)
            //{
            //    if (line < 1)
            //        line = 1;
            //    if (line > document.LineCount)
            //        line = document.LineCount;

            //    IScrollInfo scrollInfo = textView;
            //    if (!scrollInfo.CanHorizontallyScroll)
            //    {
            //        // Word wrap is enabled. Ensure that we have up-to-date info about line height so that we scroll
            //        // to the correct position.
            //        // This avoids that the user has to repeat the ScrollTo() call several times when there are very long lines.
            //        VisualLine vl = textView.GetOrConstructVisualLine(document.GetLineByNumber(line));
            //        double remainingHeight = scrollViewer.Viewport.Height / 2;
            //        while (remainingHeight > 0)
            //        {
            //            DocumentLine prevLine = vl.FirstDocumentLine.PreviousLine;
            //            if (prevLine == null)
            //                break;
            //            vl = textView.GetOrConstructVisualLine(prevLine);
            //            remainingHeight -= vl.Height;
            //        }
            //    }

            //    Point p = textArea.TextView.GetVisualPosition(new TextViewPosition(line, Math.Max(1, column)), VisualYPosition.LineMiddle);
            //    double verticalPos = p.Y - scrollViewer.ViewportHeight / 2;
            //    if (Math.Abs(verticalPos - scrollViewer.VerticalOffset) > MinimumScrollPercentage * scrollViewer.ViewportHeight)
            //    {
            //        scrollViewer.ScrollToVerticalOffset(Math.Max(0, verticalPos));
            //    }
            //    if (column > 0)
            //    {
            //        if (p.X > scrollViewer.ViewportWidth - Caret.MinimumDistanceToViewBorder * 2)
            //        {
            //            double horizontalPos = Math.Max(0, p.X - scrollViewer.ViewportWidth / 2);
            //            if (Math.Abs(horizontalPos - scrollViewer.HorizontalOffset) > MinimumScrollPercentage * scrollViewer.ViewportWidth)
            //            {
            //                scrollViewer.ScrollToHorizontalOffset(horizontalPos);
            //            }
            //        }
            //        else
            //        {
            //            scrollViewer.ScrollToHorizontalOffset(0);
            //        }
            //    }
            //}
        }
    }
}
