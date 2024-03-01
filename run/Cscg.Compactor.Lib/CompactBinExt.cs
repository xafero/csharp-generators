using System.IO;
using System.Text;

namespace Cscg.Compactor.Lib
{
    public static class CompactBinExt
    {
        public static void ReadBinary(this ICompacted obj, Stream stream)
        {
            using var reader = new BinaryReader(stream, Encoding.UTF8, true);
            // TODO obj.ReadBinary(ref reader);
        }

        public static void WriteBinary(this ICompacted obj, Stream stream)
        {
            using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            // TODO obj.WriteBinary(ref writer);
            writer.Flush();
            stream.Flush();
        }
    }
}