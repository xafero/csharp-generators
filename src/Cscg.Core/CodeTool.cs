﻿using System;
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

        public static string GetAttributeName(string name)
        {
            const string tmp = "Attribute";
            if (!name.EndsWith(tmp))
                name += tmp;
            return name;
        }

        public static string CreateAttribute(string name, string space, CodeWriter body = null,
            AttributeTargets dst = AttributeTargets.Class)
        {
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"[AttributeUsage(AttributeTargets.{dst}, Inherited = false, AllowMultiple = false)]");
            code.AppendLine($"public sealed class {name} : Attribute");
            code.AppendLine("{");
            if (body != null) code.AppendLines(body);
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }

        public static string CreateClass(string name, string space, CodeWriter body = null)
        {
            var code = new CodeWriter();
            code.AppendLine("using System;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"public sealed class {name}");
            code.AppendLine("{");
            if (body != null) code.AppendLines(body);
            code.AppendLine("}");
            code.AppendLine("}");
            return code.ToString();
        }

        public static string GetFullName(string space, string name)
        {
            var fqn = $"{space}.{name}";
            return fqn;
        }
    }
}