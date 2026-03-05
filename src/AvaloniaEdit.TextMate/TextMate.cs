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

        public class Installation : IDisposable
        {
            private bool _isDisposed;
            private readonly IRegistryOptions _textMateRegistryOptions;
            private readonly Registry _textMateRegistry;
            private readonly TextEditor _editor;
            private TextEditorModel _editorModel;
            private IGrammar _grammar;
            private TMModel _tmModel;
            private TextMateColoringTransformer _transformer;
            private readonly bool _ownsTransformer;
            private ReadOnlyDictionary<string, string> _themeColorsDictionary;
            public IRegistryOptions RegistryOptions { get { return _textMateRegistryOptions; } }
            public TextEditorModel EditorModel { get { return _editorModel; } }

            public event EventHandler<Installation> AppliedTheme;

            public Installation(TextEditor editor, IRegistryOptions registryOptions, bool initCurrentDocument = true)
            {
                _textMateRegistryOptions = registryOptions ?? throw new ArgumentNullException(nameof(registryOptions));
                _editor = editor ?? throw new ArgumentNullException(nameof(editor));

                _textMateRegistry = new Registry(registryOptions);
                _transformer = _editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>().FirstOrDefault();

                if (_transformer is null)
                {
                    _transformer = new TextMateColoringTransformer(_editor.TextArea.TextView, _exceptionHandler);
                    _editor.TextArea.TextView.LineTransformers.Add(_transformer);
                    _ownsTransformer = true;
                }

                SetTheme(registryOptions.GetDefaultTheme());

                editor.DocumentChanged += OnEditorOnDocumentChanged;

                if (initCurrentDocument)
                {
                    OnEditorOnDocumentChanged(editor, EventArgs.Empty);
                }
            }

            public void SetGrammar(string scopeName)
            {
                ThrowIfDisposed();
                SetGrammarInternal(_textMateRegistry.LoadGrammar(scopeName));
            }

            public void SetGrammarFile(string path)
            {
                ThrowIfDisposed();
                if (_transformer == null || _editor?.TextArea?.TextView.LineTransformers == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(TextMate)} is not initialized. You must call {nameof(TextMate)}.{nameof(InstallTextMate)} before using this feature.");
                }

                _grammar = _textMateRegistry.LoadGrammarFromPathSync(path, 0, null);
                SetGrammarInternal(_grammar);
            }

            private void SetGrammarInternal(IGrammar grammar)
            {
                ThrowIfDisposed();

                _grammar = grammar;
                _transformer.SetGrammar(_grammar);
                _editor.TextArea.TextView.Redraw();
            }

            public bool TryGetThemeColor(string colorKey, out string colorString)
            {
                ThrowIfDisposed();

                return _themeColorsDictionary.TryGetValue(colorKey, out colorString);
            }

            public void SetTheme(IRawTheme theme)
            {
                ThrowIfDisposed();
                if (_transformer == null || _editor?.TextArea?.TextView.LineTransformers == null)
                {
                    throw new InvalidOperationException(
                        $"{nameof(TextMate)} is not initialized. You must call {nameof(TextMate)}.{nameof(InstallTextMate)} before using this feature.");
                }

                _textMateRegistry.SetTheme(theme);

                var registryTheme = _textMateRegistry.GetTheme();
                _transformer.SetTheme(registryTheme);

                _tmModel?.InvalidateLine(0);

                _editorModel?.InvalidateViewPortLines();

                _themeColorsDictionary = registryTheme.GetGuiColorDictionary();

                AppliedTheme?.Invoke(this, this);
            }

            public void Dispose()
            {
                if (_isDisposed)
                    return;

                // mark disposed first to prevent reentrancy recreating state during teardown
                _isDisposed = true;

                _editor.DocumentChanged -= OnEditorOnDocumentChanged;

                DisposeEditorModel(_editorModel);
                _editorModel = null;

                DisposeTMModel(_tmModel, _transformer);
                _tmModel = null;

                // Only remove/dispose if we created it; otherwise we don't own lifecycle.
                if (_ownsTransformer && _transformer != null)
                {
                    _editor.TextArea.TextView.LineTransformers.Remove(_transformer);
                    DisposeTransformer(_transformer);
                }

                _transformer = null;
            }

            void OnEditorOnDocumentChanged(object sender, EventArgs args)
            {
                // don't use ThrowIfDisposed here because throwing exceptions in an event handler is a no-no!
                if (_isDisposed)
                    return;

                try
                {
                    DisposeEditorModel(_editorModel);
                    DisposeTMModel(_tmModel, _transformer);

                    _editorModel = new TextEditorModel(_editor.TextArea.TextView, _editor.Document, _exceptionHandler);
                    _tmModel = new TMModel(_editorModel);
                    _tmModel.SetGrammar(_grammar);

                    _transformer.SetModel(_editor.Document, _tmModel);
                    _tmModel.AddModelTokensChangedListener(_transformer);
                }
                catch (Exception ex)
                {
                    _exceptionHandler?.Invoke(ex);
                }
            }

            void ThrowIfDisposed()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(nameof(Installation));
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
