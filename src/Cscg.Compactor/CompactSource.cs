using System.Collections.Generic;
using System.Linq;
using Cscg.Compactor.Lib;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Compactor
{
    internal static class CompactSource
    {
        internal const string LibSpace = "Cscg.Compactor.Lib";
        private const string Space = Coding.AutoNamespace;
        internal const string BinObjName = "Compacted";
        private const string ExtObjName = "CompactedExt";
        internal const string IntObjName = "ICompacted";
        private const string ItfObjName = "ICompactor";
        internal const string RflObjName = "Reflections";

        internal static void ExtractArg(this AttributeData ad, out DataFormat format)
        {
            format = default;
            foreach (var (ak, av) in ad.FindArgs())
            {
                if (ak is "0" or "Format")
                {
                    var att = av.Type.ToTrimDisplay();
                    if (av.Value is { } atv)
                        if (att == "Cscg.Compactor.Lib.DataFormat")
                        {
                            format = (DataFormat)(int)atv;
                        }
                }
            }
        }

        internal static CodeWriter GetJsonReadHead(bool isAlone, CodeWriter readerC)
        {
            var readerH = new CodeWriter();
            readerH.AppendLine("r.Read();");
            readerH.AppendLine("string key;");
            readerH.AppendLine("while (r.Read())");
            readerH.AppendLine("{");
            readerH.AppendLine("r.Read();");
            readerH.AppendLine("key = r.GetString();");
            if (isAlone)
                readerH.AppendLines(readerC);
            else
                readerH.AppendLine("ReadJsonCore(ref r, key);");
            readerH.AppendLine("}");
            readerH.AppendLine("r.Read();");
            return readerH;
        }

        internal static CodeWriter GetXmlReadHead(bool isAlone, CodeWriter readerC)
        {
            var readerH = new CodeWriter();
            readerH.AppendLine("r.ReadStartElement();");
            readerH.AppendLine("var count = (int)r.ReadContentAsInt();");
            readerH.AppendLine("string key;");
            readerH.AppendLine("for (var i = 0; i < count; i++)");
            readerH.AppendLine("{");
            readerH.AppendLine("key = r.ReadString();");
            if (isAlone)
                readerH.AppendLines(readerC);
            else
                readerH.AppendLine("ReadXmlCore(ref r, key);");
            readerH.AppendLine("}");
            readerH.AppendLine("r.ReadEndElement();");
            return readerH;
        }

        internal static CodeWriter GetBinReadHead(bool isAlone, CodeWriter readerC)
        {
            var readerH = new CodeWriter();
            readerH.AppendLine("var count = (int)r.Read7BitEncodedInt();");
            readerH.AppendLine("string key;");
            readerH.AppendLine("for (var i = 0; i < count; i++)");
            readerH.AppendLine("{");
            readerH.AppendLine("key = r.ReadString();");
            if (isAlone)
                readerH.AppendLines(readerC);
            else
                readerH.AppendLine("ReadBinaryCore(ref r, key);");
            readerH.AppendLine("}");
            readerH.AppendLine("r.Read7BitEncodedInt();");
            return readerH;
        }

        internal static CodeWriter GetCborReadHead(bool isAlone, CodeWriter readerC)
        {
            var readerH = new CodeWriter();
            readerH.AppendLine("var count = (int)r.ReadStartMap();");
            readerH.AppendLine("string key;");
            readerH.AppendLine("for (var i = 0; i < count; i++)");
            readerH.AppendLine("{");
            readerH.AppendLine("key = r.ReadTextString();");
            if (isAlone)
                readerH.AppendLines(readerC);
            else
                readerH.AppendLine("ReadCborCore(ref r, key);");
            readerH.AppendLine("}");
            readerH.AppendLine("r.ReadEndMap();");
            return readerH;
        }

        internal static CodeWriter GetJsonWriteHead(bool isAlone, CodeWriter writerC)
        {
            var writerH = new CodeWriter();
            writerH.AppendLine("w.WriteStartObject();");
            if (isAlone)
                writerH.AppendLines(writerC);
            else
                writerH.AppendLine("WriteJsonCore(ref w);");
            writerH.AppendLine("w.WriteEndObject();");
            return writerH;
        }

        internal static CodeWriter GetXmlWriteHead(bool isAlone, CodeWriter writerC)
        {
            var writerH = new CodeWriter();
            writerH.AppendLine("w.WriteStartDocument();");
            if (isAlone)
                writerH.AppendLines(writerC);
            else
                writerH.AppendLine("WriteXmlCore(ref w);");
            writerH.AppendLine("w.WriteEndDocument();");
            return writerH;
        }

        internal static CodeWriter GetBinWriteHead(bool isAlone, CodeWriter writerC)
        {
            var writerH = new CodeWriter();
            writerH.AppendLine("w.Write7BitEncodedInt('b');");
            if (isAlone)
                writerH.AppendLines(writerC);
            else
                writerH.AppendLine("WriteBinaryCore(ref w);");
            writerH.AppendLine("w.Write7BitEncodedInt('e');");
            return writerH;
        }

        internal static CodeWriter GetCborWriteHead(bool isAlone, CodeWriter writerC)
        {
            var writerH = new CodeWriter();
            writerH.AppendLine("w.WriteStartMap(null);");
            if (isAlone)
                writerH.AppendLines(writerC);
            else
                writerH.AppendLine("WriteCborCore(ref w);");
            writerH.AppendLine("w.WriteEndMap();");
            return writerH;
        }

        internal static CodeWriter GetReadCode(bool isAlone, bool callBase, string callMode,
            CodeWriter readerH, CodeWriter readerC, string fmt, string clazz)
        {
            var reader = new CodeWriter();
            reader.AppendLine($"public {callMode}void Read{fmt}(ref {clazz} r)");
            reader.AppendLine("{");
            reader.AppendLines(readerH);
            reader.AppendLine("}");

            if (!isAlone)
            {
                reader.AppendLine();
                reader.AppendLine($"public {callMode}void Read{fmt}Core(ref {clazz} r, string key)");
                reader.AppendLine("{");
                if (callBase) reader.AppendLine($"base.Read{fmt}Core(ref r, key);");
                reader.AppendLines(readerC);
                reader.AppendLine("}");
            }
            return reader;
        }

        internal static CodeWriter GetWriteCode(bool isAlone, bool callBase, string callMode,
            CodeWriter writerH, CodeWriter writerC, string fmt, string clazz)
        {
            var writer = new CodeWriter();
            writer.AppendLine($"public {callMode}void Write{fmt}(ref {clazz} w)");
            writer.AppendLine("{");
            writer.AppendLines(writerH);
            writer.AppendLine("}");

            if (!isAlone)
            {
                writer.AppendLine();
                writer.AppendLine($"public {callMode}void Write{fmt}Core(ref {clazz} w)");
                writer.AppendLine("{");
                if (callBase) writer.AppendLine($"base.Write{fmt}Core(ref w);");
                writer.AppendLines(writerC);
                writer.AppendLine("}");
            }
            return writer;
        }

        internal static CodeWriter GetConstruct(ClassDeclarationSyntax cds, IEnumerable<string> registry)
        {
            var construct = new CodeWriter();
            construct.AppendLine($"static {cds.GetClassName()}()");
            construct.AppendLine("{");
            construct.AppendLines(registry.OrderBy(e => e).Distinct());
            construct.AppendLine("}");
            return construct;
        }
    }
}