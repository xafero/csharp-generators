using System;
using System.Threading;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Cscg.AdoNet.AdoSource;
using static Cscg.Core.CodeTool;

namespace Cscg.AdoNet
{
    [Generator(LanguageNames.CSharp)]
    public sealed class AdoGenerator : IIncrementalGenerator
    {
        private static readonly string TableAn = GetAttributeName(TableAttrName);
        private static readonly string ColAn = GetAttributeName(ColAttrName);
        private static readonly string KeyAn = GetAttributeName(KeyAttrName);

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(PostInitial);

            var sp = igi.SyntaxProvider;
            var tableAf = GetFullName(LibSpace, TableAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(tableAf, Check, Wrap), Exec);
        }

        private static void PostInitial(IncrementalGeneratorPostInitializationContext ctx)
        {
            var tableAc = CreateAttribute(TableAn, LibSpace, new CodeWriter
            {
                Lines = { "public string Name { get; set; }" }
            }, default, default, AttributeTargets.Class);
            ctx.AddSource($"{TableAn}.cs", tableAc);

            var colAc = CreateAttribute(ColAn, LibSpace, new CodeWriter
            {
                Lines = { "public string Name { get; set; }" }
            }, default, default, AttributeTargets.Property, AttributeTargets.Field);
            ctx.AddSource($"{ColAn}.cs", colAc);

            var keyAc = CreateAttribute(KeyAn, LibSpace, new CodeWriter
            {
                Lines = { }
            }, default, default, AttributeTargets.Property, AttributeTargets.Field);
            ctx.AddSource($"{KeyAn}.cs", keyAc);

            // var dbsBody = new CodeWriter();
            // var dbsCode = CodeTool.CreateClass($"{DbsName}<T>", LibSpace, dbsBody);
            // ctx.AddSource($"{DbsName}.cs", dbsCode);
        }

        private static bool Check(SyntaxNode node, CancellationToken _)
            => node is ClassDeclarationSyntax;

        private static SyntaxWrap Wrap(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
            => ctx.Wrap();

        private static void Exec(SourceProductionContext ctx, SyntaxWrap syntax)
        {
            var cds = syntax.Class;
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new CodeWriter();
            code.AddUsings(LibSpace, "System");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.WriteClassLine(name);
            code.AppendLine("{");

            var body = new CodeWriter();
            body.AppendLine("public static string CreateTable()");
            body.AppendLine("{");
            var table = SqliteSource.Quote(BuildPlural(name));
            body.AppendLine("var sql = string.Join(Environment.NewLine, [");
            body.AppendLine($"@\"CREATE TABLE \"{table}\" (\",");

            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var pp = syntax.GetInfo(syntax.GetSymbol(pds));
                    var pName = SqliteSource.Quote(pp.Name);
                    var (pType, pCond) = SqliteSource.GetType(pp.ReturnType);
                    body.AppendLine($"@\"    \"{pName}\" {pType} {pCond},\",");
                }

            body.ModifyLast(l => l.Replace(",\",", "\","));
            body.AppendLine("\");\"");
            body.AppendLine("]);");
            body.AppendLine("return sql;");

            body.AppendLine("}");
            code.AppendLines(body);

            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }
    }
}