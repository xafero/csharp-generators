using System;

namespace Cscg.Compactor.Lib
{
    [Flags]
    public enum DataFormat
    {
        None = 0,

        Binary = 0b00000001,

        Cbor = 0b00000010,

        Json = 0b00000100,

        Xml = 0b00001000,

        // All = Binary | Cbor | Json | Xml

        All = Json | Cbor
    }
}