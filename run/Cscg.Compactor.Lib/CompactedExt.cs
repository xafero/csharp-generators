using System;
using System.Collections.Generic;
using System.Formats.Cbor;
using System.IO;

namespace Cscg.Compactor.Lib
{
    public static class CompactedExt
    {
        public static byte[] ToBytes(this Stream stream)
        {
            byte[] array;
            if (stream is MemoryStream mem)
            {
                array = mem.ToArray();
            }
            else
            {
                using var copy = new MemoryStream();
                stream.CopyTo(copy);
                array = copy.ToArray();
            }
            return array;
        }

        public static void WriteProperty(this ICompacted _, ref CborWriter w, string v)
            => w.WriteTextString(v);

        public static string ReadString(this ICompacted _, ref CborReader r)
            => r.ReadTextString();

        public static void WriteString(this ICompacted _, ref CborWriter w, string v)
            => w.WriteTextString(v);

        public static void WriteInt(this ICompacted _, ref CborWriter w, int v)
            => w.WriteInt32(v);

        public static void WriteIntEnum<T>(this ICompacted _, ref CborWriter w, T v)
            where T : Enum => w.WriteInt32((int)(object)v);

        public static int ReadInt(this ICompacted _, ref CborReader r)
            => r.ReadInt32();

        public static T ReadIntEnum<T>(this ICompacted _, ref CborReader r)
            => (T)(object)r.ReadInt32();

        public static float ReadFloat(this ICompacted _, ref CborReader r)
            => r.ReadSingle();

        public static bool ReadBool(this ICompacted _, ref CborReader r)
            => r.ReadBoolean();

        public static long ReadLong(this ICompacted _, ref CborReader r)
            => r.ReadInt64();

        public static short ReadShort(this ICompacted _, ref CborReader r)
            => (short)r.ReadInt32();

        public static byte ReadByte(this ICompacted _, ref CborReader r)
            => (byte)r.ReadInt32();

        public static Guid ReadGuid(this ICompacted _, ref CborReader r)
            => new(r.ReadByteString());

        public static TimeSpan ReadTimeSpan(this ICompacted _, ref CborReader r)
            => TimeSpan.FromTicks(r.ReadInt64());

        public static DateTime ReadDateTime(this ICompacted _, ref CborReader r)
            => DateTime.FromBinary(r.ReadInt64());

        public static ulong ReadUlong(this ICompacted _, ref CborReader r)
            => r.ReadUInt64();

        public static DateTimeOffset ReadDateTimeOffset(this ICompacted _, ref CborReader r)
            => r.ReadDateTimeOffset();

        public static uint ReadUint(this ICompacted _, ref CborReader r)
            => r.ReadUInt32();

        public static ushort ReadUshort(this ICompacted _, ref CborReader r)
            => (ushort)r.ReadInt32();

        public static sbyte ReadSbyte(this ICompacted _, ref CborReader r)
            => (sbyte)r.ReadInt32();

        public static decimal ReadDecimal(this ICompacted _, ref CborReader r)
            => r.ReadDecimal();

        public static double ReadDouble(this ICompacted _, ref CborReader r)
            => r.ReadDouble();

        public static T ReadExact<T>(this ICompacted _, ref CborReader r)
        {
            throw new NotImplementedException();
        }

        public static Half ReadHalf(this ICompacted _, ref CborReader r)
            => r.ReadHalf();

        public static char[] ReadCharArray(this ICompacted _, ref CborReader r)
            => r.ReadTextString().ToCharArray();

        public static char ReadChar(this ICompacted _, ref CborReader r)
            => (char)r.ReadInt32();

        public static byte[] ReadByteArray(this ICompacted _, ref CborReader r)
            => r.ReadByteString();

        public static bool? ReadNullableBool(this ICompacted _, ref CborReader r)
            => r.ReadBoolean();

        public static int? ReadNullableInt(this ICompacted _, ref CborReader r)
            => r.ReadInt32();

        public static double? ReadNullableDouble(this ICompacted _, ref CborReader r)
            => r.ReadDouble();

        public static DateTime? ReadNullableDateTime(this ICompacted c, ref CborReader r)
            => c.ReadDateTime(ref r);

        public static void WriteFloat(this ICompacted _, ref CborWriter w, float v)
            => w.WriteSingle(v);

        public static void WriteBool(this ICompacted _, ref CborWriter w, bool v)
            => w.WriteBoolean(v);

        public static void WriteNullableDateTime(this ICompacted _, ref CborWriter w, DateTime? v)
            => w.WriteInt64(v.Value.ToBinary());

        public static void WriteNullableInt(this ICompacted _, ref CborWriter w, int? v)
            => w.WriteInt32(v.Value);

        public static void WriteNullableDouble(this ICompacted _, ref CborWriter w, double? v)
            => w.WriteDouble(v.Value);

        public static void WriteNullableBool(this ICompacted _, ref CborWriter w, bool? v)
            => w.WriteBoolean(v.Value);

        public static void WriteGuid(this ICompacted _, ref CborWriter w, Guid v)
            => w.WriteByteString(v.ToByteArray());

        public static void WriteTimeSpan(this ICompacted _, ref CborWriter w, TimeSpan v)
            => w.WriteInt64(v.Ticks);

        public static void WriteDateTime(this ICompacted _, ref CborWriter w, DateTime v)
            => w.WriteInt64(v.ToBinary());

        public static void WriteDateTimeOffset(this ICompacted _, ref CborWriter w, DateTimeOffset v)
            => w.WriteDateTimeOffset(v);

        public static void WriteUlong(this ICompacted _, ref CborWriter w, ulong v)
            => w.WriteUInt64(v);

        public static void WriteUint(this ICompacted _, ref CborWriter w, uint v)
            => w.WriteUInt32(v);

        public static void WriteUshort(this ICompacted _, ref CborWriter w, ushort v)
            => w.WriteInt32(v);

        public static void WriteSbyte(this ICompacted _, ref CborWriter w, sbyte v)
            => w.WriteInt32(v);

        public static void WriteDouble(this ICompacted _, ref CborWriter w, double v)
            => w.WriteDouble(v);

        public static void WriteExact<T>(this ICompacted _, ref CborWriter w, T v)
        {
            throw new NotImplementedException();
        }

        public static void WriteDecimal(this ICompacted _, ref CborWriter w, decimal v)
            => w.WriteDecimal(v);

        public static void WriteHalf(this ICompacted _, ref CborWriter w, Half v)
            => w.WriteHalf(v);

        public static void WriteCharArray(this ICompacted _, ref CborWriter w, char[] v)
            => w.WriteTextString(v);

        public static void WriteChar(this ICompacted _, ref CborWriter w, char v)
            => w.WriteInt32(v);

        public static void WriteByteArray(this ICompacted _, ref CborWriter w, byte[] v)
            => w.WriteByteString(v);

        public static void WriteByte(this ICompacted _, ref CborWriter w, byte v)
            => w.WriteInt32(v);

        public static void WriteShort(this ICompacted _, ref CborWriter w, short v)
            => w.WriteInt32(v);

        public static void WriteLong(this ICompacted _, ref CborWriter w, long v)
            => w.WriteInt64(v);

        public static T ReadOneOf<T>(this ICompacted _, ref CborReader r)
        {
            throw new NotImplementedException();
        }

        public static void WriteOneOf<T>(this ICompacted _, ref CborWriter w, T v)
        {
            throw new NotImplementedException();
        }

        public static List<T> ReadList<T>(this ICompacted _, ref CborReader r)
        {
            throw new NotImplementedException();
        }

        public static Dictionary<string, T> ReadDict<T>(this ICompacted _, ref CborReader r)
        {
            throw new NotImplementedException();
        }

        public static short[] ReadShortArray(this ICompacted _, ref CborReader r)
        {
            throw new NotImplementedException();
        }

        public static void WriteList<T>(this ICompacted _, ref CborWriter w, IEnumerable<T> v)
        {
            throw new NotImplementedException();
        }

        public static void WriteDict<T>(this ICompacted _, ref CborWriter w, IEnumerable<KeyValuePair<string, T>> v)
        {
            throw new NotImplementedException();
        }

        public static void WriteShortArray(this ICompacted _, ref CborWriter w, short[] v)
        {
            throw new NotImplementedException();
        }
    }
}