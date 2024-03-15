using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using R = System.Xml.XmlReader;
using W = System.Xml.XmlWriter;

#if NETFRAMEWORK
using Drexel;
#endif

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib.Xml
{
    public static class CompactXmlExt
    {
        public static bool ReadBool(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsBoolean();
        }

        public static byte ReadByte(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return XmlConvert.ToByte(r.ReadContentAsString());
        }

        public static byte[] ReadByteArray(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return Convert.FromBase64String(r.ReadContentAsString());
        }

        public static char ReadChar(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return (char)r.ReadContentAsInt();
        }

        public static char[] ReadCharArray(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsString().ToCharArray();
        }

        public static DateTime ReadDateTime(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsDateTime();
        }

        public static DateTimeOffset ReadDateTimeOffset(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsDateTimeOffset();
        }

        public static decimal ReadDecimal(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsDecimal();
        }

        public static IDictionary<string, T> ReadDict<T>(this IXmlCompacted c, string type, ref R r)
        {
            if (IsNull(ref r, false))
                return default;
            var count = int.Parse(r.GetAttribute("count")!);
            var d = new Dictionary<string, T>(count);
            for (var i = 0; i < count; i++)
            {
                r.Read();
                var key = r.GetAttribute("key")!;
                var val = c.ReadOneOf<T>(type, ref r);
                d[key] = val;
            }
            if (!r.IsEmptyElement)
                r.ReadEndElement();
            return d;
        }

        public static double ReadDouble(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsDouble();
        }

        public static T ReadExact<T>(this IXmlCompacted _, string type, ref R r) where T : IXmlCompacted
        {
            if (IsNull(ref r))
                return default;
            var (item, obj) = Reflections.Create<T, IXmlCompacted>(type);
            obj.ReadXml(ref r);
            return item;
        }

        public static float ReadFloat(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsFloat();
        }

        public static Guid ReadGuid(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return new Guid(r.ReadContentAsString());
        }

        public static Half ReadHalf(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return (Half)r.ReadContentAsFloat();
        }

        public static T ReadIntEnum<T>(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return (T)(object)r.ReadContentAsInt();
        }

        public static int ReadInt(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsInt();
        }

        public static List<T> ReadList<T>(this IXmlCompacted c, string type, ref R r)
        {
            var array = c.ReadArray(ref r, (ref R x) => c.ReadOneOf<T>(type, ref x));
            if (array == null)
                return null;
            var d = new List<T>(array);
            return d;
        }

        public static long ReadLong(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsLong();
        }

        public static bool? ReadNullableBool(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsBoolean();
        }

        public static DateTime? ReadNullableDateTime(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsDateTime();
        }

        public static double? ReadNullableDouble(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsDouble();
        }

        public static int? ReadNullableInt(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return r.ReadContentAsInt();
        }

        private static string FindAttribute(string name, ref R r)
        {
            var attr = r.GetAttribute(name);
            if (attr == null)
            {
                r.Read();
                attr = r.GetAttribute(name);
            }
            return attr;
        }

        public static T ReadOneOf<T>(this IXmlCompacted _, string __, ref R r)
        {
            if (IsNull(ref r, false))
                return default;
            var real = FindAttribute("type", ref r);
            var (item, obj) = Reflections.Create<T, IXmlCompacted>(real);
            obj.ReadXml(ref r);
            r.ReadEndElement();
            return item;
        }

        public static sbyte ReadSbyte(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return XmlConvert.ToSByte(r.ReadContentAsString());
        }

        public static short ReadShort(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return XmlConvert.ToInt16(r.ReadContentAsString());
        }

        public static short[] ReadShortArray(this IXmlCompacted c, ref R r)
        {
            return c.ReadArray(ref r, c.ReadShort);
        }

        private static T[] ReadArray<T>(this IXmlCompacted _, ref R r, Reader<T> f)
        {
            if (IsNull(ref r, false))
                return default;
            var count = int.Parse(r.GetAttribute("count")!);
            var array = new T[count];
            for (var i = 0; i < count; i++)
            {
                r.Read();
                array[i] = f(ref r);
            }
            if (!r.IsEmptyElement)
                r.ReadEndElement();
            return array;
        }

        public delegate T Reader<out T>(ref R r);

        private static bool IsNull(ref R r, bool next = true)
        {
            if (r.IsEmptyElement && r.AttributeCount == 0) return true;
            if (next) r.Read();
            return false;
        }

        public static string ReadString(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return null;
            return r.ReadContentAsString();
        }

        public static TimeSpan ReadTimeSpan(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return TimeSpan.Parse(r.ReadContentAsString());
        }

        public static uint ReadUint(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return XmlConvert.ToUInt32(r.ReadContentAsString());
        }

        public static ulong ReadUlong(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return XmlConvert.ToUInt64(r.ReadContentAsString());
        }

        public static ushort ReadUshort(this IXmlCompacted _, ref R r)
        {
            if (IsNull(ref r))
                return default;
            return XmlConvert.ToUInt16(r.ReadContentAsString());
        }

        public static void WriteBool(this IXmlCompacted _, ref W w, bool v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteByte(this IXmlCompacted _, ref W w, byte v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteByteArray(this IXmlCompacted _, ref W w, byte[] v)
        {
            if (v != null)
                w.WriteString(Convert.ToBase64String(v));
            w.WriteEndElement();
        }

        public static void WriteChar(this IXmlCompacted _, ref W w, char v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteCharArray(this IXmlCompacted _, ref W w, char[] v)
        {
            if (v != null)
                w.WriteValue(new string(v));
            w.WriteEndElement();
        }

        public static void WriteDateTime(this IXmlCompacted _, ref W w, DateTime v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteDateTimeOffset(this IXmlCompacted _, ref W w, DateTimeOffset v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteDecimal(this IXmlCompacted _, ref W w, decimal v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteDict<T>(this IXmlCompacted c, string _, ref W w,
            IEnumerable<KeyValuePair<string, T>> v)
        {
            if (v != null)
            {
                var copy = v.ToArray();
                w.WriteAttributeString("count", copy.Length.ToString());
                foreach (var item in copy)
                {
                    w.WriteStartElement("item");
                    w.WriteAttributeString("key", item.Key);
                    c.WriteOneOf(_, ref w, item.Value);
                }
            }
            w.WriteEndElement();
        }

        public static void WriteDouble(this IXmlCompacted _, ref W w, double v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteExact<T>(this IXmlCompacted _, string __, ref W w, T v)
        {
            if (v is IXmlCompacted item)
                item.WriteXml(ref w);
            w.WriteEndElement();
        }

        public static void WriteFloat(this IXmlCompacted _, ref W w, float v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteGuid(this IXmlCompacted _, ref W w, Guid v)
        {
            w.WriteValue(v.ToString());
            w.WriteEndElement();
        }

        public static void WriteHalf(this IXmlCompacted _, ref W w, Half v)
        {
            w.WriteValue((float)v);
            w.WriteEndElement();
        }

        public static void WriteInt(this IXmlCompacted _, ref W w, int v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteIntEnum<T>(this IXmlCompacted _, ref W w, T v)
        {
            w.WriteValue((int)(object)v);
            w.WriteEndElement();
        }

        public static void WriteList<T>(this IXmlCompacted c, string type, ref W w, IEnumerable<T> v)
        {
            c.WriteArray(ref w, v?.ToArray(), (T i, ref W x) => c.WriteOneOf(type, ref x, i));
        }

        public static void WriteLong(this IXmlCompacted _, ref W w, long v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteNullableBool(this IXmlCompacted _, ref W w, bool? v)
        {
            if (v != null)
                w.WriteValue(v.Value);
            w.WriteEndElement();
        }

        public static void WriteNullableDateTime(this IXmlCompacted _, ref W w, DateTime? v)
        {
            if (v != null)
                w.WriteValue(v.Value);
            w.WriteEndElement();
        }

        public static void WriteNullableDouble(this IXmlCompacted _, ref W w, double? v)
        {
            if (v != null)
                w.WriteValue(v.Value);
            w.WriteEndElement();
        }

        public static void WriteNullableInt(this IXmlCompacted _, ref W w, int? v)
        {
            if (v != null)
                w.WriteValue(v.Value);
            w.WriteEndElement();
        }

        public static void WriteOneOf<T>(this IXmlCompacted _, string __, ref W w, T v)
        {
            if (v == null || v is not IXmlCompacted bc)
            {
                w.WriteEndElement();
                return;
            }
            bc.WriteXml(ref w);
            w.WriteEndElement();
        }

        public static void WriteProperty(this IXmlCompacted _, ref W w, string name)
        {
            w.WriteStartElement(name);
        }

        public static void WriteSbyte(this IXmlCompacted _, ref W w, sbyte v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteShort(this IXmlCompacted _, ref W w, short v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        private static void WriteArray<T>(this IXmlCompacted _, ref W w, IReadOnlyCollection<T> v, Writer<T> f)
        {
            if (v != null)
            {
                w.WriteAttributeString("count", v.Count.ToString());
                foreach (var item in v)
                {
                    w.WriteStartElement("item");
                    f(item, ref w);
                }
            }
            w.WriteEndElement();
        }

        public delegate void Writer<in T>(T item, ref W w);

        public static void WriteShortArray(this IXmlCompacted c, ref W w, short[] v)
        {
            c.WriteArray(ref w, v, (short i, ref W x) => c.WriteShort(ref x, i));
        }

        public static void WriteString(this IXmlCompacted _, ref W w, string v)
        {
            if (v != null)
                w.WriteString(v);
            w.WriteEndElement();
        }

        public static void WriteTimeSpan(this IXmlCompacted _, ref W w, TimeSpan v)
        {
            w.WriteValue(v.ToString());
            w.WriteEndElement();
        }

        public static void WriteUint(this IXmlCompacted _, ref W w, uint v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }

        public static void WriteUlong(this IXmlCompacted _, ref W w, ulong v)
        {
            w.WriteValue((decimal)v);
            w.WriteEndElement();
        }

        public static void WriteUshort(this IXmlCompacted _, ref W w, ushort v)
        {
            w.WriteValue(v);
            w.WriteEndElement();
        }
    }
}