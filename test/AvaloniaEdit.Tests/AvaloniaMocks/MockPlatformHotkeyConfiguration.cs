using Avalonia.Input.Platform;

namespace AvaloniaEdit.AvaloniaMocks
{
    public class MockPlatformHotkeyConfiguration : PlatformHotkeyConfiguration
    {
        public MockPlatformHotkeyConfiguration() : base(Avalonia.Input.KeyModifiers.Control)
        {

        }

    }
}
