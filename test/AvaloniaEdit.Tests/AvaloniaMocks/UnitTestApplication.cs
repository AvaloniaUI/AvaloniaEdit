using Avalonia;
using Avalonia.Headless;
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
    }
}