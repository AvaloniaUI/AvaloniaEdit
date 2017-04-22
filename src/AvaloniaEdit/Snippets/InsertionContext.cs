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
using System.Diagnostics.CodeAnalysis;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;

namespace AvaloniaEdit.Snippets
{
    /// <summary>
    /// Represents the context of a snippet insertion.
    /// </summary>
    public class InsertionContext
    {
        private enum Status
        {
            Insertion,
            RaisingInsertionCompleted,
            Interactive,
            RaisingDeactivated,
            Deactivated
        }

        private Status _currentStatus = Status.Insertion;

        /// <summary>
        /// Creates a new InsertionContext instance.
        /// </summary>
        public InsertionContext(TextArea textArea, int insertionPosition)
        {
            TextArea = textArea ?? throw new ArgumentNullException(nameof(textArea));
            Document = textArea.Document;
            SelectedText = textArea.Selection.GetText();
            InsertionPosition = insertionPosition;
            _startPosition = insertionPosition;

            DocumentLine startLine = Document.GetLineByOffset(insertionPosition);
            ISegment indentation = TextUtilities.GetWhitespaceAfter(Document, startLine.Offset);
            Indentation = Document.GetText(indentation.Offset, Math.Min(indentation.EndOffset, insertionPosition) - indentation.Offset);
            Tab = textArea.Options.IndentationString;

            LineTerminator = TextUtilities.GetNewLineFromDocument(Document, startLine.LineNumber);
        }

        /// <summary>
        /// Gets the text area.
        /// </summary>
        public TextArea TextArea { get; }

        /// <summary>
        /// Gets the text document.
        /// </summary>
        public TextDocument Document { get; }

        /// <summary>
        /// Gets the text that was selected before the insertion of the snippet.
        /// </summary>
        public string SelectedText { get; }

        /// <summary>
        /// Gets the indentation at the insertion position.
        /// </summary>
        public string Indentation { get; }

        /// <summary>
        /// Gets the indentation string for a single indentation level.
        /// </summary>
        public string Tab { get; }

        /// <summary>
        /// Gets the line terminator at the insertion position.
        /// </summary>
        public string LineTerminator { get; }

        /// <summary>
        /// Gets/Sets the insertion position.
        /// </summary>
        public int InsertionPosition { get; set; }

        private readonly int _startPosition;
        private AnchorSegment _wholeSnippetAnchor;
        private bool _deactivateIfSnippetEmpty;

        /// <summary>
        /// Gets the start position of the snippet insertion.
        /// </summary>
        public int StartPosition
        {
            get
            {
                if (_wholeSnippetAnchor != null)
                    return _wholeSnippetAnchor.Offset;
                return _startPosition;
            }
        }

        /// <summary>
        /// Inserts text at the insertion position and advances the insertion position.
        /// This method will add the current indentation to every line in <paramref name="text"/> and will
        /// replace newlines with the expected newline for the document.
        /// </summary>
        public void InsertText(string text)
        {
            if (_currentStatus != Status.Insertion)
                throw new InvalidOperationException();

            text = text?.Replace("\t", Tab) ?? throw new ArgumentNullException(nameof(text));

            using (Document.RunUpdate())
            {
                int textOffset = 0;
                SimpleSegment segment;
                while ((segment = NewLineFinder.NextNewLine(text, textOffset)) != SimpleSegment.Invalid)
                {
                    string insertString = text.Substring(textOffset, segment.Offset - textOffset)
                        + LineTerminator + Indentation;
                    Document.Insert(InsertionPosition, insertString);
                    InsertionPosition += insertString.Length;
                    textOffset = segment.EndOffset;
                }
                string remainingInsertString = text.Substring(textOffset);
                Document.Insert(InsertionPosition, remainingInsertString);
                InsertionPosition += remainingInsertString.Length;
            }
        }

        private readonly Dictionary<SnippetElement, IActiveElement> _elementMap = new Dictionary<SnippetElement, IActiveElement>();
        private readonly List<IActiveElement> _registeredElements = new List<IActiveElement>();

        /// <summary>
        /// Registers an active element. Elements should be registered during insertion and will be called back
        /// when insertion has completed.
        /// </summary>
        /// <param name="owner">The snippet element that created the active element.</param>
        /// <param name="element">The active element.</param>
        public void RegisterActiveElement(SnippetElement owner, IActiveElement element)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            if (element == null)
                throw new ArgumentNullException(nameof(element));
            if (_currentStatus != Status.Insertion)
                throw new InvalidOperationException();
            _elementMap.Add(owner, element);
            _registeredElements.Add(element);
        }

        /// <summary>
        /// Returns the active element belonging to the specified snippet element, or null if no such active element is found.
        /// </summary>
        public IActiveElement GetActiveElement(SnippetElement owner)
        {
            if (owner == null)
                throw new ArgumentNullException(nameof(owner));
            return _elementMap.TryGetValue(owner, out var element) ? element : null;
        }

        /// <summary>
        /// Gets the list of active elements.
        /// </summary>
        public IEnumerable<IActiveElement> ActiveElements => _registeredElements;

        /// <summary>
        /// Calls the <see cref="IActiveElement.OnInsertionCompleted"/> method on all registered active elements
        /// and raises the <see cref="InsertionCompleted"/> event.
        /// </summary>
        /// <param name="e">The EventArgs to use</param>
        [SuppressMessage("Microsoft.Design", "CA1030:UseEventsWhereAppropriate",
                                                         Justification = "There is an event and this method is raising it.")]
        public void RaiseInsertionCompleted(EventArgs e)
        {
            if (_currentStatus != Status.Insertion)
                throw new InvalidOperationException();
            if (e == null)
                e = EventArgs.Empty;

            _currentStatus = Status.RaisingInsertionCompleted;
            int endPosition = InsertionPosition;
            _wholeSnippetAnchor = new AnchorSegment(Document, _startPosition, endPosition - _startPosition);
            TextDocumentWeakEventManager.UpdateFinished.AddHandler(Document, OnUpdateFinished);
            _deactivateIfSnippetEmpty = (endPosition != _startPosition);

            foreach (IActiveElement element in _registeredElements)
            {
                element.OnInsertionCompleted();
            }
            InsertionCompleted?.Invoke(this, e);
            _currentStatus = Status.Interactive;
            if (_registeredElements.Count == 0)
            {
                // deactivate immediately if there are no interactive elements
                Deactivate(new SnippetEventArgs(DeactivateReason.NoActiveElements));
            }
            else
            {
                _myInputHandler = new SnippetInputHandler(this);
                // disable existing snippet input handlers - there can be only 1 active snippet
                foreach (TextAreaStackedInputHandler h in TextArea.StackedInputHandlers)
                {
                    if (h is SnippetInputHandler)
                        TextArea.PopStackedInputHandler(h);
                }
                TextArea.PushStackedInputHandler(_myInputHandler);
            }
        }

        private SnippetInputHandler _myInputHandler;

        /// <summary>
        /// Occurs when the all snippet elements have been inserted.
        /// </summary>
        public event EventHandler InsertionCompleted;

        /// <summary>
        /// Calls the <see cref="IActiveElement.Deactivate"/> method on all registered active elements.
        /// </summary>
        /// <param name="e">The EventArgs to use</param>
        public void Deactivate(SnippetEventArgs e)
        {
            if (_currentStatus == Status.Deactivated || _currentStatus == Status.RaisingDeactivated)
                return;
            if (_currentStatus != Status.Interactive)
                throw new InvalidOperationException("Cannot call Deactivate() until RaiseInsertionCompleted() has finished.");
            if (e == null)
                e = new SnippetEventArgs(DeactivateReason.Unknown);

            TextDocumentWeakEventManager.UpdateFinished.RemoveHandler(Document, OnUpdateFinished);
            _currentStatus = Status.RaisingDeactivated;
            TextArea.PopStackedInputHandler(_myInputHandler);
            foreach (IActiveElement element in _registeredElements)
            {
                element.Deactivate(e);
            }
            Deactivated?.Invoke(this, e);
            _currentStatus = Status.Deactivated;
        }

        /// <summary>
        /// Occurs when the interactive mode is deactivated.
        /// </summary>
        public event EventHandler<SnippetEventArgs> Deactivated;

        void OnUpdateFinished(object sender, EventArgs e)
        {
            // Deactivate if snippet is deleted. This is necessary for correctly leaving interactive
            // mode if Undo is pressed after a snippet insertion.
            if (_wholeSnippetAnchor.Length == 0 && _deactivateIfSnippetEmpty)
                Deactivate(new SnippetEventArgs(DeactivateReason.Deleted));
        }

        /// <summary>
        /// Adds existing segments as snippet elements.
        /// </summary>
        public void Link(ISegment mainElement, ISegment[] boundElements)
        {
            var main = new SnippetReplaceableTextElement { Text = Document.GetText(mainElement) };
            RegisterActiveElement(main, new ReplaceableActiveElement(this, mainElement.Offset, mainElement.EndOffset));
            foreach (var boundElement in boundElements)
            {
                var bound = new SnippetBoundElement { TargetElement = main };
                var start = Document.CreateAnchor(boundElement.Offset);
                start.MovementType = AnchorMovementType.BeforeInsertion;
                start.SurviveDeletion = true;
                var end = Document.CreateAnchor(boundElement.EndOffset);
                end.MovementType = AnchorMovementType.BeforeInsertion;
                end.SurviveDeletion = true;

                RegisterActiveElement(bound, new BoundActiveElement(this, main, bound, new AnchorSegment(start, end)));
            }
        }
    }
}
