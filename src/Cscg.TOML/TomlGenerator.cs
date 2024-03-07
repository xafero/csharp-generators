using System;
using System.Globalization;
using System.Linq;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using static Cscg.Core.Sources;

namespace Cscg.TOML
{
    [Generator(LanguageNames.CSharp)]
    public sealed class TomlGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            var files = ctx.AdditionalTextsProvider.Where(IsText);
            var contents = files.Select(ToNamed);
            ctx.RegisterSourceOutput(contents, Proc);
        }

        private static void Proc(SourceProductionContext spc, (string name, string content) nac)
        {
            const string space = Coding.AutoNamespace;
            var className = $"ConstStrings.{nac.name}";
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine("public static partial class ConstStrings");
            code.AppendLine("{");
            code.AppendLine("}");
            code.AppendLine("}");
            spc.AddSource(className, code.ToString());
        }

        private static bool IsText(AdditionalText file)
            => file.HasEnding("toml");
    }
}