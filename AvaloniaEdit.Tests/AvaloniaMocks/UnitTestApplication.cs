using System;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;

namespace AvaloniaEdit.AvaloniaMocks
{
    public class UnitTestApplication : Application
    {
        private readonly TestServices _services;

        public UnitTestApplication(TestServices services)
        {
            _services = services ?? new TestServices();
            RegisterServices();
        }

        public static new UnitTestApplication Current => (UnitTestApplication)Application.Current;

        public TestServices Services => _services;

        public static IDisposable Start(TestServices services = null)
        {
            var scope = AvaloniaLocator.EnterScope();
            var app = new UnitTestApplication(services);
            AvaloniaLocator.CurrentMutable.BindToSelf<Application>(app);
            var updateServices = Dispatcher.UIThread.GetType().GetMethod("UpdateServices", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            updateServices?.Invoke(Dispatcher.UIThread, null);
            return Disposable.Create(() =>
            {
                scope.Dispose();
                updateServices?.Invoke(Dispatcher.UIThread, null);
            });
        }

        public override void RegisterServices()
        {
            AvaloniaLocator.CurrentMutable
                .Bind<IAssetLoader>().ToConstant(Services.AssetLoader)
                .Bind<IFocusManager>().ToConstant(Services.FocusManager)
                .BindToSelf<IGlobalStyles>(this)
                .Bind<IInputManager>().ToConstant(Services.InputManager)
                .Bind<IKeyboardDevice>().ToConstant(Services.KeyboardDevice?.Invoke())
                .Bind<IKeyboardNavigationHandler>().ToConstant(Services.KeyboardNavigation)
                .Bind<ILayoutManager>().ToConstant(Services.LayoutManager)
                .Bind<IMouseDevice>().ToConstant(Services.MouseDevice?.Invoke())
                .Bind<IRuntimePlatform>().ToConstant(Services.Platform)
                .Bind<IPlatformRenderInterface>().ToConstant(Services.RenderInterface)
                .Bind<IPlatformThreadingInterface>().ToConstant(Services.ThreadingInterface)
                .Bind<IScheduler>().ToConstant(Services.Scheduler)
                .Bind<IStandardCursorFactory>().ToConstant(Services.StandardCursorFactory)
                .Bind<IStyler>().ToConstant(Services.Styler)
                .Bind<IWindowingPlatform>().ToConstant(Services.WindowingPlatform)
                .Bind<IApplicationLifecycle>().ToConstant(this);
            var styles = Services.Theme?.Invoke();

            if (styles != null)
            {
                Styles.AddRange(styles);
            }
        }
    }
}