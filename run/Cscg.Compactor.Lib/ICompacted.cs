using System.Formats.Cbor;
using System.Text.Json;

namespace Cscg.Compactor.Lib
{
    public interface ICompacted
    {
        public void WriteCbor(ref CborWriter w)
        {
        }

        public void ReadCbor(ref CborReader r)
        {
        }

        public void WriteJson(ref Utf8JsonWriter w)
        {
        }

        public void ReadJson(ref Utf8JsonReader r)
        {
        }
    }
}