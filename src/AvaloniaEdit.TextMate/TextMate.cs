using System;
using System.Collections.ObjectModel;
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
            IRegistryOptions registryOptions,
            bool initCurrentDocument = true)
        {
            return new Installation(editor, registryOptions, initCurrentDocument);
        }

        public class Installation
        {
            private IRegistryOptions _textMateRegistryOptions;
            private Registry _textMateRegistry;
            private TextEditor _editor;
            private TextEditorModel _editorModel;
            private IGrammar _grammar;
            private TMModel _tmModel;
            private TextMateColoringTransformer _transformer;
            private ReadOnlyDictionary<string, string> _themeColorsDictionary;
            public IRegistryOptions RegistryOptions { get { return _textMateRegistryOptions; } }
            public TextEditorModel EditorModel { get { return _editorModel; } }

            public event EventHandler<Installation> AppliedTheme;

            public Installation(TextEditor editor, IRegistryOptions registryOptions, bool initCurrentDocument = true)
            {
                _textMateRegistryOptions = registryOptions;
                _textMateRegistry = new Registry(registryOptions);

                _editor = editor;
                SetTheme(registryOptions.GetDefaultTheme());

                editor.DocumentChanged += OnEditorOnDocumentChanged;

                if (initCurrentDocument)
                {
                    OnEditorOnDocumentChanged(editor, EventArgs.Empty);
                }
            }

            public void SetGrammar(string scopeName)
            {
                SetGrammarInternal(_textMateRegistry.LoadGrammar(scopeName));
            }

            public void SetGrammarFile(string path)
            {
                SetGrammarInternal(_grammar = _textMateRegistry.LoadGrammarFromPathSync(path, 0, null));
            }

            private void SetGrammarInternal(IGrammar grammar)
            {
                _grammar = grammar;
                GetOrCreateTransformer().SetGrammar(_grammar);
                _editor.TextArea.TextView.Redraw();
            }

            public bool TryGetThemeColor(string colorKey, out string colorString)
            {
                return _themeColorsDictionary.TryGetValue(colorKey, out colorString);
            }

            public void SetTheme(IRawTheme theme)
            {
                _textMateRegistry.SetTheme(theme);

                var registryTheme = _textMateRegistry.GetTheme();
                GetOrCreateTransformer().SetTheme(registryTheme);

                _tmModel?.InvalidateLine(0);

                _editorModel?.InvalidateViewPortLines();

                _themeColorsDictionary = registryTheme.GetGuiColorDictionary();

                AppliedTheme?.Invoke(this,this);
            }

            public void Dispose()
            {
                _editor.DocumentChanged -= OnEditorOnDocumentChanged;

                DisposeEditorModel(_editorModel);
                DisposeTMModel(_tmModel, _transformer);
                DisposeTransformer(_transformer);
            }

            void OnEditorOnDocumentChanged(object sender, EventArgs args)
            {
                try
                {
                    DisposeEditorModel(_editorModel);
                    DisposeTMModel(_tmModel, _transformer);

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
                    transformer = new TextMateColoringTransformer(
                        _editor.TextArea.TextView, _exceptionHandler);

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

            static void DisposeTMModel(TMModel tmModel, TextMateColoringTransformer transformer)
            {
                if (tmModel == null)
                    return;

                if (transformer != null)
                    tmModel.RemoveModelTokensChangedListener(transformer);

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
