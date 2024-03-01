using System.Formats.Cbor;

namespace Cscg.Compactor.Lib
{
    public interface ICompacted
    {
        void WriteCbor(ref CborWriter w);

        void ReadCbor(ref CborReader r);
    }
}