using System;
using System.Collections.Generic;
using System.Formats.Cbor;
using R = System.Formats.Cbor.CborReader;
using W = System.Formats.Cbor.CborWriter;

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib
{
    public static class CompactCborExt
    {
        public static void WriteProperty(this ICompacted _, ref W w, string v)
        {
            w.WriteTextString(v);
        }

        public static string ReadString(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadTextString();
        }

        public static void WriteString(this ICompacted _, ref W w, string v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            w.WriteTextString(v);
        }

        public static void WriteInt(this ICompacted _, ref W w, int v)
        {
            w.WriteInt32(v);
        }

        public static void WriteIntEnum<T>(this ICompacted _, ref W w, T v)
            where T : Enum
        {
            w.WriteInt32((int)(object)v);
        }

        public static int ReadInt(this ICompacted _, ref R r)
        {
            return r.ReadInt32();
        }

        public static T ReadIntEnum<T>(this ICompacted _, ref R r)
            where T : Enum
        {
            return (T)(object)r.ReadInt32();
        }

        public static float ReadFloat(this ICompacted _, ref R r)
        {
            return r.ReadSingle();
        }

        public static bool ReadBool(this ICompacted _, ref R r)
        {
            return r.ReadBoolean();
        }

        public static long ReadLong(this ICompacted _, ref R r)
        {
            return r.ReadInt64();
        }

        public static short ReadShort(this ICompacted _, ref R r)
        {
            return (short)r.ReadInt32();
        }

        public static byte ReadByte(this ICompacted _, ref R r)
        {
            return (byte)r.ReadInt32();
        }

        public static Guid ReadGuid(this ICompacted _, ref R r)
        {
            return new Guid(r.ReadByteString());
        }

        public static TimeSpan ReadTimeSpan(this ICompacted _, ref R r)
        {
            return TimeSpan.FromTicks(r.ReadInt64());
        }

        public static DateTime ReadDateTime(this ICompacted _, ref R r)
        {
            return DateTime.FromBinary(r.ReadInt64());
        }

        public static ulong ReadUlong(this ICompacted _, ref R r)
        {
            return r.ReadUInt64();
        }

        public static DateTimeOffset ReadDateTimeOffset(this ICompacted _, ref R r)
        {
            return r.ReadDateTimeOffset();
        }

        public static uint ReadUint(this ICompacted _, ref R r)
        {
            return r.ReadUInt32();
        }

        public static ushort ReadUshort(this ICompacted _, ref R r)
        {
            return (ushort)r.ReadInt32();
        }

        public static sbyte ReadSbyte(this ICompacted _, ref R r)
        {
            return (sbyte)r.ReadInt32();
        }

        public static decimal ReadDecimal(this ICompacted _, ref R r)
        {
            return r.ReadDecimal();
        }

        public static double ReadDouble(this ICompacted _, ref R r)
        {
            return r.ReadDouble();
        }

        public static T ReadExact<T>(this ICompacted _, string type, ref R r)
            where T : ICompacted
        {
            if (IsNull(ref r)) 
                return default;
            var (item, obj) = Reflections.Create<T, ICborCompacted>(type);
            obj.ReadCbor(ref r);
            return item;
        }

        public static Half ReadHalf(this ICompacted _, ref R r)
        {
            return r.ReadHalf();
        }

        public static char[] ReadCharArray(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadTextString().ToCharArray();
        }

        public static char ReadChar(this ICompacted _, ref R r)
        {
            return (char)r.ReadInt32();
        }

        public static byte[] ReadByteArray(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadByteString();
        }

        public static bool? ReadNullableBool(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadBoolean();
        }

        public static int? ReadNullableInt(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadInt32();
        }

        public static double? ReadNullableDouble(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadDouble();
        }

        public static DateTime? ReadNullableDateTime(this ICompacted c, ref R r)
        {
            return IsNull(ref r) ? null : c.ReadDateTime(ref r);
        }

        public static void WriteFloat(this ICompacted _, ref W w, float v)
        {
            w.WriteSingle(v);
        }

        public static void WriteBool(this ICompacted _, ref W w, bool v)
        {
            w.WriteBoolean(v);
        }

        public static void WriteNullableDateTime(this ICompacted c, ref W w, DateTime? v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            c.WriteDateTime(ref w, v.Value);
        }

        public static void WriteNullableInt(this ICompacted c, ref W w, int? v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            c.WriteInt(ref w, v.Value);
        }

        public static void WriteNullableDouble(this ICompacted c, ref W w, double? v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            c.WriteDouble(ref w, v.Value);
        }

        public static void WriteNullableBool(this ICompacted c, ref W w, bool? v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            c.WriteBool(ref w, v.Value);
        }

        public static void WriteGuid(this ICompacted _, ref W w, Guid v)
        {
            w.WriteByteString(v.ToByteArray());
        }

        public static void WriteTimeSpan(this ICompacted _, ref W w, TimeSpan v)
        {
            w.WriteInt64(v.Ticks);
        }

        public static void WriteDateTime(this ICompacted _, ref W w, DateTime v)
        {
            w.WriteInt64(v.ToBinary());
        }

        public static void WriteDateTimeOffset(this ICompacted _, ref W w, DateTimeOffset v)
        {
            w.WriteDateTimeOffset(v);
        }

        public static void WriteUlong(this ICompacted _, ref W w, ulong v)
        {
            w.WriteUInt64(v);
        }

        public static void WriteUint(this ICompacted _, ref W w, uint v)
        {
            w.WriteUInt32(v);
        }

        public static void WriteUshort(this ICompacted _, ref W w, ushort v)
        {
            w.WriteInt32(v);
        }

        public static void WriteSbyte(this ICompacted _, ref W w, sbyte v)
        {
            w.WriteInt32(v);
        }

        public static void WriteDouble(this ICompacted _, ref W w, double v)
        {
            w.WriteDouble(v);
        }

        public static void WriteExact<T>(this ICompacted _, string __, ref W w, T v)
        {
            if (v is ICompacted c)
                c.WriteCbor(ref w);
            else
                w.WriteNull();
        }

        public static void WriteDecimal(this ICompacted _, ref W w, decimal v)
        {
            w.WriteDecimal(v);
        }

        public static void WriteHalf(this ICompacted _, ref W w, Half v)
        {
            w.WriteHalf(v);
        }

        public static void WriteCharArray(this ICompacted _, ref W w, char[] v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            w.WriteTextString(v);
        }

        public static void WriteChar(this ICompacted _, ref W w, char v)
        {
            w.WriteInt32(v);
        }

        public static void WriteByteArray(this ICompacted _, ref W w, byte[] v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            w.WriteByteString(v);
        }

        public static void WriteByte(this ICompacted _, ref W w, byte v)
        {
            w.WriteInt32(v);
        }

        public static void WriteShort(this ICompacted _, ref W w, short v)
        {
            w.WriteInt32(v);
        }

        public static void WriteLong(this ICompacted _, ref W w, long v)
        {
            w.WriteInt64(v);
        }

        public static T ReadOneOf<T>(this ICompacted _, string type, ref R r)
        {
            if (IsNull(ref r)) return default;
            var (item, obj) = Reflections.Create<T, ICborCompacted>(type);
            obj.ReadCbor(ref r);
            return item;
        }

        public static void WriteOneOf<T>(this ICompacted _, string type, ref W w, T v)
        {
            if (v is ICompacted item)
                item.WriteCbor(ref w);
            else
                w.WriteNull();
        }

        public static List<T> ReadList<T>(this ICompacted _, string type, ref R r)
        {
            if (IsNull(ref r)) return default;
            var size = (int)r.ReadStartArray();
            var d = new List<T>(size);
            for (var j = 0; j < size; j++)
            {
                var (dv, dvi) = Reflections.Create<T, ICborCompacted>(type);
                dvi.ReadCbor(ref r);
                d.Add(dv);
            }
            r.ReadEndArray();
            return d;
        }

        public static Dictionary<string, T> ReadDict<T>(this ICompacted _, string type, ref R r)
        {
            if (IsNull(ref r)) return default;
            var size = (int)r.ReadStartMap();
            var d = new Dictionary<string, T>(size);
            for (var j = 0; j < size; j++)
            {
                var dk = r.ReadTextString();
                var (dv, dvi) = Reflections.Create<T, ICborCompacted>(type);
                dvi.ReadCbor(ref r);
                d[dk] = dv;
            }
            r.ReadEndMap();
            return d;
        }

        public static short[] ReadShortArray(this ICompacted c, ref R r)
        {
            if (IsNull(ref r)) return default;
            var count = (int)r.ReadStartArray();
            var array = new short[count];
            for (var i = 0; i < count; i++)
                array[i] = c.ReadShort(ref r);
            return array;
        }

        private static bool IsNull(ref R r)
        {
            var isNull = r.PeekState() == CborReaderState.Null;
            if (isNull) r.ReadNull();
            return isNull;
        }

        public static void WriteList<T>(this ICompacted _, string type, ref W w, IEnumerable<T> v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            w.WriteStartArray(null);
            foreach (var item in v)
                ((ICompacted)item).WriteCbor(ref w);
            w.WriteEndArray();
        }

        public static void WriteArray<T>(this ICompacted _, ref W w, T[] v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            w.WriteStartArray(null);
            foreach (var item in v)
                ((ICompacted)item).WriteCbor(ref w);
            w.WriteEndArray();
        }

        private static T[] ReadArray<T>(this ICompacted _, Func<T> create, ref R r)
        {
            if (IsNull(ref r))
                return null;
            var size = (int)r.ReadStartArray();
            var v = new T[size];
            for (var j = 0; j < size; j++)
            {
                if (create() is ICompacted c)
                {
                    c.ReadCbor(ref r);
                    v[j] = (T)c;
                }
            }
            r.ReadEndArray();
            return v;
        }

        public static void WriteDict<T>(this ICompacted _, string type, ref W w, IEnumerable<KeyValuePair<string, T>> v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            w.WriteStartMap(null);
            foreach (var item in v)
            {
                var key = item.Key;
                var val = item.Value;
                w.WriteTextString(key);
                if (val is ICompacted c)
                    c.WriteCbor(ref w);
                else
                    w.WriteNull();
            }
            w.WriteEndMap();
        }

        public static void WriteShortArray(this ICompacted c, ref W w, short[] v)
        {
            if (v == null)
            {
                w.WriteNull();
                return;
            }
            w.WriteStartArray(null);
            foreach (var item in v)
                c.WriteShort(ref w, item);
            w.WriteEndArray();
        }
    }
}