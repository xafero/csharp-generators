using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Binary
{
    [Generator(LanguageNames.CSharp)]
    public sealed class BinaryGenerator : IIncrementalGenerator
    {
        private const string Space = Coding.AutoNamespace;
        private const string BinObjName = "BinaryObj";
        private const string ExtObjName = "BinaryObjExt";
        private const string IntObjName = "IBinaryObj";

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(ctx =>
            {
                var attrCode = Coding.GenerateAttr(BinObjName, Space);
                ctx.AddSource($"{BinObjName}Attribute.g.cs", Sources.From(attrCode));

                var intCode = Coding.GenerateIntf(IntObjName, Space, new List<string>
                {
                    "void Read(System.IO.Stream stream)", "void Write(System.IO.Stream stream)"
                });
                ctx.AddSource($"{IntObjName}.g.cs", Sources.From(intCode));

                var extCode = Coding.GenerateExt(ExtObjName, Space, new List<string>());
                ctx.AddSource($"{ExtObjName}.g.cs", Sources.From(extCode));
            });

            var classes = igi.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (sn, _) => sn.HasThisAttribute(BinObjName),
                transform: static (ctx, _) => ctx.GetTarget());
            igi.RegisterSourceOutput(classes, Generate);
        }

        private static void Generate(SourceProductionContext ctx, ClassDeclarationSyntax cds)
        {
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new StringBuilder();
            code.AppendLine("using autogen;");
            code.AppendLine("using System;");
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.IO;");
            code.AppendLine();
            code.AppendLine($"namespace {space};");
            code.AppendLine();
            code.AppendLine($"partial class {name} : {Space}.{IntObjName}");
            code.AppendLine("{");

            var reader = new StringBuilder();
            var writer = new StringBuilder();
            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var propName = pds.GetName();
                    var propType = Typing.Parse(pds.Type, out var rank).ToTitle();

                    var readArg = $"this.{propName}";
                    var readFunc = $"reader.Read{propType}()";
                    if (propType == "DateTime") readFunc = "DateTime.FromBinary(reader.ReadInt64())";
                    else if (propType == "TimeSpan") readFunc = "TimeSpan.FromTicks(reader.ReadInt64())";
                    else if (propType == "Guid") readFunc = "new Guid(reader.ReadBytes(16))";
                    var readLine = $"{readArg} = {readFunc};";
                    if (propType == "String") readLine = BuildNullableRead(propType, propName);
                    if (rank >= 1)
                    {
                        if (propType is "Byte" or "Char") readLine = BuildArrayRead(propType, readArg);
                    }
                    if (propType.StartsWith("_")) readLine = BuildSubRead(propType.Substring(1), readArg);
                    reader.AppendLine($"\t\t{readLine};");

                    var writeArg = $"this.{propName}";
                    if (propType == "DateTime") writeArg += ".ToBinary()";
                    else if (propType == "TimeSpan") writeArg += ".Ticks";
                    else if (propType == "Guid") writeArg += ".ToByteArray()";
                    var writeFunc = $"writer.Write({writeArg})";
                    if (propType == "String") writeFunc = BuildNullableWrite(propName);
                    if (rank >= 1) writeFunc = BuildArrayWrite(writeArg);
                    if (propType.StartsWith("_")) writeFunc = BuildSubWrite(propType.Substring(1), writeArg);
                    writer.AppendLine($"\t\t{writeFunc};");
                }

            code.AppendLine("\tpublic void Read(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tusing var reader = new BinaryReader(stream, Encoding.UTF8, true);");
            code.Append(reader);
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void Write(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tusing var writer = new BinaryWriter(stream, Encoding.UTF8, true);");
            code.Append(writer);
            code.AppendLine("\t}");

            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }

        private static string BuildArrayRead(string type, string prop)
        {
            var code = new StringBuilder();
            var idx = $"{prop.Split(['.'], 2).Last()}_i";
            code.Append($"{prop} = reader.ReadInt32() is var {idx} && {idx} == -1 ? default");
            code.Append($" : reader.Read{type}s({idx})");
            return code.ToString();
        }

        private static string BuildArrayWrite(string prop)
        {
            var code = new StringBuilder();
            code.Append($"if ({prop} == null) {{ writer.Write(-1); }}");
            code.Append($" else {{ writer.Write({prop}.Length); writer.Write({prop}); }}");
            return code.ToString();
        }

        private static string BuildSubRead(string type, string prop)
        {
            var code = new StringBuilder();
            code.Append($"if (typeof({type}).IsEnum) {{ {prop} = ({type})(object)reader.ReadInt32(); }}");
            code.Append($" else {{ if (stream.ReadByte() == 0) {{ {prop} = default; }}");
            code.Append($" else {{ var v = new {type}(); (({IntObjName})(object)v)!.Read(stream); {prop} = v; }} }}");
            return code.ToString();
        }

        private static string BuildSubWrite(string type, string prop)
        {
            var code = new StringBuilder();
            code.Append($"if (typeof({type}).IsEnum) {{ writer.Write((int)(object){prop}); }}");
            code.Append($" else {{ stream.WriteByte((byte)({prop} == default ? 0 : 1));");
            code.Append($" (({IntObjName})(object){prop})?.Write(stream); }}");
            return code.ToString();
        }

        private static string BuildNullableRead(string type, string prop)
        {
            var code = new StringBuilder();
            code.Append($"if (stream.ReadByte() == 0) {{ {prop} = default; }}");
            code.Append($" else {{ {prop} = reader.Read{type}(); }}");
            return code.ToString();
        }

        private static string BuildNullableWrite(string prop)
        {
            var code = new StringBuilder();
            code.Append($"if ({prop} == default) {{ stream.WriteByte((byte)0); }}");
            code.Append($" else {{ stream.WriteByte((byte)1); writer.Write({prop}); }}");
            return code.ToString();
        }
    }
}