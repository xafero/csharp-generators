using System;
using System.Collections.Generic;
using System.Xml;
using R = System.Xml.XmlReader;
using W = System.Xml.XmlWriter;

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib
{
    public static class CompactXmlExt
    {
        public static bool ReadBool(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsBoolean();
        }

        public static byte ReadByte(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return XmlConvert.ToByte(r.ReadContentAsString());
        }

        public static byte[] ReadByteArray(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return Convert.FromBase64String(r.ReadContentAsString());
        }

        public static char ReadChar(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return (char)r.ReadContentAsInt();
        }

        public static char[] ReadCharArray(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsString().ToCharArray();
        }

        public static DateTime ReadDateTime(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsDateTime();
        }

        public static DateTimeOffset ReadDateTimeOffset(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsDateTimeOffset();
        }

        public static decimal ReadDecimal(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsDecimal();
        }

        public static IDictionary<string, T> ReadDict<T>(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            var d = new Dictionary<string, T>();
            // TODO ?
            return d;
        }

        public static double ReadDouble(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsDouble();
        }

        public static T ReadExact<T>(this ICompacted _, string type, ref R r) where T : ICompacted
        {
            if (IsNull(ref r)) return default;
            var v = Reflections.Create<T>(type);
            v.ReadXml(ref r);
            return v;
        }

        public static float ReadFloat(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsFloat();
        }

        public static Guid ReadGuid(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return new Guid(r.ReadContentAsString());
        }

        public static Half ReadHalf(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return (Half)r.ReadContentAsFloat();
        }

        public static T ReadIntEnum<T>(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return (T)(object)r.ReadContentAsInt();
        }

        public static int ReadInt(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsInt();
        }

        public static List<T> ReadList<T>(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            var d = new List<T>();
            // TODO ?
            return d;
        }

        public static long ReadLong(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsLong();
        }

        public static bool? ReadNullableBool(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsBoolean();
        }

        public static DateTime? ReadNullableDateTime(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsDateTime();
        }

        public static double? ReadNullableDouble(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsDouble();
        }

        public static int? ReadNullableInt(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return r.ReadContentAsInt();
        }

        public static T ReadOneOf<T>(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            // TODO ?
            return default;
        }

        public static sbyte ReadSbyte(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return XmlConvert.ToSByte(r.ReadContentAsString());
        }

        public static short ReadShort(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return XmlConvert.ToInt16(r.ReadContentAsString());
        }

        public static short[] ReadShortArray(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            var array = new short[0];
            // TODO ?
            return array;
        }

        private static bool IsNull(ref R r)
        {
            if (r.IsEmptyElement) return true;
            r.Read();
            return false;
        }

        public static string ReadString(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return null;
            return r.ReadContentAsString();
        }

        public static TimeSpan ReadTimeSpan(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return TimeSpan.Parse(r.ReadContentAsString());
        }

        public static uint ReadUint(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return XmlConvert.ToUInt32(r.ReadContentAsString());
        }

        public static ulong ReadUlong(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return XmlConvert.ToUInt64(r.ReadContentAsString());
        }

        public static ushort ReadUshort(this ICompacted _, ref R r)
        {
            if (IsNull(ref r)) return default;
            return XmlConvert.ToUInt16(r.ReadContentAsString());
        }

        public static void WriteBool(this ICompacted _, ref W w, bool v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteByte(this ICompacted _, ref W w, byte v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteByteArray(this ICompacted _, ref W w, byte[] v)
        {
            if (v != null)
                w.WriteString(Convert.ToBase64String(v));
            w.WriteEndElement();
        }

        public static void WriteChar(this ICompacted _, ref W w, char v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteCharArray(this ICompacted _, ref W w, char[] v)
        {
            if (v != null)
                w.WriteValue(new string(v));
            w.WriteEndElement();
        }

        public static void WriteDateTime(this ICompacted _, ref W w, DateTime v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteDateTimeOffset(this ICompacted _, ref W w, DateTimeOffset v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteDecimal(this ICompacted _, ref W w, decimal v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteDict<T>(this ICompacted _, ref W w, IEnumerable<KeyValuePair<string, T>> v)
        {
            // TODO ?
        }

        public static void WriteDouble(this ICompacted _, ref W w, double v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteExact<T>(this ICompacted _, string __, ref W w, T v)
        {
            if (v is ICompacted item) item.WriteXml(ref w);
            w.WriteEndElement();
        }

        public static void WriteFloat(this ICompacted _, ref W w, float v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteGuid(this ICompacted _, ref W w, Guid v)
        {
            w.WriteValue(v.ToString());
            w.WriteEndElement();
        }

        public static void WriteHalf(this ICompacted _, ref W w, Half v)
        {
            w.WriteValue((float)v);
            w.WriteEndElement();
        }

        public static void WriteInt(this ICompacted _, ref W w, int v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteIntEnum<T>(this ICompacted _, ref W w, T v)
        {
            w.WriteValue((int)(object)v);
            w.WriteEndElement();
        }

        public static void WriteList<T>(this ICompacted _, ref W w, IEnumerable<T> v)
        {
            // TODO ?
        }

        public static void WriteLong(this ICompacted _, ref W w, long v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteNullableBool(this ICompacted _, ref W w, bool? v)
        {
            if (v == null)
            {
                w.WriteEndElement();
                return;
            }
            w.WriteValue(v.Value);
            w.WriteEndElement();
        }

        public static void WriteNullableDateTime(this ICompacted _, ref W w, DateTime? v)
        {
            if (v == null)
            {
                w.WriteEndElement();
                return;
            }
            w.WriteValue(v.Value);
            w.WriteEndElement();
        }

        public static void WriteNullableDouble(this ICompacted _, ref W w, double? v)
        {
            if (v == null)
            {
                w.WriteEndElement();
                return;
            }
            w.WriteValue(v.Value);
            w.WriteEndElement();
        }

        public static void WriteNullableInt(this ICompacted _, ref W w, int? v)
        {
            if (v == null)
            {
                w.WriteEndElement();
                return;
            }
            w.WriteValue(v.Value);
            w.WriteEndElement();
        }

        public static void WriteOneOf<T>(this ICompacted _, ref W w, T v)
        {
            if (v == null)
            {
                w.WriteEndElement();
                return;
            }
            // TODO ?
        }

        public static void WriteProperty(this ICompacted _, ref W w, string name)
        {
            w.WriteStartElement(name);
        }

        public static void WriteSbyte(this ICompacted _, ref W w, sbyte v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteShort(this ICompacted _, ref W w, short v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteShortArray(this ICompacted _, ref W w, short[] v)
        {
            if (v == null)
            {
                w.WriteEndElement();
                return;
            }
            // TODO ?
        }

        public static void WriteString(this ICompacted _, ref W w, string v)
        {
            if (v == null)
            {
                w.WriteEndElement();
                return;
            }
            w.WriteString(v);
            w.WriteEndElement();
        }

        public static void WriteTimeSpan(this ICompacted _, ref W w, TimeSpan v)
        {
            w.WriteValue(v.ToString());
            w.WriteEndElement();
        }

        public static void WriteUint(this ICompacted _, ref W w, uint v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteUlong(this ICompacted _, ref W w, ulong v)
        {
            w.WriteValue((decimal)v);
            w.WriteEndElement();
        }

        public static void WriteUshort(this ICompacted _, ref W w, ushort v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }
    }
}