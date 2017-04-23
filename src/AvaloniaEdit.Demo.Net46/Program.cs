using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia;

namespace AvaloniaEdit.Demo.Net46
{
    class Program
    {
        static void Main(string[] args)
        {
            AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .Start<MainWindow>();
        }
    }
}
