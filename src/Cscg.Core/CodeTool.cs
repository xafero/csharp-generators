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
            if (interfaceStr.Length != 0)
                interfaceStr = $" : {interfaceStr}";
            code.AppendLine($"partial {type} {name}{interfaceStr}");
        }

        public static string CreateAttribute(string name, string space)
        {
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
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

        public static string CreateClass(string name, string space, CodeWriter body)
        {
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"public sealed class {name}");
            code.AppendLine("{");
            code.AppendLines(body);
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }
    }
}