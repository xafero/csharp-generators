using System.Text.Json;

namespace Cscg.Compactor.Lib.Json
{
    public interface IJsonCompacted
    {
        void WriteJson(ref Utf8JsonWriter w);

        void ReadJson(ref Utf8JsonReader r);
    }
}