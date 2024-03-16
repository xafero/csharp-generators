using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

            var className = $"Stub.{nac.name}";
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLines(CreateUsings(model.Assembly));

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
                                    var rpvType = GuessType(item.Value);
                                    var propType = rpvType ?? "object";
                                    code.AppendLine($"public {propType} {subName} {{ get; set; }}");
                                    break;
                                case "M":
                                    code.AppendLine();
                                    if (!subName.EndsWith(")")) subName += "()";
                                    subName = InsertArgs(item.Params, subName);
                                    const string cSt = "#ctor";
                                    if (subName.StartsWith(cSt))
                                    {
                                        subName = subName.Replace(cSt, tP.name);
                                        code.AppendLines(ToElements(item));
                                        code.AppendLine($"public {subName}");
                                        code.AppendLine("{");
                                        code.AppendLine(ThrowError());
                                        code.AppendLine("}");
                                        continue;
                                    }
                                    code.AppendLines(ToElements(item));
                                    var retType = GuessType(item.Returns);
                                    var metType = retType ?? "void";
                                    code.AppendLine($"public {metType} {subName}");
                                    code.AppendLine("{");
                                    code.AppendLine(ThrowError());
                                    code.AppendLine("}");
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

        private static IEnumerable<string> CreateUsings(XmlAssembly ass)
        {
            var text = ass.Name;
            var last = string.Empty;
            foreach (var part in text.Split('.'))
            {
                last += $".{part}";
                yield return $"using {last.Trim('.')};";
            }
        }

        private static string GuessType(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return null;

            text = PatchBack(text);

            var tmp = text.Split(["<see cref=\"T:"], StringSplitOptions.None);
            if (tmp.Length >= 2)
                return tmp[1].Split('"')[0].Trim('1', '`');

            if (text.Contains("<c>true</c>"))
                return "bool";

            return "object";
        }

        private static string ThrowError(string type = "NotImplementedException")
        {
            return $"throw new {type}();";
        }

        private static string InsertArgs(List<XmlParam> @params, string text)
        {
            var parts = text.Split(['('], 2);
            var first = parts[0];
            var second = parts[1];
            var bld = new StringBuilder();
            bld.Append(first);
            bld.Append('(');
            var paramIdx = 0;
            foreach (var c in second)
            {
                if (char.IsLetterOrDigit(c) || c == '.' || c == '[' || c == ']')
                {
                    bld.Append(c);
                    continue;
                }
                if (c == ',' || c == ')')
                {
                    if (paramIdx < @params.Count)
                        bld.Append($" {@params[paramIdx++].Name}, ");
                }
            }
            return bld.ToString().Trim(' ', ',') + ')';
        }

        private static IEnumerable<string> ToElements(XmlMember item)
        {
            var lines = new List<string>();
            lines.AddRange(ToElement(item.Summary, "summary"));
            foreach (var ip in item.Params)
            {
                var ipTag = $"param name=\"{ip.Name}\"";
                lines.AddRange(ToElement(ip.Description, ipTag, "param"));
            }
            lines.AddRange(ToElement(item.Returns, "returns"));
            lines.AddRange(ToElement(item.Remarks, "remarks"));
            lines.AddRange(ToElement(item.Value, "value"));
            return lines;
        }

        private static List<string> ToElement(string text, string tag, string end = null)
        {
            var lines = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return lines;
            lines.Add($"/// <{tag}>");
            foreach (var line in text.Split('\n'))
            {
                var item = line.Trim();
                if (item.Length == 0)
                    continue;
                item = PatchBack(item);
                lines.Add($"/// {item}");
            }
            lines.Add($"/// </{end ?? tag}>");
            return lines;
        }

        private static string PatchBack(string item)
        {
            return item.Replace("|see_cref=(", "<see cref=\"")
                .Replace(")__|", "\" />")
                .Replace(")_|", "\"/>")
                .Replace("|c(", "<c>")
                .Replace(")_c|", "</c>");
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