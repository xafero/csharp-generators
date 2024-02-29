using System;
using System.Globalization;
using System.Linq;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using static Cscg.Core.Sources;

namespace Cscg.Constants
{
    [Generator(LanguageNames.CSharp)]
    public sealed class ConstGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext ctx)
        {
            var files = ctx.AdditionalTextsProvider.Where(IsText);

            var contents = files.Select(ToNamed);

            ctx.RegisterSourceOutput(contents, (spc, nac) =>
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
                var lines = nac.content.Split('\n');
                bool isFirst = true;
                foreach (var rawLine in lines)
                {
                    var line = rawLine.Trim();
                    var parts = line.Split(['='], 2);
                    if (parts.Length != 2)
                        continue;
                    var key = parts[0].Trim();
                    var raw = parts[1].Trim();
                    bool isConst;
                    string type;
                    object val;
                    if (raw.StartsWith("["))
                    {
                        var inner = raw.TrimStart('[').TrimEnd(']')
                            .Split([", "], StringSplitOptions.RemoveEmptyEntries);
                        var parsed = inner.Select(i =>
                        {
                            ParseValue(i, out var ic, out var it, out var iv);
                            return (ic, it, iv);
                        }).ToArray();
                        isConst = false;
                        type = $"{parsed.Select(p => p.it).First()}[]";
                        val = $"[ {string.Join(", ", parsed.Select(p => p.iv))} ]";
                    }
                    else
                    {
                        ParseValue(raw, out isConst, out type, out val);
                    }
                    var mode = isConst ? "const" : "static readonly";
                    if (isFirst)
                        isFirst = false;
                    else
                        code.AppendLine();
                    code.AppendLine($"public {mode} {type} {key} = {val};");
                }
                code.AppendLine("}");
                code.AppendLine("}");
                spc.AddSource(className, code.ToString());
            });
        }

        private static void ParseValue(string raw, out bool isConst, out string type, out object val)
        {
            isConst = true;
            var inv = Sources.Invariant;
            const string prefix = "base64:";
            if (raw.StartsWith(prefix) && raw.Substring(prefix.Length) is { Length: >= 1 } b64)
            {
                isConst = false;
                type = "byte[]";
                var bytes = string.Join(", ", Convert.FromBase64String(b64).Select(b => b));
                val = $"[ {bytes} ]";
            }
            else if (bool.TryParse(raw, out var bv))
            {
                type = "bool";
                val = bv ? "true" : "false";
            }
            else if (DateTime.TryParse(raw, inv, default, out var dt))
            {
                isConst = false;
                type = "DateTime";
                val = $"new DateTime({dt.Year}, {dt.Month}, {dt.Day}, {dt.Hour}, {dt.Minute}, {dt.Second})";
            }
            else if (int.TryParse(raw, out var lv))
            {
                type = "int";
                val = lv;
            }
            else if (double.TryParse(raw, NumberStyles.Any, inv, out var dv))
            {
                type = "double";
                val = dv.ToString(inv);
            }
            else
            {
                type = "string";
                val = raw.StartsWith("\"") ? raw : $"\"{raw}\"";
            }
        }

        private static bool IsText(AdditionalText file)
            => file.HasEnding("txt");
    }
}