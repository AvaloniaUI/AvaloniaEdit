using Avalonia;

namespace AvaloniaEdit.Demo
{
    class Program
    {
        static void Main()
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .Start<MainWindow>();
        }
    }
}
