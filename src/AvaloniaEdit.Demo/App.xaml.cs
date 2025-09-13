using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using System;
using System.ComponentModel;
using Avalonia.Styling;
using AvaloniaEdit.Demo.ViewModels;

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
                if (window.DataContext is MainWindowViewModel mainWindowViewModel)
                {
                    mainWindowViewModel.PropertyChanged +=MainWindowViewModelOnPropertyChanged;
                }
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

        private void MainWindowViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is not MainWindowViewModel mainWindowViewModel) return;
            if (e.PropertyName == nameof(MainWindowViewModel.SelectedTheme))
            {
                RequestedThemeVariant = mainWindowViewModel.SelectedTheme.ThemeName.ToString().ToLower().Contains("light")
                    ? ThemeVariant.Light
                    : ThemeVariant.Dark;
            }
        }
    }
}
