using System;
using Windows.UI.Xaml;

namespace UnoSkiaSharpFiddle.Wasm
{
    public class Program
    {
        static int Main(string[] args)
        {
            Windows.UI.Xaml.Application.Start(_ => new App());

            return 0;
        }
    }
}
