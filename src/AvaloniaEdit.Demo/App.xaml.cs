using Avalonia;
using Avalonia.Markup.Xaml;

namespace AvaloniaEdit.Demo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
