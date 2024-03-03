using System;
using System.Collections.Generic;
using System.Text.Json;
using Cscg.Compactor.Lib.Tools;
using R = System.Text.Json.Utf8JsonReader;
using W = System.Text.Json.Utf8JsonWriter;

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib.Json
{
    public static class CompactJsonExt
    {
        public static bool ReadBool(this IJsonCompacted _, ref R r)
        {
            return r.GetBoolean();
        }

        public static byte ReadByte(this IJsonCompacted _, ref R r)
        {
            return r.GetByte();
        }

        public static byte[] ReadByteArray(this IJsonCompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetBytesFromBase64();
        }

        public static char ReadChar(this IJsonCompacted _, ref R r)
        {
            return (char)r.GetInt16();
        }

        public static char[] ReadCharArray(this IJsonCompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetString()?.ToCharArray();
        }

        public static DateTime ReadDateTime(this IJsonCompacted _, ref R r)
        {
            return r.GetDateTime();
        }

        public static DateTimeOffset ReadDateTimeOffset(this IJsonCompacted _, ref R r)
        {
            return r.GetDateTimeOffset();
        }

        public static decimal ReadDecimal(this IJsonCompacted _, ref R r)
        {
            return r.GetDecimal();
        }

        public static IDictionary<string, T> ReadDict<T>(this IJsonCompacted c, string type, ref R r)
        {
            // TODO ?
            return default;
        }

        public static double ReadDouble(this IJsonCompacted _, ref R r)
        {
            return r.GetDouble();
        }

        public static T ReadExact<T>(this IJsonCompacted _, string type, ref R r)
        {
            if (IsNull(ref r))
                return default;
            var (item, obj) = Reflections.Create<T, IJsonCompacted>(type);
            obj.ReadJson(ref r);
            return item;
        }

        public static float ReadFloat(this IJsonCompacted _, ref R r)
        {
            return r.GetSingle();
        }

        public static Guid ReadGuid(this IJsonCompacted _, ref R r)
        {
            return r.GetGuid();
        }

        public static Half ReadHalf(this IJsonCompacted _, ref R r)
        {
            return (Half)r.GetSingle();
        }

        public static T ReadIntEnum<T>(this IJsonCompacted _, ref R r)
        {
            return (T)(object)r.GetInt32();
        }

        public static int ReadInt(this IJsonCompacted _, ref R r)
        {
            return r.GetInt32();
        }

        public static List<T> ReadList<T>(this IJsonCompacted c, string type, ref R r)
        {
            if (IsNull(ref r))
                return default;
            var d = new List<T>();
            while (r.Read())
            {
                if (r.TokenType == JsonTokenType.StartArray)
                    continue;
                if (r.TokenType == JsonTokenType.EndArray)
                    break;
                var (item, obj) = Reflections.Create<T, IJsonCompacted>(type);
                obj?.ReadJson(ref r);
                d.Add(item);
            }
            return d;
        }

        public static long ReadLong(this IJsonCompacted _, ref R r)
        {
            return r.GetInt64();
        }

        public static bool? ReadNullableBool(this IJsonCompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetBoolean();
        }

        public static DateTime? ReadNullableDateTime(this IJsonCompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetDateTime();
        }

        public static double? ReadNullableDouble(this IJsonCompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetDouble();
        }

        public static int? ReadNullableInt(this IJsonCompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetInt32();
        }

        public static T ReadOneOf<T>(this IJsonCompacted c, string type, ref R r)
        {
            // TODO ?
            return default;
        }

        public static sbyte ReadSbyte(this IJsonCompacted _, ref R r)
        {
            return r.GetSByte();
        }

        public static short ReadShort(this IJsonCompacted _, ref R r)
        {
            return r.GetInt16();
        }

        public static short[] ReadShortArray(this IJsonCompacted c, ref R r)
        {
            // TODO ?
            return default;
        }

        public static string ReadString(this IJsonCompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.GetString();
        }

        public static TimeSpan ReadTimeSpan(this IJsonCompacted _, ref R r)
        {
            return TimeSpan.FromTicks(r.GetInt64());
        }

        public static uint ReadUint(this IJsonCompacted _, ref R r)
        {
            return r.GetUInt32();
        }

        public static ulong ReadUlong(this IJsonCompacted _, ref R r)
        {
            return r.GetUInt64();
        }

        public static ushort ReadUshort(this IJsonCompacted _, ref R r)
        {
            return r.GetUInt16();
        }

        public static void WriteBool(this IJsonCompacted _, ref W w, bool v)
        {
            w.WriteBooleanValue(v);
        }

        public static void WriteByte(this IJsonCompacted _, ref W w, byte v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteByteArray(this IJsonCompacted _, ref W w, byte[] v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteBase64StringValue(v);
        }

        public static void WriteChar(this IJsonCompacted _, ref W w, char v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteCharArray(this IJsonCompacted _, ref W w, char[] v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteStringValue(v);
        }

        public static void WriteDateTime(this IJsonCompacted _, ref W w, DateTime v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteDateTimeOffset(this IJsonCompacted _, ref W w, DateTimeOffset v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteDecimal(this IJsonCompacted _, ref W w, decimal v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteDict<T>(this IJsonCompacted _, string type, ref W w, IEnumerable<KeyValuePair<string, T>> v)
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
                ((IJsonCompacted)item.Value).WriteJson(ref w);
            }
            w.WriteEndObject();
        }

        public static void WriteDouble(this IJsonCompacted _, ref W w, double v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteExact<T>(this IJsonCompacted _, string __, ref W w, T v)
        {
            if (v is IJsonCompacted item) item.WriteJson(ref w);
            else w.WriteNullValue();
        }

        public static void WriteFloat(this IJsonCompacted _, ref W w, float v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteGuid(this IJsonCompacted _, ref W w, Guid v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteHalf(this IJsonCompacted _, ref W w, Half v)
        {
            w.WriteNumberValue((float)v);
        }

        public static void WriteInt(this IJsonCompacted _, ref W w, int v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteIntEnum<T>(this IJsonCompacted _, ref W w, T v)
        {
            w.WriteNumberValue((int)(object)v);
        }

        public static void WriteList<T>(this IJsonCompacted _, string type, ref W w, IEnumerable<T> v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteStartArray();
            foreach (var item in v)
            {
                ((IJsonCompacted)item).WriteJson(ref w);
            }
            w.WriteEndArray();
        }

        public static void WriteLong(this IJsonCompacted _, ref W w, long v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteNullableBool(this IJsonCompacted c, ref W w, bool? v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            c.WriteBool(ref w, v.Value);
        }

        public static void WriteNullableDateTime(this IJsonCompacted c, ref W w, DateTime? v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            c.WriteDateTime(ref w, v.Value);
        }

        public static void WriteNullableDouble(this IJsonCompacted c, ref W w, double? v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            c.WriteDouble(ref w, v.Value);
        }

        public static void WriteNullableInt(this IJsonCompacted c, ref W w, int? v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            c.WriteInt(ref w, v.Value);
        }

        public static void WriteOneOf<T>(this IJsonCompacted _, string type, ref W w, T v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            ((IJsonCompacted)v).WriteJson(ref w);
        }

        public static void WriteProperty(this IJsonCompacted _, ref W w, string name)
        {
            w.WritePropertyName(name);
        }

        public static void WriteSbyte(this IJsonCompacted _, ref W w, sbyte v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteShort(this IJsonCompacted _, ref W w, short v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteShortArray(this IJsonCompacted c, ref W w, short[] v)
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

        public static void WriteString(this IJsonCompacted _, ref W w, string v)
        {
            if (v == null)
            {
                w.WriteNullValue();
                return;
            }
            w.WriteStringValue(v);
        }

        public static void WriteTimeSpan(this IJsonCompacted _, ref W w, TimeSpan v)
        {
            w.WriteNumberValue(v.Ticks);
        }

        public static void WriteUint(this IJsonCompacted _, ref W w, uint v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteUlong(this IJsonCompacted _, ref W w, ulong v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteUshort(this IJsonCompacted _, ref W w, ushort v)
        {
            w.WriteNumberValue(v);
        }

        private static bool IsNull(ref R r)
        {
            return r.TokenType == JsonTokenType.Null;
        }
    }
}