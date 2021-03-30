using SkiaSharp;
using SkiaSharp.Views.UWP;

namespace Sample1
{
    public class Program
    {
        public static void Paint(SKSurface surface, int width, int height)
        {
			var colors = new[] { SKColors.Cyan, SKColors.Magenta, SKColors.Yellow, SKColors.Cyan };
			var center = new SKPoint(width / 2f, height / 2f);

			using (var shader = SKShader.CreateSweepGradient(center, colors, null))
			using (var paint = new SKPaint())
			{
				paint.Shader = shader;
				surface.Canvas.DrawPaint(paint);
			}
		}
    }
}