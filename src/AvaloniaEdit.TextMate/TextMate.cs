using System;
using System.Linq;

using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Registry;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public static class TextMate
    {
        public static void RegisterExceptionHandler(Action<Exception> handler)
        {
            _exceptionHandler = handler;
        }

        public static Installation InstallTextMate(
            this TextEditor editor,
            ThemeName theme,
            IGrammar grammar = null)
        {
            return new Installation(editor, theme, grammar);
        }

        public class Installation
        {
            private RegistryOptions _textMateRegistryOptions;
            private Registry _textMateRegistry;
            private TextEditor _editor;
            private TextEditorModel _editorModel;
            private IGrammar _grammar;
            private TMModel _tmModel;
            private TextMateColoringTransformer _transformer;

            public RegistryOptions RegistryOptions { get { return _textMateRegistryOptions; } }
            public TextEditorModel EditorModel { get { return _editorModel; } }

            public Installation(TextEditor editor, ThemeName defaultTheme, IGrammar grammar)
            {
                _textMateRegistryOptions = new RegistryOptions(defaultTheme);
                _textMateRegistry = new Registry(_textMateRegistryOptions);

                _editor = editor;

                SetTheme(defaultTheme);
                SetGrammar(grammar);

                editor.DocumentChanged += OnEditorOnDocumentChanged;

                OnEditorOnDocumentChanged(editor, EventArgs.Empty);
            }

            public void SetGrammarByLanguageId(string languageId)
            {
                string scopeName = _textMateRegistryOptions.GetScopeByLanguageId(languageId);
                SetGrammar((scopeName == null) ? null : _textMateRegistry.LoadGrammar(scopeName));
            }

            public void SetGrammar(IGrammar grammar)
            {
                _grammar = grammar;

                GetOrCreateTransformer().SetGrammar(grammar);

                _editor.TextArea.TextView.Redraw();
            }

            public void SetTheme(ThemeName themeName)
            {
                IRawTheme rawTheme = _textMateRegistryOptions.LoadTheme(themeName);

                _textMateRegistry.SetTheme(rawTheme);

                GetOrCreateTransformer().SetTheme(_textMateRegistry.GetTheme());

                _tmModel?.InvalidateLine(0);
                _editorModel?.TokenizeViewPort();
            }

            public void Dispose()
            {
                _editor.DocumentChanged -= OnEditorOnDocumentChanged;

                DisposeEditorModel(_editorModel);
                DisposeTMModel(_tmModel);
                DisposeTransformer(_transformer);
            }

            void OnEditorOnDocumentChanged(object sender, EventArgs args)
            {
                try
                {
                    DisposeEditorModel(_editorModel);
                    DisposeTMModel(_tmModel);

                    _editorModel = new TextEditorModel(_editor.TextArea.TextView, _editor.Document, _exceptionHandler);
                    _tmModel = new TMModel(_editorModel);
                    _tmModel.SetGrammar(_grammar);
                    _transformer = GetOrCreateTransformer();
                    _transformer.SetModel(_editor.Document, _tmModel);
                    _tmModel.AddModelTokensChangedListener(_transformer);
                }
                catch (Exception ex)
                {
                    _exceptionHandler?.Invoke(ex);
                }
            }

            TextMateColoringTransformer GetOrCreateTransformer()
            {
                var transformer = _editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>().FirstOrDefault();

                if (transformer is null)
                {
                    transformer = new TextMateColoringTransformer(_editor.TextArea.TextView);

                    _editor.TextArea.TextView.LineTransformers.Add(transformer);
                }

                return transformer;
            }

            static void DisposeTransformer(TextMateColoringTransformer transformer)
            {
                if (transformer == null)
                    return;

                transformer.Dispose();
            }

            static void DisposeTMModel(TMModel tmModel)
            {
                if (tmModel == null)
                    return;

                tmModel.Dispose();
            }

            static void DisposeEditorModel(TextEditorModel editorModel)
            {
                if (editorModel == null)
                    return;

                editorModel.Dispose();
            }
        }

        static Action<Exception> _exceptionHandler;
    }
}
