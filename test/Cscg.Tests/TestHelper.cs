using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace Cscg.Tests
{
    public static class TestHelper
    {
        internal static Compilation CreateCompilation(this string source, string name = "Compiled", bool isLib = true)
            => CSharpCompilation.Create(assemblyName: name, syntaxTrees: new[]
                {
                    CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Latest))
                },
                references: new[]
                {
                    MetadataReference.CreateFromFile(typeof(Binder).GetTypeInfo().Assembly.Location)
                },
                options: new CSharpCompilationOptions(isLib
                    ? OutputKind.DynamicallyLinkedLibrary
                    : OutputKind.ConsoleApplication)
            );

        private static GeneratorDriver CreateDriver(Compilation compilation,
            IIncrementalGenerator[] generators, AdditionalText[] texts)
            => CSharpGeneratorDriver.Create(
                generators.Select(GeneratorExtensions.AsSourceGenerator),
                additionalTexts: texts.ToImmutableArray(),
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                driverOptions: new GeneratorDriverOptions(
                    IncrementalGeneratorOutputKind.None,
                    trackIncrementalGeneratorSteps: true)
            );

        internal static (Compilation c, GeneratorDriverRunResult r) RunGenerators(this Compilation compilation,
            out ImmutableArray<Diagnostic> diag, IIncrementalGenerator[] generators, AdditionalText[] additional)
        {
            var driver = CreateDriver(compilation, generators, additional);
            driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var upCompilation, out diag);
            var run = driver.GetRunResult();
            return (upCompilation, run);
        }

        public static void CheckNoError(this ImmutableArray<Diagnostic> diag, Compilation comp, int count)
        {
            Assert.Empty(diag);
            var others = comp.GetDiagnostics();
            Assert.Empty(others);
            var trees = comp.SyntaxTrees.ToArray();
            Assert.Equal(count, trees.Length);
        }

        public static void CheckNoError(this GeneratorDriverRunResult run, int count)
        {
            Assert.Equal(count, run.GeneratedTrees.Length);
            Assert.Empty(run.Diagnostics);
        }

        public static (string name, string content) GetLocalFile(string name,
            string first = "demo", string second = "SourceGenerated")
        {
            var loc = Assembly.GetExecutingAssembly().Location;
            loc = Path.Combine(loc, "..", "..", "..", "..", "..", "..");
            loc = Path.Combine(loc, first, second, name);
            loc = Path.GetFullPath(loc);
            var text = File.ReadAllText(loc, Encoding.UTF8);
            return (loc, text);
        }
    }
}