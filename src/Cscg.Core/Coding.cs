using System.Collections.Generic;
using System.Text;

namespace Cscg.Core
{
    public static class Coding
    {
        public const string AutoNamespace = "autogen";

        public static string GenerateAttr(string name, string space, string target = "Class")
        {
            var code = new StringBuilder();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"\t[System.AttributeUsage(System.AttributeTargets.{target})]");
            code.AppendLine($"\tpublic class {name}Attribute : System.Attribute");
            code.AppendLine("\t{");
            code.AppendLine("\t}");
            code.AppendLine("}");
            return code.ToString();
        }

        public static string GenerateIntf(string name, string space, IEnumerable<string> lines)
        {
            var code = new StringBuilder();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"\tpublic interface {name}");
            code.AppendLine("\t{");
            foreach (var line in lines)
            {
                code.AppendLine($"\t\t{line.TrimNull()};");
            }
            code.AppendLine("\t}");
            code.AppendLine("}");
            return code.ToString();
        }

        public static string GenerateExt(string name, string space, IEnumerable<string> lines)
        {
            var code = new StringBuilder();
            code.AppendLine("using System.IO;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"\tpublic static class {name}");
            code.AppendLine("\t{");
            foreach (var line in lines)
            {
                code.AppendLine($"\t\tpublic static {line.TrimNull()}");
            }
            code.AppendLine("\t}");
            code.AppendLine("}");
            return code.ToString();
        }
    }
}