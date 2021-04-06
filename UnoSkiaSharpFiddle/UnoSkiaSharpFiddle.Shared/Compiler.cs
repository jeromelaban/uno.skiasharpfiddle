using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;
using System.Threading.Tasks;
using Windows.Storage;
using System.Collections.Immutable;

namespace UnoSkiaSharpFiddle
{
	public class Compiler
	{
		public record CompilationResult 
		{
			public Assembly? Assembly;
			public ImmutableArray<Diagnostic> Diagnostics;
		}

		public static async Task<CompilationResult> Compile(string source)
		{
			var cus = SyntaxFactory.ParseCompilationUnit(source);

			var sourceLanguage = CSharpLanguage.Instance;

			await sourceLanguage.Initialize();

			Compilation compilation = sourceLanguage
			  .CreateLibraryCompilation(assemblyName: "InMemoryAssembly", enableOptimisations: false)
			  .AddSyntaxTrees(new[] { cus.SyntaxTree });

			Console.WriteLine($"Got compilation");

			// GetDiagnostics seems to keep running in a CPU Bound loop
			Console.WriteLine($"Got compilation Diagnostics: {compilation.GetDiagnostics().Length}");
			Console.WriteLine($"Got compilation DeclarationDiagnostics: {compilation.GetDeclarationDiagnostics().Length}");

			Console.WriteLine($"Emitting assembly...");
			var stream = new MemoryStream();
			var emitResult = compilation.Emit(stream);

			if (emitResult.Success)
			{
				Console.WriteLine($"Got binary assembly: {emitResult.Success}");

				return new CompilationResult()
				{
					Assembly = Assembly.Load(stream.ToArray()),
					Diagnostics = emitResult.Diagnostics
				};
			}
			else
			{
				return new CompilationResult()
				{
					Assembly = null,
					Diagnostics = emitResult.Diagnostics
				};
			}
		}
	}

	public class CSharpLanguage : ILanguageService
	{
		private MetadataReference[] _references;

		public static CSharpLanguage Instance { get; } = new CSharpLanguage();

		private CSharpLanguage()
		{
		}

		public async Task Initialize()
		{
			if (_references != null)
			{
				return;
			}

			var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri("ms-appx:///dotnet-sdk/fileslist.txt"));

			var sdkFiles = await FileIO.ReadLinesAsync(file);

			var tasks = sdkFiles
				.Select(f =>
				{
					async Task<MetadataReference> LoadReference()
					{
						var targetFile = f
						.Replace(".dll", ".clr")
						.Replace("dotnet-sdk-source", "dotnet-sdk");

						var sdkFile = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///{targetFile}"));

						using var stream = await sdkFile.OpenReadAsync();
						var streamCopy = new MemoryStream();
						stream.AsStream().CopyTo(streamCopy);

						streamCopy.Position = 0;
						return MetadataReference.CreateFromStream(streamCopy);
					}

					return LoadReference();
				});

			_references = await Task.WhenAll(tasks);
		}

		public Compilation CreateLibraryCompilation(string assemblyName, bool enableOptimisations)
		{
			var options = new CSharpCompilationOptions(
				OutputKind.DynamicallyLinkedLibrary,
				optimizationLevel: OptimizationLevel.Release,
				allowUnsafe: true)
				// Disabling concurrent builds allows for the emit to finish.
				.WithConcurrentBuild(false)
				;

			return CSharpCompilation.Create(assemblyName, options: options, references: _references);
		}
	}

}
