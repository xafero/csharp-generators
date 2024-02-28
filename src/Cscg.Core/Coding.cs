using System.Collections.Generic;
using System.Text;

namespace Cscg.Core
{
    public static class Coding
    {
        public const string AutoNamespace = $"{nameof(Cscg)}.AutoGen";

        public static string GenerateAttr(string name, string space, string target = "Class")
        {
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"[AttributeUsage(AttributeTargets.{target})]");
            code.AppendLine($"public sealed class {name}Attribute : Attribute");
            code.AppendLine("{");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }

        public static string GenerateIntf(string name, string space,
            IEnumerable<string> lines, params string[] imports)
        {
            var code = new CodeWriter();
            foreach (var @using in imports)
                code.AppendLine($"using {@using};");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"public interface {name}");
            code.AppendLine("{");
            foreach (var line in lines)
                code.AppendLine($"{line.TrimNull()};");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }

        public static string GenerateExt(string name, string space,
            IEnumerable<string[]> lines, params string[] imports)
        {
            var code = new CodeWriter();
            foreach (var @using in imports)
                code.AppendLine($"using {@using};");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"public static class {name}");
            code.AppendLine("{");
            foreach (var line in lines)
                code.AppendLines(line);
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
    }
}