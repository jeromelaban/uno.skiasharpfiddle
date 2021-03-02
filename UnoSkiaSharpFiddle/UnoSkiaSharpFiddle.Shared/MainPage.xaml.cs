using SkiaSharp;
using SkiaSharp.Views.UWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace UnoSkiaSharpFiddle
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private delegate void SkiaRefreshHandler(SKSurface surface, int width, int height);

        private SkiaRefreshHandler _paint;

        public MainPage()
        {
            this.InitializeComponent();

            source.Text = @"
using SkiaSharp;
using SkiaSharp.Views.UWP;

public class Program 
{ 
    public static void Paint(SKSurface surface, int width, int height)
    {
        surface.Canvas.Clear(SKColors.Blue);
    }
}
";
        }

        public void OnCompile()
        {
            var assembly = Compiler.Compile(source.Text);

            if (assembly.GetExportedTypes().Where(et => et.Name == "Program").FirstOrDefault() is Type programType)
            {
                Console.WriteLine("Got Program type");

                if (programType.GetMethod("Paint") is MethodInfo paintMethod)
                {
                    Console.WriteLine("Got Main method");

                    _paint = (SkiaRefreshHandler)paintMethod.CreateDelegate(typeof(SkiaRefreshHandler));
                }
            }
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs arg)
        {
            _paint(arg.Surface, arg.Info.Width, arg.Info.Height);
        }
    }
}
