using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Styling;
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
            private ReadOnlyDictionary<string, string> _guiColorDictionary;
            private IBrush _defaultSelectionBrush;
            public IRegistryOptions RegistryOptions { get { return _textMateRegistryOptions; } }
            public TextEditorModel EditorModel { get { return _editorModel; } }

            public event EventHandler<Installation> AppliedTheme;

            public Installation(TextEditor editor, IRegistryOptions registryOptions, bool initCurrentDocument = true)
            {
                _textMateRegistryOptions = registryOptions;
                _textMateRegistry = new Registry(registryOptions);

                _editor = editor;
                _defaultSelectionBrush = editor.TextArea.SelectionBrush;
                SetTheme(registryOptions.GetDefaultTheme());

                editor.DocumentChanged += OnEditorOnDocumentChanged;

                if (initCurrentDocument)
                {
                    OnEditorOnDocumentChanged(editor, EventArgs.Empty);
                }
            }
            
            public void SetGrammar(string scopeName)
            {
                _grammar = _textMateRegistry.LoadGrammar(scopeName);

                GetOrCreateTransformer().SetGrammar(_grammar);

                _editor.TextArea.TextView.Redraw();
            }
            
            public bool ApplyBrushAction(string colorKeyNameFromJson, Action<IBrush> applyColorAction)
            {
                if (!_guiColorDictionary.TryGetValue(colorKeyNameFromJson, out var colorString))
                {
                    return false;
                }
                var colorBrush = new SolidColorBrush(Color.Parse(colorString));
                applyColorAction(colorBrush);
                return true;
            }

            public void SetTheme(IRawTheme theme, bool applyWindowProperties = true)
            {
                _textMateRegistry.SetTheme(theme);

                var registryTheme = _textMateRegistry.GetTheme();
                GetOrCreateTransformer().SetTheme(registryTheme);

                _tmModel?.InvalidateLine(0);

                _editorModel?.InvalidateViewPortLines();

                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime
                    {
                        MainWindow: Window window
                    })
                {
                    _guiColorDictionary = registryTheme.GetGuiColorDictionary();
                    var themeName = theme.GetName();

                    // Applies to the application on a whole.. Some might want to opt out of this for their own setups.
                    if (applyWindowProperties)
                    {
                        if (themeName != null && themeName.ToLower().Contains("light"))
                        {
                            Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                        }
                        else
                        {
                            Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                        }
                    
                        ApplyBrushAction("editor.background",brush =>window.Background = brush);
                        ApplyBrushAction("editor.foreground",brush =>window.Foreground = brush);
                    }

                    ApplyBrushAction("editor.background",brush =>_editor.Background = brush);
                    ApplyBrushAction("editor.foreground",brush =>_editor.Foreground = brush);
                    
                    if (_defaultSelectionBrush == null)
                    {
                        _defaultSelectionBrush = _editor.TextArea.SelectionBrush;
                    }
                    
                    if (!ApplyBrushAction("editor.selectionBackground",
                            brush => _editor.TextArea.SelectionBrush = brush))
                    {
                        _editor.TextArea.SelectionBrush = _defaultSelectionBrush;
                    }

                    if (!ApplyBrushAction("editor.lineHighlightBackground",
                            brush =>
                            {
                                _editor.TextArea.TextView.CurrentLineBackground = brush;
                                _editor.TextArea.TextView.CurrentLineBorder = new Pen(brush); // Todo: VS Code didn't seem to have a border but it might be nice to have that option. For now just make it the same..
                            }))
                    {
                        _editor.TextArea.TextView.CurrentLineBackground = new SolidColorBrush(CurrentHighlightRendererDefaultBackground);
                        _editor.TextArea.TextView.CurrentLineBorder = new Pen(new SolidColorBrush(CurrentHighlightRendererDefaultBorder));
                    }

                    
                    //Todo: looks like the margin doesn't have a active line highlight, would be a nice addition
                    if (!ApplyBrushAction("editorLineNumber.foreground",
                            brush => _editor.LineNumbersForeground = brush))
                    {
                        _editor.LineNumbersForeground = _editor.TextArea.Foreground;
                    }
                    
                    AppliedTheme?.Invoke(this,this);
                }
            }
            
            //These come out of CurrentHighlightRenderer sealed class. 
            public static readonly Color CurrentHighlightRendererDefaultBackground = Color.FromArgb(22, 20, 220, 224);
            public static readonly Color CurrentHighlightRendererDefaultBorder = Color.FromArgb(52, 0, 255, 110);

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
