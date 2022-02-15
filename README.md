[![downloads](https://img.shields.io/nuget/dt/avalonia.AvaloniaEdit)](https://www.nuget.org/packages/Avalonia.AvaloniaEdit)
[![Current stable version](https://img.shields.io/nuget/v/Avalonia.AvaloniaEdit.svg)](https://www.nuget.org/packages/Avalonia.AvaloniaEdit)
# AvaloniaEdit

This project is a port of [AvalonEdit](https://github.com/icsharpcode/AvalonEdit) WPF-based text editor for [Avalonia](https://github.com/AvaloniaUI/Avalonia).

AvaloniaEdit supports features like:

  * Syntax highlighting using [TextMate](https://github.com/danipen/TextMateSharp) grammars
  * Line numeration
  * Rectangular selection
  * Intra-column adornments
  * Word wrapping
  * Scrolling below document
  * Hyperlinks

  and many,many more!
  
AvaloniaEdit currently consists of 2 packages
  * [Avalonia.AvaloniaEdit](https://www.nuget.org/packages/Avalonia.AvaloniaEdit) well-known package that incudes text editor itself.
  * [AvaloniaEdit.TextMate](https://www.nuget.org/packages/AvaloniaEdit.TextMate/) package that adds TextMate integration to the AvaloniaEdit.

 ### How to set up TextMate theme and syntax highlighting for my project?
First of all, if you want to use grammars supported by TextMateSharp, should install the following packages:
- [AvaloniaEdit.TextMate](https://www.nuget.org/packages/AvaloniaEdit.TextMate/) 
- [TextMateSharp.Grammars](https://www.nuget.org/packages/TextMateSharp.Grammars/) 
 
Alternatively, if you want to support your own grammars, you just need to install the AvaloniaEdit.TextMate package, and implement IRegistryOptions interface, that's currently the easiest way in case you want to use AvaloniaEdit with the set of grammars different from in-bundled TextMateSharp.Grammars.
```csharp
//First of all you need to have a reference for your TextEditor for it to be used inside AvaloniaEdit.TextMate project.
var _textEditor = this.FindControl<TextEditor>("Editor");

//Here we initialize RegistryOptions with the theme we want to use.
var  _registryOptions = new RegistryOptions(ThemeName.DarkPlus);

//Initial setup of TextMate.
var _textMateInstallation = _textEditor.InstallTextMate(_registryOptions);

//Here we are getting the language by the extension and right after that we are initializing grammar with this language.
//And that's all 😀, you are ready to use AvaloniaEdit with syntax highlighting!
_textMateInstallation.SetGrammar(_registryOptions.GetScopeByLanguageId(_registryOptions.GetLanguageByExtension(".cs").Id));
```
  
![image](https://user-images.githubusercontent.com/53405089/147930720-b388df7e-9b83-4ade-9338-6d311b334814.png)


