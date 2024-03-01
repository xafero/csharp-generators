using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib
{
    public static class CompactJsonExt
    {
        public static void ReadJson(this ICompacted obj, Stream stream)
        {
            var array = stream.ToBytes();
            var reader = new Utf8JsonReader(array, new JsonReaderOptions
            {
                CommentHandling = JsonCommentHandling.Disallow
            });
            obj.ReadJson(ref reader);
        }

        public static void WriteJson(this ICompacted obj, Stream stream)
        {
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = true
            });
            obj.WriteJson(ref writer);
            writer.Flush();
            stream.Flush();
        }

        public static float ReadFloat(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetSingle();
        }

        public static string ReadString(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetString();
        }

        public static int ReadInt(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetInt32();
        }

        public static bool ReadBool(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetBoolean();
        }

        public static long ReadLong(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetInt64();
        }

        public static short ReadShort(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetInt16();
        }

        public static byte ReadByte(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetByte();
        }

        public static byte[] ReadByteArray(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetBytesFromBase64();
        }

        public static char ReadChar(this ICompacted _, ref Utf8JsonReader r)
        {
            return (char)r.GetInt16();
        }

        public static char[] ReadCharArray(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetString()?.ToCharArray();
        }

        public static Half ReadHalf(this ICompacted _, ref Utf8JsonReader r)
        {
            return (Half)r.GetInt16();
        }

        public static decimal ReadDecimal(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetDecimal();
        }

        public static T ReadIntEnum<T>(this ICompacted _, ref Utf8JsonReader r)
        {
            return (T)(object)r.GetInt32();
        }

        public static T ReadExact<T>(this ICompacted _, string type, ref Utf8JsonReader r)
            where T : ICompacted
        {
            return default(T); // TODO
        }

        public static double ReadDouble(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetDouble();
        }

        public static sbyte ReadSbyte(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetSByte();
        }

        public static ushort ReadUshort(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetUInt16();
        }

        public static uint ReadUint(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetUInt32();
        }

        public static ulong ReadUlong(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetUInt64();
        }

        public static DateTimeOffset ReadDateTimeOffset(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetDateTimeOffset();
        }

        public static DateTime ReadDateTime(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetDateTime();
        }

        public static DateTime ReadNullableDateTime(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetDateTime();
        }

        public static TimeSpan ReadTimeSpan(this ICompacted _, ref Utf8JsonReader r)
        {
            return TimeSpan.FromTicks(r.GetInt64());
        }

        public static Guid ReadGuid(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetGuid();
        }

        public static bool? ReadNullableBool(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetBoolean();
        }

        public static int? ReadNullableInt(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetInt32();
        }

        public static double? ReadNullableDouble(this ICompacted _, ref Utf8JsonReader r)
        {
            return r.GetDouble();
        }

        public static void WriteProperty(this ICompacted _, ref Utf8JsonWriter w, string name)
        {
            w.WritePropertyName(name);
        }

        public static void WriteFloat(this ICompacted _, ref Utf8JsonWriter w, float v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteString(this ICompacted _, ref Utf8JsonWriter w, string v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteInt(this ICompacted _, ref Utf8JsonWriter w, int v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteBool(this ICompacted _, ref Utf8JsonWriter w, bool v)
        {
            w.WriteBooleanValue(v);
        }

        public static void WriteLong(this ICompacted _, ref Utf8JsonWriter w, long v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteShort(this ICompacted _, ref Utf8JsonWriter w, short v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteByte(this ICompacted _, ref Utf8JsonWriter w, byte v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteByteArray(this ICompacted _, ref Utf8JsonWriter w, byte[] v)
        {
            w.WriteBase64StringValue(v);
        }

        public static void WriteChar(this ICompacted _, ref Utf8JsonWriter w, char v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteCharArray(this ICompacted _, ref Utf8JsonWriter w, char[] v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteHalf(this ICompacted _, ref Utf8JsonWriter w, Half v)
        {
            w.WriteNumberValue((int)v);
        }

        public static void WriteDecimal(this ICompacted _, ref Utf8JsonWriter w, decimal v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteIntEnum<T>(this ICompacted _, ref Utf8JsonWriter w, T v)
        {
            w.WriteNumberValue((int)(object)v);
        }

        public static void WriteExact<T>(this ICompacted _, string type, ref Utf8JsonWriter w, T v)
        {
            ((ICompacted)v).WriteJson(ref w);
        }

        public static void WriteDouble(this ICompacted _, ref Utf8JsonWriter w, double v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteSbyte(this ICompacted _, ref Utf8JsonWriter w, sbyte v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteUshort(this ICompacted _, ref Utf8JsonWriter w, ushort v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteUint(this ICompacted _, ref Utf8JsonWriter w, uint v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteUlong(this ICompacted _, ref Utf8JsonWriter w, ulong v)
        {
            w.WriteNumberValue(v);
        }

        public static void WriteDateTimeOffset(this ICompacted _, ref Utf8JsonWriter w, DateTimeOffset v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteDateTime(this ICompacted _, ref Utf8JsonWriter w, DateTime v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteTimeSpan(this ICompacted _, ref Utf8JsonWriter w, TimeSpan v)
        {
            w.WriteNumberValue(v.Ticks);
        }

        public static void WriteGuid(this ICompacted _, ref Utf8JsonWriter w, Guid v)
        {
            w.WriteStringValue(v);
        }

        public static void WriteNullableBool(this ICompacted c, ref Utf8JsonWriter w, bool? v)
        {
            c.WriteBool(ref w, v.Value);
        }

        public static void WriteNullableInt(this ICompacted c, ref Utf8JsonWriter w, int? v)
        {
            c.WriteInt(ref w, v.Value);
        }

        public static void WriteNullableDouble(this ICompacted c, ref Utf8JsonWriter w, double? v)
        {
            c.WriteDouble(ref w, v.Value);
        }

        public static void WriteNullableDateTime(this ICompacted c, ref Utf8JsonWriter w, DateTime? v)
        {
            c.WriteDateTime(ref w, v.Value);
        }

        public static void WriteShortArray(this ICompacted c, ref Utf8JsonWriter w, short[] v)
        {
            throw new NotImplementedException();
        }

        public static void WriteDict<T>(this ICompacted c, ref Utf8JsonWriter w, IDictionary<string, T> v)
        {
            throw new NotImplementedException();
        }

        public static void WriteOneOf<T>(this ICompacted c, ref Utf8JsonWriter w, T v)
        {
            throw new NotImplementedException();
        }

        public static void WriteList<T>(this ICompacted c, ref Utf8JsonWriter w, List<T> v)
        {
            throw new NotImplementedException();
        }

        public static List<T> ReadList<T>(this ICompacted c, ref Utf8JsonReader r)
        {
            throw new NotImplementedException();
        }

        public static T ReadOneOf<T>(this ICompacted c, ref Utf8JsonReader r)
        {
            throw new NotImplementedException();
        }

        public static IDictionary<string, T> ReadDict<T>(this ICompacted c, ref Utf8JsonReader r)
        {
            throw new NotImplementedException();
        }

        public static short[] ReadShortArray(this ICompacted c, ref Utf8JsonReader r)
        {
            throw new NotImplementedException();
        }
    }
}