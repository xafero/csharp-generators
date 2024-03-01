using System;
using System.Collections.Generic;
using System.Text.Json;
using R = System.Text.Json.Utf8JsonReader;
using W = System.Text.Json.Utf8JsonWriter;

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib
{
    public static class CompactJsonExt
    {
        public static bool ReadBool(this ICompacted _, ref R r)
        {
            return r.GetBoolean();
        }

        public static byte ReadByte(this ICompacted _, ref R r)
        {
            return r.GetByte();
        }

        public static byte[] ReadByteArray(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetBytesFromBase64();
        }

        public static char ReadChar(this ICompacted _, ref R r)
        {
            return (char)r.GetInt16();
        }

        public static char[] ReadCharArray(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetString()?.ToCharArray();
        }

        public static DateTime ReadDateTime(this ICompacted _, ref R r)
        {
            return r.GetDateTime();
        }

        public static DateTimeOffset ReadDateTimeOffset(this ICompacted _, ref R r)
        {
            return r.GetDateTimeOffset();
        }

        public static decimal ReadDecimal(this ICompacted _, ref R r)
        {
            return r.GetDecimal();
        }

        public static IDictionary<string, T> ReadDict<T>(this ICompacted c, ref R r)
        {
            throw new NotImplementedException();
        }

        public static double ReadDouble(this ICompacted _, ref R r)
        {
            return r.GetDouble();
        }

        public static T ReadExact<T>(this ICompacted _, string type, ref R r) where T : ICompacted
        {
            var obj = Reflections.Create<T>(type);
            obj.ReadJson(ref r);
            return obj;
        }

        public static float ReadFloat(this ICompacted _, ref R r)
        {
            return r.GetSingle();
        }

        public static Guid ReadGuid(this ICompacted _, ref R r)
        {
            return r.GetGuid();
        }

        public static Half ReadHalf(this ICompacted _, ref R r)
        {
            return (Half)r.GetInt16();
        }

        public static T ReadIntEnum<T>(this ICompacted _, ref R r)
        {
            return (T)(object)r.GetInt32();
        }

        public static int ReadInt(this ICompacted _, ref R r)
        {
            return r.GetInt32();
        }

        public static List<T> ReadList<T>(this ICompacted c, ref R r)
        {
            throw new NotImplementedException();
        }

        public static long ReadLong(this ICompacted _, ref R r)
        {
            return r.GetInt64();
        }

        public static bool? ReadNullableBool(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetBoolean();
        }

        public static DateTime? ReadNullableDateTime(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetDateTime();
        }

        public static double? ReadNullableDouble(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetDouble();
        }

        public static int? ReadNullableInt(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetInt32();
        }

        public static T ReadOneOf<T>(this ICompacted c, ref R r)
        {
            throw new NotImplementedException();
        }

        public static sbyte ReadSbyte(this ICompacted _, ref R r)
        {
            return r.GetSByte();
        }

        public static short ReadShort(this ICompacted _, ref R r)
        {
            return r.GetInt16();
        }

        public static short[] ReadShortArray(this ICompacted c, ref R r)
        {
            throw new NotImplementedException();
        }

        public static string ReadString(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetString();
        }

        public static TimeSpan ReadTimeSpan(this ICompacted _, ref R r)
        {
            return TimeSpan.FromTicks(r.GetInt64());
        }

        public static uint ReadUint(this ICompacted _, ref R r)
        {
            return r.GetUInt32();
        }

        public static ulong ReadUlong(this ICompacted _, ref R r)
        {
            return r.GetUInt64();
        }

        public static ushort ReadUshort(this ICompacted _, ref R r)
        {
            return r.GetUInt16();
        }

        public static void WriteBool(this ICompacted _, ref W w, bool v)
        {
            w.WriteBooleanValue(v);
        }

        public static void WriteByte(this ICompacted _, ref W w, byte v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteByteArray(this ICompacted _, ref W w, byte[] v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteBase64StringValue(v);
        }

        public static void WriteChar(this ICompacted _, ref W w, char v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteCharArray(this ICompacted _, ref W w, char[] v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteStringValue(v);
        }

        public static void WriteDateTime(this ICompacted _, ref W w, DateTime v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteDateTimeOffset(this ICompacted _, ref W w, DateTimeOffset v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteDecimal(this ICompacted _, ref W w, decimal v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteDict<T>(this ICompacted _, ref W w, IEnumerable<KeyValuePair<string, T>> v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteStartObject();
            foreach (var item in v)
            {
                w.WritePropertyName(item.Key);
                ((ICompacted)item.Value).WriteJson(ref w);
            }
            w.WriteEndObject();
        }

        public static void WriteDouble(this ICompacted _, ref W w, double v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteExact<T>(this ICompacted _, string __, ref W w, T v)
        {
            if (v is ICompacted item) item.WriteJson(ref w);
            else w.WriteNullValue();
        }

        public static void WriteFloat(this ICompacted _, ref W w, float v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteGuid(this ICompacted _, ref W w, Guid v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteHalf(this ICompacted _, ref W w, Half v)
        {
            w.WriteNumberValue((int)v);
        }

        public static void WriteInt(this ICompacted _, ref W w, int v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteIntEnum<T>(this ICompacted _, ref W w, T v)
        {
            w.WriteNumberValue((int)(object)v);
        }

        public static void WriteList<T>(this ICompacted _, ref W w, IEnumerable<T> v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteStartArray();
            foreach (var item in v)
            {
                ((ICompacted)item).WriteJson(ref w);
            }
            w.WriteEndArray();
        }

        public static void WriteLong(this ICompacted _, ref W w, long v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteNullableBool(this ICompacted c, ref W w, bool? v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            c.WriteBool(ref w, v.Value);
        }

        public static void WriteNullableDateTime(this ICompacted c, ref W w, DateTime? v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            c.WriteDateTime(ref w, v.Value);
        }

        public static void WriteNullableDouble(this ICompacted c, ref W w, double? v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            c.WriteDouble(ref w, v.Value);
        }

        public static void WriteNullableInt(this ICompacted c, ref W w, int? v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            c.WriteInt(ref w, v.Value);
        }

        public static void WriteOneOf<T>(this ICompacted _, ref W w, T v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            ((ICompacted)v).WriteJson(ref w);
        }

        public static void WriteProperty(this ICompacted _, ref W w, string name)
        {
            w.WritePropertyName(name);
        }

        public static void WriteSbyte(this ICompacted _, ref W w, sbyte v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteShort(this ICompacted _, ref W w, short v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteShortArray(this ICompacted c, ref W w, short[] v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteStartArray();
            foreach (var item in v) c.WriteShort(ref w, item);
            w.WriteEndArray();
        }

        public static void WriteString(this ICompacted _, ref W w, string v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteStringValue(v);
        }

        public static void WriteTimeSpan(this ICompacted _, ref W w, TimeSpan v)
        {
            w.WriteNumberValue(v.Ticks);
        }

        public static void WriteUint(this ICompacted _, ref W w, uint v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteUlong(this ICompacted _, ref W w, ulong v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteUshort(this ICompacted _, ref W w, ushort v)
        {
            w.WriteNumberValue(v);
        }

        private static bool IsNull(ref R r)
        {
            return r.TokenType == JsonTokenType.Null;
        }
    }
}