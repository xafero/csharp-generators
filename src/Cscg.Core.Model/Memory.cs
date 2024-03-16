using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using XmlTool = Cscg.Core.XmlTool<Cscg.Core.Model.CTypes>;

namespace Cscg.Core.Model
{
    public static class Memory
    {
        private static readonly Dictionary<string, CType> Storage = new();

        public static void Store(ClassDeclarationSyntax cds)
        {
            var model = cds.Extract();
            var key = model.FullName;
            Storage[key] = model;
        }

        public static string Write()
        {
            var root = new CTypes { Types = [] };
            foreach (var pair in Storage)
                root.Types.Add(pair.Value);

            var str = new StringWriter();
            XmlTool.Write(root, str);
            return str.ToString();
        }
    }
}