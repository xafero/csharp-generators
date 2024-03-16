using Cscg.Core;
using Microsoft.CodeAnalysis;
using static Cscg.Core.Sources;

namespace Cscg.StubCreator
{
    [Generator(LanguageNames.CSharp)]
    public sealed class StubGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            var files = ctx.AdditionalTextsProvider.Where(IsText);

            var contents = files.Select(ToNamed);

            ctx.RegisterSourceOutput(contents, Generate);
        }

        private static void Generate(SourceProductionContext spc, (string name, string content) nac)
        {
            const string space = Coding.AutoNamespace;
            var className = $"ConstStrings.{nac.name}";
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine("public static partial class CoStrings");
            code.AppendLine("{");

            // TODO ?!

            code.AppendLine("}");
            code.AppendLine("}");
            spc.AddSource(className, code.ToString());
        }

        private static bool IsText(AdditionalText file)
            => file.HasEnding("xml");
    }
}