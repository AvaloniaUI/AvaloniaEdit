[![downloads](https://img.shields.io/nuget/dt/avalonia.AvaloniaEdit)](https://www.nuget.org/packages/Avalonia.AvaloniaEdit)
[![Current stable version](https://img.shields.io/nuget/v/Avalonia.AvaloniaEdit.svg)](https://www.nuget.org/packages/Avalonia.AvaloniaEdit)
# AvaloniaEdit

This project is a port of [AvalonEdit](https://github.com/icsharpcode/AvalonEdit), a WPF-based text editor for [Avalonia](https://github.com/AvaloniaUI/Avalonia).

AvaloniaEdit supports features like:

  * Syntax highlighting using [TextMate](https://github.com/danipen/TextMateSharp) grammars and themes.
  * Code folding.
  * Code completion.
  * Fully customizable and extensible.
  * Line numeration.
  * Display whitespaces EOLs and tabs.
  * Line virtualization.
  * Multi-caret edition.
  * Intra-column adornments.
  * Word wrapping.
  * Scrolling below document.
  * Hyperlinks.

  and many,many more!
  
AvaloniaEdit currently consists of 2 packages
  * [Avalonia.AvaloniaEdit](https://www.nuget.org/packages/Avalonia.AvaloniaEdit) well-known package that incudes text editor itself.
  * [AvaloniaEdit.TextMate](https://www.nuget.org/packages/AvaloniaEdit.TextMate/) package that adds TextMate integration to the AvaloniaEdit.

## Getting started

 ### How to set up a new project that displays an AvaloniaEdit editor?
* Create an [empty Avalonia application](https://docs.avaloniaui.net/docs/getting-started).
* Add a the [nuget reference](https://www.nuget.org/packages/Avalonia.AvaloniaEdit/#versions-body-tab) to the latest version:
`<PackageReference Include="Avalonia.AvaloniaEdit" Version="x.y.z.t" />`
* Include the *needed styles* into your `<Application.Styles>` in your `App.xaml`: 
  * If you're using `0.10.x.y` based versions, include `<StyleInclude Source="avares://AvaloniaEdit/AvaloniaEdit.xaml" />`
  * If you're `11.x.y` based versions, include `<StyleInclude Source="avares://AvaloniaEdit/Themes/Fluent/AvaloniaEdit.xaml" />`
* Finally, add the AvaloniaEdit editor into your window:
```xaml
<Window xmlns="https://github.com/avaloniaui"
        ...
        xmlns:AvaloniaEdit="clr-namespace:AvaloniaEdit;assembly=AvaloniaEdit"
        ...>
  ...
  <AvaloniaEdit:TextEditor Text="Hello AvaloniaEdit!"
                           ShowLineNumbers="True"
                           FontFamily="Cascadia Code,Consolas,Menlo,Monospace"/>
  ...
</Window>
```
You can see the [Demo Application](https://github.com/AvaloniaUI/AvaloniaEdit/tree/master/src/AvaloniaEdit.Demo) as a reference.

 ### How to set up TextMate theme and syntax highlighting for my project?
First of all, if you want to use grammars supported by [TextMateSharp](https://github.com/danipen/TextMateSharp), should install the following packages:
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
  
![avaloniaedit-demo](https://user-images.githubusercontent.com/501613/169226248-946e716d-dea3-4c6d-9ae9-6148b2a51f03.gif)


