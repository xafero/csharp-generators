using System.Text;

namespace Cscg.Core
{
    public static class Coding
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
    }
}