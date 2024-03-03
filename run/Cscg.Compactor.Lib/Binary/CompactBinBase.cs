using System.IO;
using System.Text;

namespace Cscg.Compactor.Lib.Binary
{
    public static class CompactBinBase
    {
        public static void ReadBinary(this IBinCompacted obj, Stream stream)
        {
            var reader = new BinaryReader(stream, Encoding.UTF8, true);
            obj.ReadBinary(ref reader);
            reader.Dispose();
        }

        public static void WriteBinary(this IBinCompacted obj, Stream stream)
        {
            var writer = new BinaryWriter(stream, Encoding.UTF8, true);
            obj.WriteBinary(ref writer);
            writer.Flush();
            stream.Flush();
            writer.Dispose();
        }
    }
}