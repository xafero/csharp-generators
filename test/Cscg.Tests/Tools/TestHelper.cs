using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Xml;
using Cscg.Compactor.Lib;
using Cscg.AdoNet.Lib;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;
using System.ComponentModel;

namespace Cscg.Tests.Tools
{
    public static class TestHelper
    {
        internal static Compilation CreateCompilation(this string source, string name = "Compiled",
            bool isLib = true, PortableExecutableReference[] addedRefs = null, string[] addedSrcs = null)
        {
            var references = new[]
            {
                GetMetaRef<CompactedAttribute>(), GetMetaRef<object>("System.Xml.ReaderWriter.dll"),
                GetMetaRef<Utf8JsonWriter>(), GetMetaRef<XmlWriter>(), GetMetaRef<object>("System.Runtime.dll"),
                GetMetaRef<Binder>(), GetMetaRef(typeof(Console)), GetMetaRef<object>("System.Collections.dll"),
                GetMetaRef<TableAttribute>(), GetMetaRef<IQueryable>(), GetMetaRef<IComponent>(), 
                GetMetaRef(typeof(Enumerable)), GetMetaRef<object>("System.Private.Uri.dll")
            }
            .Concat(addedRefs ?? []).ToArray();
            var cpo = new CSharpParseOptions(LanguageVersion.Latest);
            var syntaxTrees = new[] { source }.Concat(addedSrcs ?? [])
                .Select(st => CSharpSyntaxTree.ParseText(st, cpo)).ToArray();
            return CSharpCompilation.Create(assemblyName: name, syntaxTrees, references,
                options: new CSharpCompilationOptions(isLib
                    ? OutputKind.DynamicallyLinkedLibrary
                    : OutputKind.ConsoleApplication)
            );
        }

        internal static PortableExecutableReference GetMetaRef<T>(string replace = null)
            => GetMetaRef(typeof(T), replace);

        private static PortableExecutableReference GetMetaRef(Type type, string replace = null)
        {
            var loc = type.GetTypeInfo().Assembly.Location;
            if (replace != null)
                loc = Path.Combine(Path.GetDirectoryName(loc)!, replace);
            return MetadataReference.CreateFromFile(loc);
        }

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
            var ids = new[] { "CS8019" };
            Assert.Empty(diag);
            var others = comp.GetDiagnostics().Where(c => !ids.Contains(c.Id)).ToArray();
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