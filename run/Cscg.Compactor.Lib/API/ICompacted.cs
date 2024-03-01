using System;
using System.Formats.Cbor;
using System.IO;
using System.Text.Json;
using System.Xml;

namespace Cscg.Compactor.Lib
{
    public interface ICborCompacted
    {
        public void WriteCbor(ref CborWriter w)
        {
            throw new NotImplementedException();
        }

        public void ReadCbor(ref CborReader r)
        {
            throw new NotImplementedException();
        }
    }

    public interface IJsonCompacted
    {
        public void WriteJson(ref Utf8JsonWriter w)
        {
            throw new NotImplementedException();
        }

        public void ReadJson(ref Utf8JsonReader r)
        {
            throw new NotImplementedException();
        }
    }

    public interface IBinCompacted
    {
        public void WriteBinary(ref BinaryWriter w)
        {
            throw new NotImplementedException();
        }

        public void ReadBinary(ref BinaryReader r)
        {
            throw new NotImplementedException();
        }
    }

    public interface IXmlCompacted
    {
        public void WriteXml(ref XmlWriter w)
        {
            throw new NotImplementedException();
        }

        public void ReadXml(ref XmlReader r)
        {
            throw new NotImplementedException();
        }
    }

    public interface ICompacted : IBinCompacted, IXmlCompacted, IJsonCompacted, ICborCompacted
    {
    }
}