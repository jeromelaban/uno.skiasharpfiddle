using Tizen.Applications;
using Uno.UI.Runtime.Skia;

namespace UnoSkiaSharpFiddle.Skia.Tizen
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new TizenHost(() => new UnoSkiaSharpFiddle.App(), args);
            host.Run();
        }
    }
}
