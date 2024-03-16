using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            var className = $"ConstStrings.{nac.name}";
            var code = new CodeWriter();
            code.AppendLine("using System;");

            foreach (var member in model.Members)
            {
                var (type, name) = SplitName(member.Name);
                switch (type)
                {
                    case "T":
                        code.AppendLine();
                        var tP = FindParent(name);
                        code.AppendLine($"namespace {tP.parent}");
                        code.AppendLine("{");
                        code.AppendLines(ToElements(member));
                        code.AppendLine($"public partial class {tP.name}");
                        code.AppendLine("{");
                        foreach (var item in model.Members)
                        {
                            var (subType, subName) = SplitName(item.Name);
                            if (!subName.StartsWith(name))
                                continue;
                            subName = subName.Replace(name, string.Empty);
                            if (!subName.StartsWith("."))
                                continue;
                            subName = subName.Trim('.');
                            switch (subType)
                            {
                                case "P":
                                    code.AppendLine();
                                    code.AppendLines(ToElements(item));
                                    code.AppendLine($"public object {subName} {{ get; set; }}");
                                    break;
                                case "M":
                                    code.AppendLine();
                                    const string cSt = "#ctor";
                                    if (subName.StartsWith(cSt))
                                    {
                                        subName = subName.Replace(cSt, tP.name);
                                        code.AppendLine($"public {subName} {{ }}");
                                        continue;
                                    }
                                    code.AppendLines(ToElements(item));
                                    code.AppendLine($"public void {subName} {{ }}");
                                    break;
                            }
                        }
                        code.AppendLine("}");
                        code.AppendLine("}");
                        continue;
                }
            }

            spc.AddSource(className, code.ToString());
        }

        private static IEnumerable<string> ToElements(XmlMember item)
        {
            return ToElement(item.Summary, "summary")
                .Concat(ToElement(item.Returns, "returns"))
                .Concat(ToElement(item.Remarks, "remarks"))
                .Concat(ToElement(item.Value, "value"));
        }

        private static List<string> ToElement(string text, string tag)
        {
            var lines = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return lines;
            lines.Add($"/// <{tag}>");
            foreach (var line in text.Split('\n'))
            {
                var item = line.Trim();
                if (item.Length == 0)
                    continue;
                item = item.Replace("|see_cref=(", "<see cref=\"")
                    .Replace(")__|", "\" />")
                    .Replace(")_|", "\"/>")
                    .Replace("|c(", "<c>")
                    .Replace(")_c|", "</c>");
                lines.Add($"/// {item}");
            }
            lines.Add($"/// </{tag}>");
            return lines;
        }

        private static (string type, string name) SplitName(string text)
        {
            var parts = text.Split([':'], 2);
            var type = parts[0];
            var name = parts[1];
            return (type, name);
        }

        private static (string parent, string name) FindParent(string text)
        {
            var dot = text.LastIndexOf('.');
            var parent = text.Substring(0, dot);
            var name = text.Substring(dot + 1);
            return (parent, name);
        }

        private static bool IsText(AdditionalText file)
            => file.HasEnding("xml");
    }
}