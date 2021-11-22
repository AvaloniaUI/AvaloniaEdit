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
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;

namespace AvaloniaEdit.Search
{
    /// <summary>
    /// Provides search functionality for AvalonEdit. It is displayed in the top-right corner of the TextArea.
    /// </summary>
    public class SearchPanel : TemplatedControl, IRoutedCommandBindable
    {
        private TextArea _textArea;
        private SearchInputHandler _handler;
        private TextDocument _currentDocument;
        private SearchResultBackgroundRenderer _renderer;
        private TextBox _searchTextBox;
        private TextEditor _textEditor { get; set; }
        private Border _border;

        #region DependencyProperties
        /// <summary>
        /// Dependency property for <see cref="UseRegex"/>.
        /// </summary>
        public static readonly StyledProperty<bool> UseRegexProperty =
            AvaloniaProperty.Register<SearchPanel, bool>(nameof(UseRegex));

        /// <summary>
        /// Gets/sets whether the search pattern should be interpreted as regular expression.
        /// </summary>
        public bool UseRegex
        {
            get => GetValue(UseRegexProperty);
            set => SetValue(UseRegexProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="MatchCase"/>.
        /// </summary>
        public static readonly StyledProperty<bool> MatchCaseProperty =
            AvaloniaProperty.Register<SearchPanel, bool>(nameof(MatchCase));

        /// <summary>
        /// Gets/sets whether the search pattern should be interpreted case-sensitive.
        /// </summary>
        public bool MatchCase
        {
            get => GetValue(MatchCaseProperty);
            set => SetValue(MatchCaseProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="WholeWords"/>.
        /// </summary>
        public static readonly StyledProperty<bool> WholeWordsProperty =
            AvaloniaProperty.Register<SearchPanel, bool>(nameof(WholeWords));

        /// <summary>
        /// Gets/sets whether the search pattern should only match whole words.
        /// </summary>
        public bool WholeWords
        {
            get => GetValue(WholeWordsProperty);
            set => SetValue(WholeWordsProperty, value);
        }

        /// <summary>
        /// Dependency property for <see cref="SearchPattern"/>.
        /// </summary>
        public static readonly StyledProperty<string> SearchPatternProperty =
            AvaloniaProperty.Register<SearchPanel, string>(nameof(SearchPattern), "");

        /// <summary>
        /// Gets/sets the search pattern.
        /// </summary>
        public string SearchPattern
        {
            get => GetValue(SearchPatternProperty);
            set => SetValue(SearchPatternProperty, value);
        }

        public static readonly StyledProperty<bool> IsReplaceModeProperty =
            AvaloniaProperty.Register<SearchPanel, bool>(nameof(IsReplaceMode));

        /// <summary>
        /// Checks if replacemode is allowed
        /// </summary>
        /// <returns>False if editor is not null and readonly</returns>
        private static bool ValidateReplaceMode(SearchPanel panel, bool v1)
        {
            if (panel._textEditor == null || !v1) return v1;
            return !panel._textEditor.IsReadOnly;
        }

        public bool IsReplaceMode
        {
            get => GetValue(IsReplaceModeProperty);
            set => SetValue(IsReplaceModeProperty, _textEditor?.IsReadOnly ?? false ? false : value);
        }

        public static readonly StyledProperty<string> ReplacePatternProperty =
            AvaloniaProperty.Register<SearchPanel, string>(nameof(ReplacePattern));

        public string ReplacePattern
        {
            get => GetValue(ReplacePatternProperty);
            set => SetValue(ReplacePatternProperty, value);
        }


        /// <summary>
        /// Dependency property for <see cref="MarkerBrush"/>.
        /// </summary>
        public static readonly StyledProperty<IBrush> MarkerBrushProperty =
            AvaloniaProperty.Register<SearchPanel, IBrush>(nameof(MarkerBrush), Brushes.LightGreen);

        /// <summary>
        /// Gets/sets the Brush used for marking search results in the TextView.
        /// </summary>
        public IBrush MarkerBrush
        {
            get => GetValue(MarkerBrushProperty);
            set => SetValue(MarkerBrushProperty, value);
        }

        #endregion

        private static void MarkerBrushChangedCallback(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is SearchPanel panel)
            {
                panel._renderer.MarkerBrush = (IBrush)e.NewValue;
            }
        }

        private ISearchStrategy _strategy;

        private static void SearchPatternChangedCallback(AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Sender is SearchPanel panel)
            {
                panel.ValidateSearchText();
                panel.UpdateSearch();
            }
        }

        private void UpdateSearch()
        {
            // only reset as long as there are results
            // if no results are found, the "no matches found" message should not flicker.
            // if results are found by the next run, the message will be hidden inside DoSearch ...
            if (_renderer.CurrentResults.Any())
                _messageView.IsOpen = false;
            _strategy = SearchStrategyFactory.Create(SearchPattern ?? "", !MatchCase, WholeWords, UseRegex ? SearchMode.RegEx : SearchMode.Normal);
            OnSearchOptionsChanged(new SearchOptionsChangedEventArgs(SearchPattern, MatchCase, UseRegex, WholeWords));
            DoSearch(true);
        }

        static SearchPanel()
        {
            UseRegexProperty.Changed.Subscribe(SearchPatternChangedCallback);
            MatchCaseProperty.Changed.Subscribe(SearchPatternChangedCallback);
            WholeWordsProperty.Changed.Subscribe(SearchPatternChangedCallback);
            SearchPatternProperty.Changed.Subscribe(SearchPatternChangedCallback);

            MarkerBrushProperty.Changed.Subscribe(MarkerBrushChangedCallback);
        }

        /// <summary>
        /// Creates a new SearchPanel.
        /// </summary>
        private SearchPanel()
        {
        }

        /// <summary>
        /// Creates a SearchPanel and installs it to the TextEditor's TextArea.
        /// </summary>
        /// <remarks>This is a convenience wrapper.</remarks>
        public static SearchPanel Install(TextEditor editor)
        {
            if (editor == null) throw new ArgumentNullException(nameof(editor));
            SearchPanel searchPanel = Install(editor.TextArea);
            searchPanel._textEditor = editor;
            return searchPanel;
        }

        /// <summary>
        /// Creates a SearchPanel and installs it to the TextArea.
        /// </summary>
        public static SearchPanel Install(TextArea textArea)
        {
            if (textArea == null) throw new ArgumentNullException(nameof(textArea));
            var panel = new SearchPanel();
            panel.AttachInternal(textArea);
            panel._handler = new SearchInputHandler(textArea, panel);
            textArea.DefaultInputHandler.NestedInputHandlers.Add(panel._handler);
            ((ISetLogicalParent)panel).SetParent(textArea);

            return panel;
        }

        /// <summary>
        /// Adds the commands used by SearchPanel to the given CommandBindingCollection.
        /// </summary>
        public void RegisterCommands(ICollection<RoutedCommandBinding> commandBindings)
        {
            _handler.RegisterGlobalCommands(commandBindings);
        }

        /// <summary>
        /// Removes the SearchPanel from the TextArea.
        /// </summary>
        public void Uninstall()
        {
            Close();
            _textArea.DocumentChanged -= TextArea_DocumentChanged;
            if (_currentDocument != null)
                _currentDocument.TextChanged -= TextArea_Document_TextChanged;
            _textArea.DefaultInputHandler.NestedInputHandlers.Remove(_handler);
        }

        private void AttachInternal(TextArea textArea)
        {
            _textArea = textArea;

            _renderer = new SearchResultBackgroundRenderer();
            _currentDocument = textArea.Document;
            if (_currentDocument != null)
                _currentDocument.TextChanged += TextArea_Document_TextChanged;
            textArea.DocumentChanged += TextArea_DocumentChanged;
            KeyDown += SearchLayerKeyDown;

            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.FindNext, (sender, e) => FindNext()));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.FindPrevious, (sender, e) => FindPrevious()));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.CloseSearchPanel, (sender, e) => Close()));

            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Find, (sender, e) =>
            {
                IsReplaceMode = false;
                Reactivate();
            }));
            CommandBindings.Add(new RoutedCommandBinding(ApplicationCommands.Replace, (sender, e) => IsReplaceMode = true));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.ReplaceNext, (sender, e) => ReplaceNext(), (sender, e) => e.CanExecute = IsReplaceMode));
            CommandBindings.Add(new RoutedCommandBinding(SearchCommands.ReplaceAll, (sender, e) => ReplaceAll(), (sender, e) => e.CanExecute = IsReplaceMode));

            IsClosed = true;
        }

        private void TextArea_DocumentChanged(object sender, EventArgs e)
        {
            if (_currentDocument != null)
                _currentDocument.TextChanged -= TextArea_Document_TextChanged;
            _currentDocument = _textArea.Document;
            if (_currentDocument != null)
            {
                _currentDocument.TextChanged += TextArea_Document_TextChanged;
                DoSearch(false);
            }
        }

        private void TextArea_Document_TextChanged(object sender, EventArgs e)
        {
            DoSearch(false);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _border = e.NameScope.Find<Border>("PART_Border");
            _searchTextBox = e.NameScope.Find<TextBox>("PART_searchTextBox");
            _messageView = e.NameScope.Find<Popup>("PART_MessageView");
            _messageViewContent = e.NameScope.Find<ContentPresenter>("PART_MessageContent");
        }

        private void ValidateSearchText()
        {
            if (_searchTextBox == null)
                return;

            try
            {
                UpdateSearch();
                _validationError = null;
            }
            catch (SearchPatternException ex)
            {
                _validationError = ex.Message;
            }
        }

        /// <summary>
        /// Reactivates the SearchPanel by setting the focus on the search box and selecting all text.
        /// </summary>
        public void Reactivate()
        {
            if (_searchTextBox == null)
                return;
            _searchTextBox.Focus();
            _searchTextBox.SelectionStart = 0;
            _searchTextBox.SelectionEnd = _searchTextBox.Text?.Length ?? 0;
        }

        /// <summary>
        /// Moves to the next occurrence in the file.
        /// </summary>
        public void FindNext()
        {
            var result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(_textArea.Caret.Offset + 1) ??
                         _renderer.CurrentResults.FirstSegment;
            if (result != null)
            {
                SelectResult(result);
            }
        }

        /// <summary>
        /// Moves to the previous occurrence in the file.
        /// </summary>
        public void FindPrevious()
        {
            var result = _renderer.CurrentResults.FindFirstSegmentWithStartAfter(_textArea.Caret.Offset);
            if (result != null)
                result = _renderer.CurrentResults.GetPreviousSegment(result);
            if (result == null)
                result = _renderer.CurrentResults.LastSegment;
            if (result != null)
            {
                SelectResult(result);
            }
        }

        public void ReplaceNext()
        {
            if (!IsReplaceMode) return;

            FindNext();
            if (!_textArea.Selection.IsEmpty)
            {
                _textArea.Selection.ReplaceSelectionWithText(ReplacePattern ?? string.Empty);
            }

            UpdateSearch();
        }

        public void ReplaceAll()
        {
            if (!IsReplaceMode) return;

            var replacement = ReplacePattern ?? string.Empty;
            var document = _textArea.Document;
            using (document.RunUpdate())
            {
                var segments = _renderer.CurrentResults.OrderByDescending(x => x.EndOffset).ToArray();
                foreach (var textSegment in segments)
                {
                    document.Replace(textSegment.StartOffset, textSegment.Length,
                        new StringTextSource(replacement));
                }
            }
        }

        private Popup _messageView;
        private ContentPresenter _messageViewContent;
        private string _validationError;

        private void DoSearch(bool changeSelection)
        {
            if (IsClosed)
                return;
            _renderer.CurrentResults.Clear();

            if (!string.IsNullOrEmpty(SearchPattern))
            {
                var offset = _textArea.Caret.Offset;
                if (changeSelection)
                {
                    _textArea.ClearSelection();
                }

                // We cast from ISearchResult to SearchResult; this is safe because we always use the built-in strategy
                foreach (var result in _strategy.FindAll(_textArea.Document, 0, _textArea.Document.TextLength).Cast<SearchResult>())
                {
                    if (changeSelection && result.StartOffset >= offset)
                    {
                        SelectResult(result);
                        changeSelection = false;
                    }
                    _renderer.CurrentResults.Add(result);
                }
            }

            if (_messageView != null)
            {
                if (!_renderer.CurrentResults.Any())
                {
                    _messageViewContent.Content = SR.SearchNoMatchesFoundText;
                    _messageView.PlacementTarget = _searchTextBox;
                    _messageView.IsOpen = true;
                }
                else
                    _messageView.IsOpen = false;
            }

            _textArea.TextView.InvalidateLayer(KnownLayer.Selection);
        }

        private void SelectResult(TextSegment result)
        {
            _textArea.Caret.Offset = result.StartOffset;
            _textArea.Selection = Selection.Create(_textArea, result.StartOffset, result.EndOffset);

            double distanceToViewBorder = _border == null ?
                Caret.MinimumDistanceToViewBorder :
                _border.Bounds.Height + _textArea.TextView.DefaultLineHeight;
            _textArea.Caret.BringCaretToView(distanceToViewBorder);

            // show caret even if the editor does not have the Keyboard Focus
            _textArea.Caret.Show();
        }

        private void SearchLayerKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                    e.Handled = true;
                    if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        FindPrevious();
                    }
                    else
                    {
                        FindNext();
                    }
                    if (_searchTextBox != null)
                    {
                        if (_validationError != null)
                        {
                            _messageViewContent.Content = SR.SearchErrorText + " " + _validationError;
                            _messageView.PlacementTarget = _searchTextBox;
                            _messageView.IsOpen = true;
                        }
                    }
                    break;
                case Key.Escape:
                    e.Handled = true;
                    Close();
                    break;
            }
        }

        /// <summary>
        /// Gets whether the Panel is already closed.
        /// </summary>
        public bool IsClosed { get; private set; }

        /// <summary>
        /// Closes the SearchPanel.
        /// </summary>
        public void Close()
        {
            _textArea.RemoveChild(this);
            _messageView.IsOpen = false;
            _textArea.TextView.BackgroundRenderers.Remove(_renderer);
            
            IsClosed = true;

            // Clear existing search results so that the segments don't have to be maintained
            _renderer.CurrentResults.Clear();

            _textArea.Focus();
        }

        /// <summary>
        /// Opens the an existing search panel.
        /// </summary>
        public void Open()
        {
            if (!IsClosed) return;

            _textArea.AddChild(this);

            _textArea.TextView.BackgroundRenderers.Add(_renderer);
            IsClosed = false;
            DoSearch(false);
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            e.Handled = true;

            base.OnPointerPressed(e);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            Cursor = Cursor.Default;
            base.OnPointerMoved(e);
        }

        protected override void OnGotFocus(GotFocusEventArgs e)
        {
            e.Handled = true;

            base.OnGotFocus(e);
        }

        /// <summary>
        /// Fired when SearchOptions are changed inside the SearchPanel.
        /// </summary>
        public event EventHandler<SearchOptionsChangedEventArgs> SearchOptionsChanged;

        /// <summary>
        /// Raises the <see cref="SearchOptionsChanged" /> event.
        /// </summary>
        protected virtual void OnSearchOptionsChanged(SearchOptionsChangedEventArgs e)
        {
            SearchOptionsChanged?.Invoke(this, e);
        }

        public IList<RoutedCommandBinding> CommandBindings { get; } = new List<RoutedCommandBinding>();
    }

    /// <summary>
    /// EventArgs for <see cref="SearchPanel.SearchOptionsChanged"/> event.
    /// </summary>
    public class SearchOptionsChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the search pattern.
        /// </summary>
        public string SearchPattern { get; }

        /// <summary>
        /// Gets whether the search pattern should be interpreted case-sensitive.
        /// </summary>
        public bool MatchCase { get; }

        /// <summary>
        /// Gets whether the search pattern should be interpreted as regular expression.
        /// </summary>
        public bool UseRegex { get; }

        /// <summary>
        /// Gets whether the search pattern should only match whole words.
        /// </summary>
        public bool WholeWords { get; }

        /// <summary>
        /// Creates a new SearchOptionsChangedEventArgs instance.
        /// </summary>
        public SearchOptionsChangedEventArgs(string searchPattern, bool matchCase, bool useRegex, bool wholeWords)
        {
            SearchPattern = searchPattern;
            MatchCase = matchCase;
            UseRegex = useRegex;
            WholeWords = wholeWords;
        }
    }
}
