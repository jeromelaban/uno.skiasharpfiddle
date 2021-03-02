using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Linq;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Host;

namespace UnoSkiaSharpFiddle
{
    public class Compiler
    {
        public static Assembly Compile(string source)
        {
            var cus = SyntaxFactory.ParseCompilationUnit(source);

            var sourceLanguage = CSharpLanguage.Instance;

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

                return Assembly.Load(stream.ToArray());
            }
            else
            {
                foreach (var diagnostic in emitResult.Diagnostics)
                {
                    Console.WriteLine(diagnostic);
                }

                throw new InvalidOperationException("Failed to emit assembly");
            }
        }
    }

    public class CSharpLanguage : ILanguageService
    {
        private readonly IEnumerable<MetadataReference> _references;

        public static CSharpLanguage Instance { get; } = new CSharpLanguage();

        private CSharpLanguage()
        {
            var sdkFiles = this.GetType().Assembly.GetManifestResourceNames().Where(f => f.Contains("dotnet_sdk"));

            _references = sdkFiles
                .Select(f =>
                {
                    using (var s = this.GetType().Assembly.GetManifestResourceStream(f))
                    {
                        return MetadataReference.CreateFromStream(s);
                    }
                })
                .ToArray();
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
