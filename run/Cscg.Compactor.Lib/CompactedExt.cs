using System.IO;

namespace Cscg.Compactor.Lib
{
    public static class CompactedExt
    {
        public static byte[] ToBytes(this Stream stream)
        {
            byte[] array;
            if (stream is MemoryStream mem)
            {
                array = mem.ToArray();
            }
            else
            {
                using var copy = new MemoryStream();
                stream.CopyTo(copy);
                array = copy.ToArray();
            }
            return array;
        }
    }
}
