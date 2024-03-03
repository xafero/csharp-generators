using System.IO;

namespace Cscg.Compactor.Lib.Binary
{
    public interface IBinCompacted
    {
        void WriteBinary(ref BinaryWriter w);

        void ReadBinary(ref BinaryReader r);
    }
}