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

		private SkiaRefreshHandler? _paint;
		private Sample selectedSample;

		public MainPage()
		{
			this.InitializeComponent();

			PopuplateSamples();

			source.Text = Samples.FirstOrDefault()?.Code ?? "No embedded samples !";
		}

		public Sample[] Samples { get; private set; }

		public Sample SelectedSample
		{
			get => selectedSample;
			set
			{
				selectedSample = value;

				source.Text = value?.Code ?? "";
			}
		}

		public class Sample
		{
			public string Name { get; set; }
			public string Code { get; set; }
		}

		public void PopuplateSamples()
		{
			var names = typeof(MainPage).Assembly
				.GetManifestResourceNames()
				.Where(n => n.Contains("skia_samples"));

			var list = new List<Sample>();
			foreach (var name in names)
			{
				using (var reader = new StreamReader(typeof(MainPage).Assembly.GetManifestResourceStream(name)))
				{
					var parts = name.Split('.');

					var sample = new Sample
					{
						Name = parts[parts.Length - 2],
						Code = reader.ReadToEnd()
					};

					list.Add(sample);
				}
			}

			Samples = list.ToArray();
		}

		public async void OnCompile()
		{
			try
			{
				var assembly = await Compiler.Compile(source.Text);

				if (assembly.GetExportedTypes().Where(et => et.Name == "Program").FirstOrDefault() is Type programType)
				{
					Console.WriteLine("Got Program type");

					if (programType.GetMethod("Paint") is MethodInfo paintMethod)
					{
						Console.WriteLine("Got Main method");

						_paint = (SkiaRefreshHandler)paintMethod.CreateDelegate(typeof(SkiaRefreshHandler));

						skiaCanvas.Invalidate();
					}
				}
			}
			catch (Exception e)
			{
				Console.WriteLine(e);
			}
		}

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs arg)
		{
			_paint?.Invoke(arg.Surface, arg.Info.Width, arg.Info.Height);
		}
	}
}
