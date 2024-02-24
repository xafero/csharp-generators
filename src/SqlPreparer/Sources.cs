using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace SqlPreparer
{
    public static class Sources
    {
        public static string GenerateAttr(string name, string space = "Generator", string target = "Class")
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

        public static SourceText From(string source)
        {
            var text = SourceText.From(source, Encoding.UTF8);
            return text;
        }
    }
}