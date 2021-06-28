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
        }
        
        public static void InstallGrammar(this TextEditor editor, IGrammar grammar)
        {
            var transformer = editor.GetOrCreateTransformer();
            
            transformer.SetGrammar(grammar);
        }

        public static void InstallTheme(this TextEditor editor, Theme theme)
        {
            var transformer = editor.GetOrCreateTransformer();
            
            transformer.SetTheme(theme);
            
            editor.InvalidateVisual();
        }

        private static TextMateColoringTransformer GetOrCreateTransformer(this TextEditor editor)
        {
            var transformer = editor.TextArea.TextView.LineTransformers.OfType<TextMateColoringTransformer>().FirstOrDefault();

            if (transformer is null)
            {
                var editorModel = new TextEditorModel(editor);
                var model = new TMModel(editorModel);
                
                transformer = new TextMateColoringTransformer(model);
                model.AddModelTokensChangedListener(editorModel);
                
                editor.TextArea.TextView.LineTransformers.Add(transformer);
            }

            return transformer;
        }
    }
}