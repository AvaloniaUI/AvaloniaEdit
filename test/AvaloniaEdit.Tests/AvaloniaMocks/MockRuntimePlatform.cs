using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Avalonia.Platform;

namespace AvaloniaEdit.AvaloniaMocks
{
    public class MockRuntimePlatform : IRuntimePlatform
    {
        IUnmanagedBlob IRuntimePlatform.AllocBlob(int size)
        {
            throw new NotImplementedException();
        }

        RuntimePlatformInfo IRuntimePlatform.GetRuntimeInfo()
        {
            return new RuntimePlatformInfo();
        }

        IDisposable IRuntimePlatform.StartSystemTimer(TimeSpan interval, Action tick)
        {
            throw new NotImplementedException();
        }
    }
}
