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
            public RegistryOptions RegistryOptions { get { return _textMateRegistryOptions; } }

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

                DisposeTMModel(_tmModel);
            }

            void OnEditorOnDocumentChanged(object sender, EventArgs args)
            {
                try
                {
                    DisposeTMModel(_tmModel);

                    _editorModel = new TextEditorModel(_editor, _editor.Document, _exceptionHandler);
                    _tmModel = new TMModel(_editorModel);
                    _tmModel.SetGrammar(_grammar);
                    GetOrCreateTransformer().SetModel(_editor.Document, _editor.TextArea.TextView, _tmModel);
                    _tmModel.AddModelTokensChangedListener(GetOrCreateTransformer());
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

            RegistryOptions _textMateRegistryOptions;
            Registry _textMateRegistry;
            TextEditor _editor;
            TextEditorModel _editorModel;
            IGrammar _grammar;
            TMModel _tmModel;
        }

        static Action<Exception> _exceptionHandler;
    }
}
