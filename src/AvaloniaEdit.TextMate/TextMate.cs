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
            IRegistryOptions registryOptions)
        {
            return new Installation(editor, registryOptions);
        }

        public class Installation
        {
            public Installation(TextEditor editor, IRegistryOptions registryOptions)
            {
                _textMateRegistry = new Registry(registryOptions);

                _editor = editor;

                SetTheme(registryOptions.GetDefaultTheme());

                editor.DocumentChanged += OnEditorOnDocumentChanged;

                OnEditorOnDocumentChanged(editor, EventArgs.Empty);
            }

            public void SetGrammar(string scopeName)
            {
                _grammar = _textMateRegistry.LoadGrammar(scopeName);

                GetOrCreateTransformer().SetGrammar(_grammar);

                _editor.TextArea.TextView.Redraw();
            }

            public void SetTheme(IRawTheme theme)
            {
                _textMateRegistry.SetTheme(theme);

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

            Registry _textMateRegistry;
            TextEditor _editor;
            TextEditorModel _editorModel;
            IGrammar _grammar;
            TMModel _tmModel;
        }

        static Action<Exception> _exceptionHandler;
    }
}
