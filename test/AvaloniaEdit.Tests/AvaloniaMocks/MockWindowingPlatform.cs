using System;
using Avalonia.Platform;
using Moq;

namespace AvaloniaEdit.AvaloniaMocks
{
    public class MockWindowingPlatform : IWindowingPlatform
    {
        private readonly Func<IWindowImpl> _windowImpl;
        private readonly Func<IPopupImpl> _popupImpl;

        public MockWindowingPlatform(Func<IWindowImpl> windowImpl = null, Func<IPopupImpl> popupImpl = null)
        {
            _windowImpl = windowImpl;
            _popupImpl = popupImpl;
        }

        public IWindowImpl CreateWindow()
        {
            return _windowImpl?.Invoke() ?? Mock.Of<IWindowImpl>(x => x.RenderScaling == 1);
        }

        public IWindowImpl CreateEmbeddableWindow()
        {
            throw new NotImplementedException();
        }

        public ITrayIconImpl CreateTrayIcon()
        {
            throw new NotImplementedException();
        }

        public IPopupImpl CreatePopup() => _popupImpl?.Invoke() ?? Mock.Of<IPopupImpl>(x => x.RenderScaling == 1);
    }
}