using Microsoft.CodeAnalysis;
using Monaco;
using Monaco.Editor;
using Monaco.Helpers;
using SkiaSharp;
using SkiaSharp.Views.UWP;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
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
			Console.WriteLine("Compile {0}", source.Text);

			var result = await Compiler.Compile(source.Text);

			if (result.Assembly != null)
			{
				if (result.Assembly.GetExportedTypes().Where(et => et.Name == "Program").FirstOrDefault() is Type programType)
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

			source.Decorations.Clear();

			var sb = new StringBuilder();

			foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity >= DiagnosticSeverity.Warning ))
			{
				sb.AppendLine(diagnostic.ToString());

				var lineSpan = diagnostic.Location.GetLineSpan();

				var range = new Monaco.Range(
					(uint)lineSpan.StartLinePosition.Line + 1,
					(uint)lineSpan.StartLinePosition.Character,
					(uint)lineSpan.EndLinePosition.Line + 1,
					(uint)lineSpan.StartLinePosition.Character
				);

				// Highlight Error Line
				source.Decorations.Add(new IModelDeltaDecoration(
					range,
					new IModelDecorationOptions() { 
						IsWholeLine = true,
						ClassName = _errorStyle, 
						HoverMessage = new string[] { diagnostic.ToString() }.ToMarkdownString() 
					}));

				// Show Glyph Icon
				source.Decorations.Add(new IModelDeltaDecoration(
					range,
					new IModelDecorationOptions() { 
						IsWholeLine = true,
						GlyphMarginClassName = _errorIconStyle, 
						GlyphMarginHoverMessage = new string[] { diagnostic.ToString() }.ToMarkdownString() }
					));

			}

			buildOutput.Text = sb.ToString();
		}

		private CssLineStyle _errorStyle = new CssLineStyle()
		{
			BackgroundColor = new SolidColorBrush(Color.FromArgb(0x00, 152, 12, 19))
		};

		private CssGlyphStyle _errorIconStyle = new CssGlyphStyle()
		{
			GlyphImage = new System.Uri("Icons/Error.png", UriKind.Relative)
		};

		private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs arg)
		{
			_paint?.Invoke(arg.Surface, arg.Info.Width, arg.Info.Height);
		}
	}
}
