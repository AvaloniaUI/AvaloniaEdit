using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Platform;
using Avalonia.Markup.Xaml;
using System;

namespace AvaloniaEdit.Demo
{
    public class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
            {

                var window = new MainWindow();

                desktopLifetime.MainWindow = window;
                //PlatformManager.CreateEmbeddableWindow().Crea
                

            }
            else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewLifetime)
            {
                throw new NotImplementedException();
                /*
                view.Initialized += (sennder, e) =>
                {
                    App.Load();
                };

                singleViewLifetime.MainView = view;
                */
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
