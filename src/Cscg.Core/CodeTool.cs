using System;
using System.Linq;
using Microsoft.CodeAnalysis;

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

        public static void WrapForError(this SourceProductionContext ctx, SyntaxWrap syntax, Exec action)
        {
            var cds = syntax.Class;
            var name = cds.GetClassName();
            var fName = name;
            if (syntax.Method is { } m) fName = $"{fName}.{m.Item2.Name}";
            var fileName = $"{fName}.g.cs";
            var code = new CodeWriter();
            try
            {
                action(cds, name, code, syntax);
            }
            catch (Exception e)
            {
                code.AppendLine($"/* {e} */");
            }
            ctx.AddSource(fileName, code.ToString());
        }
    }
}