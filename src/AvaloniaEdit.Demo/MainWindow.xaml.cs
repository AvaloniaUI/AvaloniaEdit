using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Rendering;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.TextMate.Extensions;
using AvaloniaEdit.TextMate.Grammars.Enums;
using AvaloniaEdit.TextMate.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AvaloniaEdit.Demo
{
    using Pair = KeyValuePair<int, IControl>;
    using ResourceLoader = TextMate.Grammars.ResourceLoader;

    public class MainWindow : Window
    {
        private readonly TextEditor _textEditor;
        private readonly Installation _textMateInstallation;
        private CompletionWindow _completionWindow;
        private OverloadInsightWindow _insightWindow;
        private Button _addControlBtn;
        private Button _clearControlBtn;
        private Button _changeThemeBtn;
        private ComboBox _syntaxModeCombo;
        private TextBlock _statusTextBlock;
        private ElementGenerator _generator = new ElementGenerator();

        public MainWindow()
        {
            InitializeComponent();

            _textEditor = this.FindControl<TextEditor>("Editor");
            _textEditor.Background = Brushes.Transparent;
            _textEditor.ShowLineNumbers = true;
            _textEditor.ContextMenu = new ContextMenu
            {
                Items = new List<MenuItem>
                {
                    new MenuItem { Header = "Copy", InputGesture = new KeyGesture(Key.C, KeyModifiers.Control) },
                    new MenuItem { Header = "Paste", InputGesture = new KeyGesture(Key.V, KeyModifiers.Control) },
                    new MenuItem { Header = "Cut", InputGesture = new KeyGesture(Key.X, KeyModifiers.Control) }
                }
            };
            _textEditor.TextArea.Background = this.Background;
            _textEditor.TextArea.TextEntered += TextEditor_TextArea_TextEntered;
            _textEditor.TextArea.TextEntering += TextEditor_TextArea_TextEntering;
            _textEditor.TextArea.IndentationStrategy = new Indentation.CSharp.CSharpIndentationStrategy();
            _textEditor.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            _textEditor.TextArea.RightClickMovesCaret = true;

            _addControlBtn = this.FindControl<Button>("addControlBtn");
            _addControlBtn.Click += AddControlBtn_Click;

            _clearControlBtn = this.FindControl<Button>("clearControlBtn");
            _clearControlBtn.Click += ClearControlBtn_Click;

            _changeThemeBtn = this.FindControl<Button>("changeThemeBtn");
            _changeThemeBtn.Click += ChangeThemeBtn_Click;

            _textEditor.TextArea.TextView.ElementGenerators.Add(_generator);

            _textMateInstallation = new Installation(_textEditor, ResourceLoader.SetupStorage(ThemeName.DarkPlus, GrammarName.CSharp));

            Language csharpLanguage = _textMateInstallation.RegistryOptions.Storage.GetLanguageByExtension(".cs");

            _syntaxModeCombo = this.FindControl<ComboBox>("syntaxModeCombo");
            _syntaxModeCombo.Items = _textMateInstallation.RegistryOptions.GetAvailableLanguages();
            _syntaxModeCombo.SelectedItem = csharpLanguage;
            _syntaxModeCombo.SelectionChanged += SyntaxModeCombo_SelectionChanged;

            string scopeName = _textMateInstallation.RegistryOptions.Storage.GetScopeByLanguageId(csharpLanguage.Id);

            _textEditor.Document = new TextDocument(Demo.Resources.ResourceLoader.LoadSampleFile(scopeName));
            _textMateInstallation.SetGrammarByLanguageId(csharpLanguage.Id);
            _statusTextBlock = this.Find<TextBlock>("StatusText");

            this.AddHandler(PointerWheelChangedEvent, (o, i) =>
            {
                if (i.KeyModifiers != KeyModifiers.Control) return;
                if (i.Delta.Y > 0) _textEditor.FontSize++;
                else _textEditor.FontSize = _textEditor.FontSize > 1 ? _textEditor.FontSize - 1 : 1;
            }, RoutingStrategies.Bubble, true);
        }

        private void Caret_PositionChanged(object sender, EventArgs e)
        {
            _statusTextBlock.Text = String.Format("Line {0} Column {1}", _textEditor.TextArea.Caret.Line, _textEditor.TextArea.Caret.Column);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _textMateInstallation.Dispose();
        }

        private void SyntaxModeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Language language = (Language)_syntaxModeCombo.SelectedItem;

            string scope = _textMateInstallation.RegistryOptions.Storage.GetScopeByLanguageId(language.Id);

            _textEditor.Document = new TextDocument(Demo.Resources.ResourceLoader.LoadSampleFile(scope));
            _textMateInstallation.SetGrammarByLanguageId(language.Id);
        }

        void ChangeThemeBtn_Click(object sender, RoutedEventArgs e)
        {
            var currentIndex = 0;
            for (int i = 0; i < _textMateInstallation.RegistryOptions.Storage.ThemeStorage.Themes.Count; i++)
            {
                if (_textMateInstallation.RegistryOptions.Storage.ThemeStorage.Themes.ElementAt(i).Value == 
                        _textMateInstallation.RegistryOptions.Storage.ThemeStorage.SelectedTheme)
                {
                    currentIndex = i;
                }
            }
            _textMateInstallation.SetTheme(_textMateInstallation.RegistryOptions.Storage.ThemeStorage.Themes
                .ElementAt(currentIndex + 1 < _textMateInstallation.RegistryOptions.Storage.ThemeStorage.Themes.Count ? currentIndex + 1 : 0).Value);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        void AddControlBtn_Click(object sender, RoutedEventArgs e)
        {
            _generator.controls.Add(new Pair(_textEditor.CaretOffset, new Button() { Content = "Click me" }));
            _textEditor.TextArea.TextView.Redraw();
        }

        void ClearControlBtn_Click(object sender, RoutedEventArgs e)
        {
            //TODO: delete elements using back key
            _generator.controls.Clear();
            _textEditor.TextArea.TextView.Redraw();
        }

        void TextEditor_TextArea_TextEntering(object sender, TextInputEventArgs e)
        {
            if (e.Text.Length > 0 && _completionWindow != null)
            {
                if (!char.IsLetterOrDigit(e.Text[0]))
                {
                    // Whenever a non-letter is typed while the completion window is open,
                    // insert the currently selected element.
                    _completionWindow.CompletionList.RequestInsertion(e);
                }
            }

            _insightWindow?.Hide();

            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        void TextEditor_TextArea_TextEntered(object sender, TextInputEventArgs e)
        {
            if (e.Text == ".")
            {

                _completionWindow = new CompletionWindow(_textEditor.TextArea);
                _completionWindow.Closed += (o, args) => _completionWindow = null;

                var data = _completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("Item1"));
                data.Add(new MyCompletionData("Item2"));
                data.Add(new MyCompletionData("Item3"));
                data.Add(new MyCompletionData("Item4"));
                data.Add(new MyCompletionData("Item5"));
                data.Add(new MyCompletionData("Item6"));
                data.Add(new MyCompletionData("Item7"));
                data.Add(new MyCompletionData("Item8"));
                data.Add(new MyCompletionData("Item9"));
                data.Add(new MyCompletionData("Item10"));
                data.Add(new MyCompletionData("Item11"));
                data.Add(new MyCompletionData("Item12"));
                data.Add(new MyCompletionData("Item13"));


                _completionWindow.Show();
            }
            else if (e.Text == "(")
            {
                _insightWindow = new OverloadInsightWindow(_textEditor.TextArea);
                _insightWindow.Closed += (o, args) => _insightWindow = null;

                _insightWindow.Provider = new MyOverloadProvider(new[]
                {
                    ("Method1(int, string)", "Method1 description"),
                    ("Method2(int)", "Method2 description"),
                    ("Method3(string)", "Method3 description"),
                });

                _insightWindow.Show();
            }
        }

        private class MyOverloadProvider : IOverloadProvider
        {
            private readonly IList<(string header, string content)> _items;
            private int _selectedIndex;

            public MyOverloadProvider(IList<(string header, string content)> items)
            {
                _items = items;
                SelectedIndex = 0;
            }

            public int SelectedIndex
            {
                get => _selectedIndex;
                set
                {
                    _selectedIndex = value;
                    OnPropertyChanged();
                    // ReSharper disable ExplicitCallerInfoArgument
                    OnPropertyChanged(nameof(CurrentHeader));
                    OnPropertyChanged(nameof(CurrentContent));
                    // ReSharper restore ExplicitCallerInfoArgument
                }
            }

            public int Count => _items.Count;
            public string CurrentIndexText => null;
            public object CurrentHeader => _items[SelectedIndex].header;
            public object CurrentContent => _items[SelectedIndex].content;

            public event PropertyChangedEventHandler PropertyChanged;

            private void OnPropertyChanged([CallerMemberName] string propertyName = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public class MyCompletionData : ICompletionData
        {
            public MyCompletionData(string text)
            {
                Text = text;
            }

            public IBitmap Image => null;

            public string Text { get; }

            // Use this property if you want to show a fancy UIElement in the list.
            public object Content => Text;

            public object Description => "Description for " + Text;

            public double Priority { get; } = 0;

            public void Complete(TextArea textArea, ISegment completionSegment,
                EventArgs insertionRequestEventArgs)
            {
                textArea.Document.Replace(completionSegment, Text);
            }
        }

        class ElementGenerator : VisualLineElementGenerator, IComparer<Pair>
        {
            public List<Pair> controls = new List<Pair>();

            /// <summary>
            /// Gets the first interested offset using binary search
            /// </summary>
            /// <returns>The first interested offset.</returns>
            /// <param name="startOffset">Start offset.</param>
            public override int GetFirstInterestedOffset(int startOffset)
            {
                int pos = controls.BinarySearch(new Pair(startOffset, null), this);
                if (pos < 0)
                    pos = ~pos;
                if (pos < controls.Count)
                    return controls[pos].Key;
                else
                    return -1;
            }

            public override VisualLineElement ConstructElement(int offset)
            {
                int pos = controls.BinarySearch(new Pair(offset, null), this);
                if (pos >= 0)
                    return new InlineObjectElement(0, controls[pos].Value);
                else
                    return null;
            }

            int IComparer<Pair>.Compare(Pair x, Pair y)
            {
                return x.Key.CompareTo(y.Key);
            }
        }
    }
}
