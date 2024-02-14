using System;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Markup.Xaml.Styling;
using AvaloniaEdit.AvaloniaMocks;

[assembly: AvaloniaTestApplication(typeof(UnitTestApplication))]

namespace AvaloniaEdit.AvaloniaMocks
{
    public class UnitTestApplication : Application
    {
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<UnitTestApplication>()
                .UseHeadless(new AvaloniaHeadlessPlatformOptions
                {
                    UseHeadlessDrawing = true
                });

        public static void InitializeStyles()
        {
            ResourceInclude styleInclude = new ResourceInclude(new Uri("avares://AvaloniaEdit"))
            {
                Source = new Uri("/Themes/Base.xaml", UriKind.Relative)
            };

            Application.Current?.Resources.MergedDictionaries.Add(styleInclude);
        }
    }
}