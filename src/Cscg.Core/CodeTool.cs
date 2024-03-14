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

        public static string GetFullName(string space, string name)
        {
            var fqn = $"{space}.{name}";
            return fqn;
        }
    }
}