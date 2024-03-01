using System.Formats.Cbor;
using System.IO;

namespace Cscg.Compactor.Lib
{
    public static class CompactCborBase
    {
        public static void ReadCbor(this ICompacted obj, Stream stream)
        {
            var array = stream.ToBytes();
            var reader = new CborReader(array, CborConformanceMode.Canonical);
            obj.ReadCbor(ref reader);
        }

        public static void WriteCbor(this ICompacted obj, Stream stream)
        {
            var writer = new CborWriter(CborConformanceMode.Canonical, true);
            obj.WriteCbor(ref writer);
            var array = writer.Encode();
            stream.Write(array, 0, array.Length);
            stream.Flush();
        }
    }
}