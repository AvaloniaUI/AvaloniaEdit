using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using AvaloniaEdit.CodeCompletion;
using AvaloniaEdit.Document;
using AvaloniaEdit.Editing;
using AvaloniaEdit.Highlighting;

namespace AvaloniaEdit.Demo
{
    public class MainWindow : Window
    {
        private readonly TextEditor _textEditor;
        private CompletionWindow _completionWindow;

        public MainWindow()
        {
            InitializeComponent();
            this.AttachDevTools();

            _textEditor = this.FindControl<TextEditor>("Editor");
            _textEditor.SyntaxHighlighting = HighlightingManager.Instance.GetDefinition("C#");
            _textEditor.TextArea.TextEntering += textEditor_TextArea_TextEntering;
            _textEditor.TextArea.TextEntered += textEditor_TextArea_TextEntered;
            var lineNumberMargin = new LineNumberMargin { Margin = new Thickness(0, 0, 10, 0) };
            TextBlock.SetForeground(lineNumberMargin, Brushes.Gray);
            _textEditor.TextArea.LeftMargins.Add(lineNumberMargin);
            _textEditor.TextArea.IndentationStrategy = new Indentation.CSharp.CSharpIndentationStrategy( );
        }

        private void InitializeComponent()
        {
            // TODO: iOS does not support dynamically loading assemblies
            // so we must refer to this resource DLL statically. For
            // now I am doing that here. But we need a better solution!!
            var theme = new Avalonia.Themes.Default.DefaultTheme();
            theme.FindResource("Button");
            AvaloniaXamlLoader.Load(this);
        }


        void textEditor_TextArea_TextEntering(object sender, TextInputEventArgs e)
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
            // Do not set e.Handled=true.
            // We still want to insert the character that was typed.
        }

        void textEditor_TextArea_TextEntered(object sender, TextInputEventArgs e)
        {
            if (e.Text == ".")
            {
                // Open code completion after the user has pressed dot:
                _completionWindow = new CompletionWindow(_textEditor.TextArea);

                var data = _completionWindow.CompletionList.CompletionData;
                data.Add(new MyCompletionData("Item1"));
                data.Add(new MyCompletionData("Item2"));
                data.Add(new MyCompletionData("Item3"));

                _completionWindow.Show();
                _completionWindow.Closed += (o, args) => _completionWindow = null;
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
    }
}
