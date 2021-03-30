using SkiaSharp;
using SkiaSharp.Views.UWP;

namespace Sample1
{
    public class Program
    {
        public static void Paint(SKSurface surface, int width, int height)
        {
            surface.Canvas.Clear(SKColors.Blue);
        }
    }
}