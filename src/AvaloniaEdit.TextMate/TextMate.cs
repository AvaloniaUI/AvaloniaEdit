using System;
using System.Collections.Generic;
using System.Linq;
using TextMateSharp.Grammars;
using TextMateSharp.Model;
using TextMateSharp.Themes;

namespace AvaloniaEdit.TextMate
{
    public static class TextMate
    {
        public static void InstallTextMate(this TextEditor editor, Theme theme, IGrammar grammar)
        {
            lock(_lock)
            {
                _installations.Add(editor, new TextMateInstallation(editor, theme, grammar));
            }
        }

        public static void DisposeTextMate(this TextEditor editor)
        {
            lock (_lock)
            {
                if (!_installations.ContainsKey(editor))
                    return;

                _installations[editor].Dispose();
                _installations.Remove(editor);
            }
        }

        public static void InstallGrammar(this TextEditor editor, IGrammar grammar)
        {
            lock (_lock)
            {
                if (!_installations.ContainsKey(editor))
                    return;

                _installations[editor].SetGrammar(grammar);
            }
        }

        public static void InstallTheme(this TextEditor editor, Theme theme)
        {
            lock (_lock)
            {
                if (!_installations.ContainsKey(editor))
                    return;

                _installations[editor].SetTheme(theme);
            }
        }

        static object _lock = new object();
        static Dictionary<TextEditor, TextMateInstallation> _installations = new Dictionary<TextEditor, TextMateInstallation>();

        class TextMateInstallation
        {
            internal TextMateInstallation(TextEditor editor, Theme theme, IGrammar grammar)
            {
                _editor = editor;

                SetTheme(theme);
                SetGrammar(grammar);

                editor.DocumentChanged += OnEditorOnDocumentChanged;

                OnEditorOnDocumentChanged(editor, EventArgs.Empty);
            }

            internal void SetGrammar(IGrammar grammar)
            {
                _grammar = grammar;

                GetOrCreateTransformer().SetGrammar(grammar);

                _editor.TextArea.TextView.Redraw();
            }

            internal void SetTheme(Theme theme)
            {
                GetOrCreateTransformer().SetTheme(theme);

                _editor.TextArea.TextView.Redraw();
            }

            internal void Dispose()
            {
                _editor.DocumentChanged -= OnEditorOnDocumentChanged;

                DisposeTMModel(_tmModel);
            }

            void OnEditorOnDocumentChanged(object sender, EventArgs args)
            {
                DisposeTMModel(_tmModel);

                var editorModel = new TextEditorModel(_editor, _editor.Document);
                _tmModel = new TMModel(editorModel);
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
            IGrammar _grammar;
            TMModel _tmModel;
        }
    }
}