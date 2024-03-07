using System.Linq;

namespace Cscg.Core
{
    public static class CodeTool
    {
        public static void AddUsings(this CodeWriter code, params string[] usings)
        {
            foreach (var @using in usings.OrderBy(e => e).Distinct())
                code.AppendLine($"using {@using};");
        }

        public static void WriteClassLine(this CodeWriter code, string name,
            string type = "class", params string[] interfaces)
        {
            var interfaceStr = string.Join(", ", interfaces);
            code.AppendLine($"partial {type} {name} : {interfaceStr}");
        }

        public static string CreateAttribute(string name)
        {
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine("namespace Cscg.AdoNet.Lib");
            code.AppendLine("{");
            code.AppendLine("[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]");
            code.AppendLine($"public sealed class {name} : Attribute");
            code.AppendLine("{");
            code.AppendLine($"public {name}()");
            code.AppendLine("{");
            code.AppendLine("}");
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
    }
}