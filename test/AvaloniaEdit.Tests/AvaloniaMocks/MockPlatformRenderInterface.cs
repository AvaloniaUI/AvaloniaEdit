using System;
using System.Collections.Generic;
using System.IO;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform;
using Moq;

namespace AvaloniaEdit.AvaloniaMocks
{
    public class MockPlatformRenderInterface : IPlatformRenderInterface
    {
        public IEnumerable<string> InstalledFontNames => throw new NotImplementedException();

        public IFormattedTextImpl CreateFormattedText(
            string text,
            Typeface typeface,
            TextAlignment textAlignment,
            TextWrapping wrapping,
            Size constraint,
            IReadOnlyList<FormattedTextStyleSpan> spans)
        {
            return Mock.Of<IFormattedTextImpl>();
        }

        public IRenderTarget CreateRenderTarget(IEnumerable<object> surfaces)
        {
            return Mock.Of<IRenderTarget>();
        }

        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(
            int width,
            int height,
            double dpiX,
            double dpiY)
        {
            return Mock.Of<IRenderTargetBitmapImpl>();
        }

        public IRenderTargetBitmapImpl CreateRenderTargetBitmap(PixelSize size, Vector dpi)
        {
            throw new NotImplementedException();
        }

        public IStreamGeometryImpl CreateStreamGeometry()
        {
            return new MockStreamGeometryImpl();
        }

        public IWriteableBitmapImpl CreateWriteableBitmap(PixelSize size, Vector dpi, PixelFormat? format = null)
        {
            throw new NotImplementedException();
        }

        public IBitmapImpl LoadBitmap(Stream stream)
        {
            return Mock.Of<IBitmapImpl>();
        }

        public IBitmapImpl LoadBitmap(string fileName)
        {
            return Mock.Of<IBitmapImpl>();
        }

        public IBitmapImpl LoadBitmap(PixelFormat format, IntPtr data, PixelSize size, Vector dpi, int stride)
        {
            throw new NotImplementedException();
        }
    }
}