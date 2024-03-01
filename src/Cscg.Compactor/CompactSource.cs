using Cscg.Compactor.Lib;
using Cscg.Core;
using Microsoft.CodeAnalysis;

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

        public static string[] GenerateRead()
        {
            var lines = new[]
            {
                $"public static void ReadCBOR(this {IntObjName} obj, Stream stream)",
                "{", "byte[] array;", "if (stream is MemoryStream mem)", "{",
                "array = mem.ToArray();", "}", "else", "{", "using var copy = new MemoryStream();",
                "stream.CopyTo(copy);", "array = copy.ToArray();", "}",
                "var reader = new CborReader(array, CborConformanceMode.Canonical);",
                "obj.ReadCBOR(ref reader);", "}"
            };
            return lines;
        }

        public static string[] GenerateWrite()
        {
            var lines = new[]
            {
                $"public static void WriteCBOR(this {IntObjName} obj, Stream stream)", "{",
                "var writer = new CborWriter(CborConformanceMode.Canonical, true);",
                "obj.WriteCBOR(ref writer);", "var array = writer.Encode();",
                "stream.Write(array, 0, array.Length);", "stream.Flush();", "}"
            };
            return lines;
        }

        public static void ExtractArg(this AttributeData ad, out DataFormat format)
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
    }
}