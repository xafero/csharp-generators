using System.IO;
using Cscg.Core;
using Cscg.StubCreator.Model;
using Microsoft.CodeAnalysis;
using static Cscg.Core.Sources;
using XmlTool = Cscg.Core.XmlTool<Cscg.StubCreator.Model.XmlDoc>;

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
            using var input = new StringReader(nac.content);
            using var patch = new XmlDocReader(input);
            var model = XmlTool.Read(patch);

            const string space = Coding.AutoNamespace;
            var className = $"ConstStrings.{nac.name}";
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine("public static partial class CoStrings");
            code.AppendLine("{");

            foreach (var member in model.Members)
            {
                code.AppendLine(" // " + member.Name);
            }

            // TODO ?!

            code.AppendLine("}");
            code.AppendLine("}");
            spc.AddSource(className, code.ToString());
        }

        private static bool IsText(AdditionalText file)
            => file.HasEnding("xml");
    }
}