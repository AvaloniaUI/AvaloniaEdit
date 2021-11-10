using System;
using System.Linq;

using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public static class TextMate
    {
        public static Installation InstallTextMate(this TextEditor editor, Theme theme, IGrammar grammar = null)
        {
            return new Installation(editor, theme, grammar);
        }

        public class Installation
        {
            public Installation(TextEditor editor, Theme theme, IGrammar grammar)
            {
                _editor = editor;

                SetTheme(theme);
                SetGrammar(grammar);

                editor.DocumentChanged += OnEditorOnDocumentChanged;

                OnEditorOnDocumentChanged(editor, EventArgs.Empty);
            }

            public void SetGrammar(IGrammar grammar)
            {
                _grammar = grammar;

                GetOrCreateTransformer().SetGrammar(grammar);

                _editor.TextArea.TextView.Redraw();
            }

            public void SetTheme(Theme theme)
            {
                GetOrCreateTransformer().SetTheme(theme);

                _editorModel?.TokenizeViewPort();
            }

            public void Dispose()
            {
                _editor.DocumentChanged -= OnEditorOnDocumentChanged;

                DisposeTMModel(_tmModel);
            }

            void OnEditorOnDocumentChanged(object sender, EventArgs args)
            {
                DisposeTMModel(_tmModel);

                _editorModel = new TextEditorModel(_editor, _editor.Document);
                _tmModel = new TMModel(_editorModel);
                _tmModel.SetGrammar(_grammar);
                GetOrCreateTransformer().SetModel(_editor.Document, _editor.TextArea.TextView, _tmModel);
                _tmModel.AddModelTokensChangedListener(GetOrCreateTransformer());
            }

            TextMateColoringTransformer GetOrCreateTransformer()
            {
                var transformer = _editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>().FirstOrDefault();

                if (transformer is null)
                {
                    transformer = new TextMateColoringTransformer();

                    _editor.TextArea.TextView.LineTransformers.Add(transformer);
                }

                return transformer;
            }

            static void DisposeTMModel(TMModel tmModel)
            {
                if (tmModel == null)
                    return;

                tmModel.Dispose();
            }

            TextEditor _editor;
            TextEditorModel _editorModel;
            IGrammar _grammar;
            TMModel _tmModel;
        }
    }
}
