using System;
using System.Collections.Generic;
using System.Linq;
using Cscg.Compactor.Lib.Tools;
using R = System.IO.BinaryReader;
using W = System.IO.BinaryWriter;

#if NETFRAMEWORK
using Drexel;
#endif

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib.Binary
{
    public static class CompactBinExt
    {
        #if NETFRAMEWORK
        public static int Read7BitEncodedInt(this R r) => r.ReadInt32();
        public static void Write7BitEncodedInt(this W w, int value) => w.Write(value);
        #endif

        public static bool ReadBool(this IBinCompacted _, ref R r)
        {
            return r.ReadBoolean();
        }

        public static byte ReadByte(this IBinCompacted _, ref R r)
        {
            return r.ReadByte();
        }

        public static byte[] ReadByteArray(this IBinCompacted _, ref R r)
        {
            var count = ReadLength(ref r);
            return count == null ? null : r.ReadBytes(count.Value);
        }

        public static char ReadChar(this IBinCompacted _, ref R r)
        {
            return r.ReadChar();
        }

        public static char[] ReadCharArray(this IBinCompacted _, ref R r)
        {
            var count = ReadLength(ref r);
            return count == null ? null : r.ReadChars(count.Value);
        }

        public static DateTime ReadDateTime(this IBinCompacted _, ref R r)
        {
            var number = r.ReadInt64();
            return DateTime.FromBinary(number);
        }

        public static DateTimeOffset ReadDateTimeOffset(this IBinCompacted _, ref R r)
        {
            var t = r.ReadInt64();
            var o = r.ReadDouble();
            var val = Values.Unite(t, o);
            return val;
        }

        public static decimal ReadDecimal(this IBinCompacted _, ref R r)
        {
            return r.ReadDecimal();
        }

        public static Dictionary<string, T> ReadDict<T>(this IBinCompacted c, string type, ref R r)
        {
            var count = ReadLength(ref r);
            if (count == null)
                return null;
            var d = new Dictionary<string, T>(count.Value);
            for (var i = 0; i < count; i++)
            {
                var key = r.ReadString();
                var val = c.ReadOneOf<T>(type, ref r);
                d[key] = val;
            }
            return d;
        }

        public static double ReadDouble(this IBinCompacted _, ref R r)
        {
            return r.ReadDouble();
        }

        public static T ReadExact<T>(this IBinCompacted _, string type, ref R r)
        {
            if (IsNull(ref r))
            {
                return default;
            }
            var (item, obj) = Reflections.Create<T, IBinCompacted>(type);
            obj.ReadBinary(ref r);
            return item;
        }

        public static float ReadFloat(this IBinCompacted _, ref R r)
        {
            return r.ReadSingle();
        }

        public static Guid ReadGuid(this IBinCompacted _, ref R r)
        {
            return new Guid(r.ReadBytes(16));
        }

        public static Half ReadHalf(this IBinCompacted _, ref R r)
        {
#if NETFRAMEWORK
            return (Half)r.ReadSingle();
#else
            return r.ReadHalf();
#endif
        }

        public static T ReadIntEnum<T>(this IBinCompacted _, ref R r)
        {
            return (T)(object)r.ReadInt32();
        }

        public static int ReadInt(this IBinCompacted _, ref R r)
        {
            return r.ReadInt32();
        }

        public static List<T> ReadList<T>(this IBinCompacted c, string type, ref R r)
        {
            var count = ReadLength(ref r);
            if (count == null)
                return null;
            var d = new List<T>(count.Value);
            for (var i = 0; i < count; i++)
            {
                var item = c.ReadOneOf<T>(type, ref r);
                d.Add(item);
            }
            return d;
        }

        public static long ReadLong(this IBinCompacted _, ref R r)
        {
            return r.ReadInt64();
        }

        public static bool? ReadNullableBool(this IBinCompacted c, ref R r)
        {
            return IsNull(ref r) ? null : c.ReadBool(ref r);
        }

        public static DateTime? ReadNullableDateTime(this IBinCompacted c, ref R r)
        {
            return IsNull(ref r) ? null : c.ReadDateTime(ref r);
        }

        public static double? ReadNullableDouble(this IBinCompacted c, ref R r)
        {
            return IsNull(ref r) ? null : c.ReadDouble(ref r);
        }

        public static int? ReadNullableInt(this IBinCompacted c, ref R r)
        {
            return IsNull(ref r) ? null : c.ReadInt(ref r);
        }

        public static T ReadOneOf<T>(this IBinCompacted _, string __, ref R r)
        {
            if (IsNull(ref r))
            {
                return default;
            }
            var fqn = r.ReadString();
            var (item, obj) = Reflections.Create<T, IBinCompacted>(fqn);
            obj.ReadBinary(ref r);
            return item;
        }

        public static sbyte ReadSbyte(this IBinCompacted _, ref R r)
        {
            return r.ReadSByte();
        }

        public static short ReadShort(this IBinCompacted _, ref R r)
        {
            return r.ReadInt16();
        }

        public static short[] ReadShortArray(this IBinCompacted c, ref R r)
        {
            return c.ReadArray(ref r, c.ReadShort);
        }

        public static string[] ReadStringArray(this IBinCompacted c, ref R r)
        {
            return c.ReadArray(ref r, c.ReadString);
        }

        private delegate T Reader<out T>(ref R r);

        private static T[] ReadArray<T>(this IBinCompacted _, ref R r, Reader<T> reader)
        {
            var count = ReadLength(ref r);
            if (count == null)
                return null;
            var v = new T[count.Value];
            for (var i = 0; i < count; i++)
                v[i] = reader(ref r);
            return v;
        }

        private static int? ReadLength(ref R r)
        {
            var count = r.Read7BitEncodedInt();
            return count == -1 ? null : count;
        }

        public static string ReadString(this IBinCompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadString();
        }

        public static TimeSpan ReadTimeSpan(this IBinCompacted _, ref R r)
        {
            return TimeSpan.FromTicks(r.ReadInt64());
        }

        public static uint ReadUint(this IBinCompacted _, ref R r)
        {
            return r.ReadUInt32();
        }

        public static ulong ReadUlong(this IBinCompacted _, ref R r)
        {
            return r.ReadUInt64();
        }

        public static ushort ReadUshort(this IBinCompacted _, ref R r)
        {
            return r.ReadUInt16();
        }

        public static void WriteBool(this IBinCompacted _, ref W w, bool v)
        {
            w.Write(v);
        }

        public static void WriteByte(this IBinCompacted _, ref W w, byte v)
        {
            w.Write(v);
        }

        public static void WriteByteArray(this IBinCompacted _, ref W w, byte[] v)
        {
            if (v == null)
            {
                w.Write7BitEncodedInt(-1);
                return;
            }
            w.Write7BitEncodedInt(v.Length);
            w.Write(v);
        }

        public static void WriteChar(this IBinCompacted _, ref W w, char v)
        {
            w.Write(v);
        }

        public static void WriteCharArray(this IBinCompacted _, ref W w, char[] v)
        {
            if (v == null)
            {
                w.Write7BitEncodedInt(-1);
                return;
            }
            w.Write7BitEncodedInt(v.Length);
            w.Write(v);
        }

        public static void WriteDateTime(this IBinCompacted _, ref W w, DateTime v)
        {
            w.Write(v.ToBinary());
        }

        public static void WriteDateTimeOffset(this IBinCompacted _, ref W w, DateTimeOffset v)
        {
            var (t, o) = v.Split();
            w.Write(t);
            w.Write(o);
        }

        public static void WriteDecimal(this IBinCompacted _, ref W w, decimal v)
        {
            w.Write(v);
        }

        public static void WriteDict<T>(this IBinCompacted c, string type, ref W w, IEnumerable<KeyValuePair<string, T>> v)
        {
            if (v == null)
            {
                w.Write7BitEncodedInt(-1);
                return;
            }
            var copy = v.ToArray();
            w.Write7BitEncodedInt(copy.Length);
            foreach (var item in copy)
            {
                w.Write(item.Key);
                c.WriteOneOf(type, ref w, item.Value);
            }
        }

        public static void WriteDouble(this IBinCompacted _, ref W w, double v)
        {
            w.Write(v);
        }

        public static void WriteExact<T>(this IBinCompacted _, string __, ref W w, T v)
        {
            if (v == null || v is not IBinCompacted bc)
            {
                WriteNull(ref w, true);
                return;
            }
            WriteNull(ref w, false);
            bc.WriteBinary(ref w);
        }

        public static void WriteFloat(this IBinCompacted _, ref W w, float v)
        {
            w.Write(v);
        }

        public static void WriteGuid(this IBinCompacted _, ref W w, Guid v)
        {
            w.Write(v.ToByteArray());
        }

        public static void WriteHalf(this IBinCompacted _, ref W w, Half v)
        {
#if NETFRAMEWORK
            w.Write((float)v);
#else
            w.Write(v);
#endif
        }

        public static void WriteInt(this IBinCompacted _, ref W w, int v)
        {
            w.Write(v);
        }

        public static void WriteIntEnum<T>(this IBinCompacted _, ref W w, T v)
        {
            w.Write((int)(object)v);
        }

        public static void WriteList<T>(this IBinCompacted c, string type, ref W w, IEnumerable<T> v)
        {
            c.WriteArray(type, ref w, v?.ToArray());
        }

        private static void WriteArray<T>(this IBinCompacted c, string type, ref W w, IReadOnlyCollection<T> v)
        {
            if (v == null)
            {
                w.Write7BitEncodedInt(-1);
                return;
            }
            w.Write7BitEncodedInt(v.Count);
            foreach (var item in v)
                c.WriteOneOf(type, ref w, item);
        }

        public static void WriteLong(this IBinCompacted _, ref W w, long v)
        {
            w.Write(v);
        }

        public static void WriteNullableBool(this IBinCompacted c, ref W w, bool? v)
        {
            if (v == null)
            {
                WriteNull(ref w, true);
                return;
            }
            WriteNull(ref w, false);
            c.WriteBool(ref w, v.Value);
        }

        public static void WriteNullableDateTime(this IBinCompacted c, ref W w, DateTime? v)
        {
            if (v == null)
            {
                WriteNull(ref w, true);
                return;
            }
            WriteNull(ref w, false);
            c.WriteDateTime(ref w, v.Value);
        }

        public static void WriteNullableDouble(this IBinCompacted c, ref W w, double? v)
        {
            if (v == null)
            {
                WriteNull(ref w, true);
                return;
            }
            WriteNull(ref w, false);
            c.WriteDouble(ref w, v.Value);
        }

        public static void WriteNullableInt(this IBinCompacted c, ref W w, int? v)
        {
            if (v == null)
            {
                WriteNull(ref w, true);
                return;
            }
            WriteNull(ref w, false);
            c.WriteInt(ref w, v.Value);
        }

        public static void WriteOneOf<T>(this IBinCompacted c, string _, ref W w, T v)
        {
            if (v == null || v is not IBinCompacted bc)
            {
                WriteNull(ref w, true);
                return;
            }
            WriteNull(ref w, false);
            var fqn = v.GetType().FullName!;
            w.Write(fqn);
            bc.WriteBinary(ref w);
        }

        public static void WriteProperty(this IBinCompacted _, ref W w, string name)
        {
            w.Write7BitEncodedInt('c');
            w.Write(name);
        }

        public static void WriteSbyte(this IBinCompacted _, ref W w, sbyte v)
        {
            w.Write(v);
        }

        public static void WriteShort(this IBinCompacted _, ref W w, short v)
        {
            w.Write(v);
        }

        private delegate void Writer<in T>(ref W w, T v);

        private static void WriteArray<T>(this IBinCompacted _, ref W w, IReadOnlyCollection<T> v, Writer<T> writer)
        {
            if (v == null)
            {
                w.Write7BitEncodedInt(-1);
                return;
            }
            w.Write7BitEncodedInt(v.Count);
            foreach (var item in v)
                writer(ref w, item);
        }

        public static void WriteShortArray(this IBinCompacted c, ref W w, short[] v)
        {
            c.WriteArray(ref w, v, c.WriteShort);
        }

        public static void WriteStringArray(this IBinCompacted c, ref W w, string[] v)
        {
            c.WriteArray(ref w, v, c.WriteString);
        }

        public static void WriteString(this IBinCompacted _, ref W w, string v)
        {
            if (v == null)
            {
                WriteNull(ref w, true);
                return;
            }
            WriteNull(ref w, false);
            w.Write(v);
        }

        private static void WriteNull(ref W w, bool e)
        {
            w.Write7BitEncodedInt(e ? 0 : 1);
        }

        private static bool IsNull(ref R r)
        {
            return r.Read7BitEncodedInt() == 0;
        }

        public static void WriteTimeSpan(this IBinCompacted _, ref W w, TimeSpan v)
        {
            w.Write(v.Ticks);
        }

        public static void WriteUint(this IBinCompacted _, ref W w, uint v)
        {
            w.Write(v);
        }

        public static void WriteUlong(this IBinCompacted _, ref W w, ulong v)
        {
            w.Write(v);
        }

        public static void WriteUshort(this IBinCompacted _, ref W w, ushort v)
        {
            w.Write(v);
        }
    }
}