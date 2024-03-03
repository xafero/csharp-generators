using System.Formats.Cbor;

namespace Cscg.Compactor.Lib.Cbor
{
    public interface ICborCompacted
    {
        void WriteCbor(ref CborWriter w);

        void ReadCbor(ref CborReader r);
    }
}