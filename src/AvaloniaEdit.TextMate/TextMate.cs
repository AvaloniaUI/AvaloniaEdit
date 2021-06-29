using System;
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
            editor.InstallTheme(theme);
            editor.InstallGrammar(grammar);


            void OnEditorOnDocumentChanged(object sender, EventArgs args)
            {
                var editorModel = new TextEditorModel(editor, editor.Document);
                var model = new TMModel(editorModel);


                editor.GetOrCreateTransformer().SetModel(model);
                model.AddModelTokensChangedListener(editorModel);
            }
            
            OnEditorOnDocumentChanged(editor, EventArgs.Empty);

            editor.DocumentChanged += OnEditorOnDocumentChanged;
        }
        
        public static void InstallGrammar(this TextEditor editor, IGrammar grammar)
        {
            var transformer = editor.GetOrCreateTransformer();
            
            transformer.SetGrammar(grammar);

            editor.TextArea.TextView.Redraw();
        }

        public static void InstallTheme(this TextEditor editor, Theme theme)
        {
            var transformer = editor.GetOrCreateTransformer();
            
            transformer.SetTheme(theme);

            editor.TextArea.TextView.Redraw();
        }

        private static TextMateColoringTransformer GetOrCreateTransformer(this TextEditor editor)
        {
            var transformer = editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>().FirstOrDefault();

            if (transformer is null)
            {
                transformer = new TextMateColoringTransformer();
                
                editor.TextArea.TextView.LineTransformers.Add(transformer);
            }

            return transformer;
        }
    }
}