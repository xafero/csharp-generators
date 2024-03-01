using System;
using System.Collections.Generic;
using System.Text.Json;
using R = System.Xml.XmlReader;
using W = System.Xml.XmlWriter;

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib
{
    public static class CompactBinExt
    {
        public static bool ReadBool(this ICompacted _, ref R r)
        {
            return default;
        }

        public static byte ReadByte(this ICompacted _, ref R r)
        {
            return default;
        }

        public static byte[] ReadByteArray(this ICompacted _, ref R r)
        {
            return default;
        }

        public static char ReadChar(this ICompacted _, ref R r)
        {
            return default;
        }

        public static char[] ReadCharArray(this ICompacted _, ref R r)
        {
            return default;
        }

        public static DateTime ReadDateTime(this ICompacted _, ref R r)
        {
            return default;
        }

        public static DateTimeOffset ReadDateTimeOffset(this ICompacted _, ref R r)
        {
            return default;
        }

        public static decimal ReadDecimal(this ICompacted _, ref R r)
        {
            return default;
        }

        public static IDictionary<string, T> ReadDict<T>(this ICompacted _, ref R r)
        {
            return default;
        }

        public static double ReadDouble(this ICompacted _, ref R r)
        {
            return default;
        }

        public static T ReadExact<T>(this ICompacted _, string type, ref R r) where T : ICompacted
        {
            return default;
        }

        public static float ReadFloat(this ICompacted _, ref R r)
        {
            return default;
        }

        public static Guid ReadGuid(this ICompacted _, ref R r)
        {
            return default;
        }

        public static Half ReadHalf(this ICompacted _, ref R r)
        {
            return default;
        }

        public static T ReadIntEnum<T>(this ICompacted _, ref R r)
        {
            return default;
        }

        public static int ReadInt(this ICompacted _, ref R r)
        {
            return default;
        }

        public static List<T> ReadList<T>(this ICompacted _, ref R r)
        {
            return default;
        }

        public static long ReadLong(this ICompacted _, ref R r)
        {
            return default;
        }

        public static bool? ReadNullableBool(this ICompacted _, ref R r)
        {
            return default;
        }

        public static DateTime? ReadNullableDateTime(this ICompacted _, ref R r)
        {
            return default;
        }

        public static double? ReadNullableDouble(this ICompacted _, ref R r)
        {
            return default;
        }

        public static int? ReadNullableInt(this ICompacted _, ref R r)
        {
            return default;
        }

        public static T ReadOneOf<T>(this ICompacted _, ref R r)
        {
            return default;
        }

        public static sbyte ReadSbyte(this ICompacted _, ref R r)
        {
            return default;
        }

        public static short ReadShort(this ICompacted _, ref R r)
        {
            return default;
        }

        public static short[] ReadShortArray(this ICompacted _, ref R r)
        {
            return default;
        }

        public static string ReadString(this ICompacted _, ref R r)
        {
            return default;
        }

        public static TimeSpan ReadTimeSpan(this ICompacted _, ref R r)
        {
            return default;
        }

        public static uint ReadUint(this ICompacted _, ref R r)
        {
            return default;
        }

        public static ulong ReadUlong(this ICompacted _, ref R r)
        {
            return default;
        }

        public static ushort ReadUshort(this ICompacted _, ref R r)
        {
            return default;
        }

        public static void WriteBool(this ICompacted _, ref W w, bool v)
        {
            return default;
        }

        public static void WriteByte(this ICompacted _, ref W w, byte v)
        {
            return default;
        }

        public static void WriteByteArray(this ICompacted _, ref W w, byte[] v)
        {
            return default;
        }

        public static void WriteChar(this ICompacted _, ref W w, char v)
        {
            return default;
        }

        public static void WriteCharArray(this ICompacted _, ref W w, char[] v)
        {
            return default;
        }

        public static void WriteDateTime(this ICompacted _, ref W w, DateTime v)
        {
            return default;
        }

        public static void WriteDateTimeOffset(this ICompacted _, ref W w, DateTimeOffset v)
        {
            return default;
        }

        public static void WriteDecimal(this ICompacted _, ref W w, decimal v)
        {
            return default;
        }

        public static void WriteDict<T>(this ICompacted _, ref W w, IEnumerable<KeyValuePair<string, T>> v)
        {
            return default;
        }

        public static void WriteDouble(this ICompacted _, ref W w, double v)
        {
            return default;
        }

        public static void WriteExact<T>(this ICompacted _, string __, ref W w, T v)
        {
            return default;
        }

        public static void WriteFloat(this ICompacted _, ref W w, float v)
        {
            return default;
        }

        public static void WriteGuid(this ICompacted _, ref W w, Guid v)
        {
            return default;
        }

        public static void WriteHalf(this ICompacted _, ref W w, Half v)
        {
            return default;
        }

        public static void WriteInt(this ICompacted _, ref W w, int v)
        {
            return default;
        }

        public static void WriteIntEnum<T>(this ICompacted _, ref W w, T v)
        {
            return default;
        }

        public static void WriteList<T>(this ICompacted _, ref W w, IEnumerable<T> v)
        {
            return default;
        }

        public static void WriteLong(this ICompacted _, ref W w, long v)
        {
            return default;
        }

        public static void WriteNullableBool(this ICompacted _, ref W w, bool? v)
        {
            return default;
        }

        public static void WriteNullableDateTime(this ICompacted _, ref W w, DateTime? v)
        {
            return default;
        }

        public static void WriteNullableDouble(this ICompacted _, ref W w, double? v)
        {
            return default;
        }

        public static void WriteNullableInt(this ICompacted _, ref W w, int? v)
        {
            return default;
        }

        public static void WriteOneOf<T>(this ICompacted _, ref W w, T v)
        {
            return default;
        }

        public static void WriteProperty(this ICompacted _, ref W w, string name)
        {
            return default;
        }

        public static void WriteSbyte(this ICompacted _, ref W w, sbyte v)
        {
            return default;
        }

        public static void WriteShort(this ICompacted _, ref W w, short v)
        {
            return default;
        }

        public static void WriteShortArray(this ICompacted _, ref W w, short[] v)
        {
            return default;
        }

        public static void WriteString(this ICompacted _, ref W w, string v)
        {
            return default;
        }

        public static void WriteTimeSpan(this ICompacted _, ref W w, TimeSpan v)
        {
            return default;
        }

        public static void WriteUint(this ICompacted _, ref W w, uint v)
        {
            return default;
        }

        public static void WriteUlong(this ICompacted _, ref W w, ulong v)
        {
            return default;
        }

        public static void WriteUshort(this ICompacted _, ref W w, ushort v)
        {
            return default;
        }
    }
}