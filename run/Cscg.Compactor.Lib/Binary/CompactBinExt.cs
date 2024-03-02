﻿using System;
using System.Collections.Generic;
using System.Linq;
using R = System.IO.BinaryReader;
using W = System.IO.BinaryWriter;

// ReSharper disable UnusedMember.Global

namespace Cscg.Compactor.Lib
{
    public static class CompactBinExt
    {
        public static bool ReadBool(this ICompacted _, ref R r)
        {
            return r.ReadBoolean();
        }

        public static byte ReadByte(this ICompacted _, ref R r)
        {
            return r.ReadByte();
        }

        public static byte[] ReadByteArray(this ICompacted _, ref R r)
        {
            var count = ReadLength(ref r);
            return count == null ? null : r.ReadBytes(count.Value);
        }

        public static char ReadChar(this ICompacted _, ref R r)
        {
            return r.ReadChar();
        }

        public static char[] ReadCharArray(this ICompacted _, ref R r)
        {
            var count = ReadLength(ref r);
            return count == null ? null : r.ReadChars(count.Value);
        }

        public static DateTime ReadDateTime(this ICompacted _, ref R r)
        {
            return DateTime.FromBinary(r.ReadInt64());
        }

        public static DateTimeOffset ReadDateTimeOffset(this ICompacted _, ref R r)
        {
            var t = r.ReadInt64();
            var o = r.ReadDouble();
            var val = Values.Unite(t, o);
            return val;
        }

        public static decimal ReadDecimal(this ICompacted _, ref R r)
        {
            return r.ReadDecimal();
        }

        public static Dictionary<string, T> ReadDict<T>(this ICompacted c, string type, ref R r)
        {
            var count = ReadLength(ref r);
            if (count == null)
                return null;
            var d = new Dictionary<string, T>();
            for (var i = 0; i < count; i++)
            {
                var key = c.ReadString(ref r);
                var val = Activator.CreateInstance<T>();
                ((ICompacted)val).ReadBinary(ref r);
                d[key] = val;
            }
            return d;
        }

        public static double ReadDouble(this ICompacted _, ref R r)
        {
            return r.ReadDouble();
        }

        public static T ReadExact<T>(this ICompacted _, string type, ref R r) where T : ICompacted
        {
            if (IsNull(ref r)) return default;
            var item = Activator.CreateInstance<T>();
            item.ReadBinary(ref r);
            return item;
        }

        public static float ReadFloat(this ICompacted _, ref R r)
        {
            return r.ReadSingle();
        }

        public static Guid ReadGuid(this ICompacted _, ref R r)
        {
            return new Guid(r.ReadBytes(16));
        }

        public static Half ReadHalf(this ICompacted _, ref R r)
        {
            return r.ReadHalf();
        }

        public static T ReadIntEnum<T>(this ICompacted _, ref R r)
        {
            return (T)(object)r.ReadInt32();
        }

        public static int ReadInt(this ICompacted _, ref R r)
        {
            return r.ReadInt32();
        }

        public static List<T> ReadList<T>(this ICompacted _, string type, ref R r)
        {
            var count = ReadLength(ref r);
            if (count == null)
                return null;
            var d = new List<T>();
            for (var i = 0; i < count; i++)
            {
                // var item = Activator.CreateInstance<T>();
                // ((ICompacted)item).ReadBinary(ref r);
                // TODO d.Add(item);
            }
            return d;
        }

        public static long ReadLong(this ICompacted _, ref R r)
        {
            return r.ReadInt64();
        }

        public static bool? ReadNullableBool(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadBoolean();
        }

        public static DateTime? ReadNullableDateTime(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : ReadDateTime(_, ref r);
        }

        public static double? ReadNullableDouble(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : ReadDouble(_, ref r);
        }

        public static int? ReadNullableInt(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : ReadInt(_, ref r);
        }

        public static T ReadOneOf<T>(this ICompacted _, string type, ref R r)
        {
            if (IsNull(ref r)) return default;
            var item = Activator.CreateInstance<T>();
            ((ICompacted)item).ReadBinary(ref r);
            return item;
        }

        public static sbyte ReadSbyte(this ICompacted _, ref R r)
        {
            return r.ReadSByte();
        }

        public static short ReadShort(this ICompacted _, ref R r)
        {
            return r.ReadInt16();
        }

        public static short[] ReadShortArray(this ICompacted c, ref R r)
        {
            var count = ReadLength(ref r);
            if (count == null)
                return null;
            var v = new short[count.Value];
            for (var i = 0; i < count; i++)
                v[i] = c.ReadShort(ref r);
            return v;
        }

        private static int? ReadLength(ref R r)
        {
            var count = r.Read7BitEncodedInt();
            return count == -1 ? null : count;
        }

        public static string ReadString(this ICompacted _, ref R r)
        {
            return IsNull(ref r) ? null : r.ReadString();
        }

        public static TimeSpan ReadTimeSpan(this ICompacted _, ref R r)
        {
            return TimeSpan.FromTicks(r.ReadInt64());
        }

        public static uint ReadUint(this ICompacted _, ref R r)
        {
            return r.ReadUInt32();
        }

        public static ulong ReadUlong(this ICompacted _, ref R r)
        {
            return r.ReadUInt64();
        }

        public static ushort ReadUshort(this ICompacted _, ref R r)
        {
            return r.ReadUInt16();
        }

        public static void WriteBool(this ICompacted _, ref W w, bool v)
        {
            w.Write(v);
        }

        public static void WriteByte(this ICompacted _, ref W w, byte v)
        {
            w.Write(v);
        }

        public static void WriteByteArray(this ICompacted _, ref W w, byte[] v)
        {
            if (v == null) { w.Write7BitEncodedInt(-1); return; } w.Write7BitEncodedInt(v.Length);
            w.Write(v);
        }

        public static void WriteChar(this ICompacted _, ref W w, char v)
        {
            w.Write(v);
        }

        public static void WriteCharArray(this ICompacted _, ref W w, char[] v)
        {
            if (v == null) { w.Write7BitEncodedInt(-1); return; } w.Write7BitEncodedInt(v.Length);
            w.Write(v);
        }
        
        public static void WriteDateTime(this ICompacted _, ref W w, DateTime v)
        {
            w.Write(v.ToBinary());
        }

        public static void WriteDateTimeOffset(this ICompacted _, ref W w, DateTimeOffset v)
        {
            var (t, o) = v.Split();
            w.Write(t);
            w.Write(o);
        }

        public static void WriteDecimal(this ICompacted _, ref W w, decimal v)
        {
            w.Write(v);
        }

        public static void WriteDict<T>(this ICompacted _, string type, ref W w, IEnumerable<KeyValuePair<string, T>> v)
        {
            if (v == null) { w.Write7BitEncodedInt(-1); return; }
            var copy = v.ToArray(); w.Write7BitEncodedInt(copy.Length);
            foreach (var item in copy)
            {
                w.Write(item.Key); 
                ((ICompacted)item.Value).WriteBinary(ref w);
            }
        }

        public static void WriteDouble(this ICompacted _, ref W w, double v)
        {
            w.Write(v);
        }

        public static void WriteExact<T>(this ICompacted _, string __, ref W w, T v)
        {
            if (v == null) { WriteNull(ref w, true); return; } WriteNull(ref w, false);
            ((ICompacted)v).WriteBinary(ref w);
        }

        public static void WriteFloat(this ICompacted _, ref W w, float v)
        {
            w.Write(v);
        }

        public static void WriteGuid(this ICompacted _, ref W w, Guid v)
        {
            w.Write(v.ToByteArray());
        }

        public static void WriteHalf(this ICompacted _, ref W w, Half v)
        {
            w.Write(v);
        }

        public static void WriteInt(this ICompacted _, ref W w, int v)
        {
            w.Write(v);
        }

        public static void WriteIntEnum<T>(this ICompacted _, ref W w, T v)
        {
            w.Write((int)(object)v);
        }

        public static void WriteList<T>(this ICompacted _, string type, ref W w, IEnumerable<T> v)
        {
            if (v == null) { w.Write7BitEncodedInt(-1); return; }
            var copy = v.ToArray(); w.Write7BitEncodedInt(copy.Length);
            foreach (var item in copy) ((ICompacted)item).WriteBinary(ref w);
        }

        public static void WriteLong(this ICompacted _, ref W w, long v)
        {
            w.Write(v);
        }

        public static void WriteNullableBool(this ICompacted _, ref W w, bool? v)
        {
            if (v == null) { WriteNull(ref w, true); return; } WriteNull(ref w, false);
            WriteBool(_, ref w, v.Value);
        }

        public static void WriteNullableDateTime(this ICompacted _, ref W w, DateTime? v)
        {
            if (v == null) { WriteNull(ref w, true); return; } WriteNull(ref w, false);
            WriteDateTime(_, ref w, v.Value);
        }

        public static void WriteNullableDouble(this ICompacted _, ref W w, double? v)
        {
            if (v == null) { WriteNull(ref w, true); return; } WriteNull(ref w, false);
            WriteDouble(_, ref w, v.Value);
        }

        public static void WriteNullableInt(this ICompacted _, ref W w, int? v)
        {
            if (v == null) { WriteNull(ref w, true); return; } WriteNull(ref w, false);
            WriteInt(_, ref w, v.Value);
        }

        public static void WriteOneOf<T>(this ICompacted _, string type, ref W w, T v)
        {
            if (v == null) { WriteNull(ref w, true); return; } WriteNull(ref w, false);
            ((ICompacted)v).WriteBinary(ref w);
        }

        public static void WriteProperty(this ICompacted _, ref W w, string name)
        {
            w.Write7BitEncodedInt('c');
            w.Write(name);
        }

        public static void WriteSbyte(this ICompacted _, ref W w, sbyte v)
        {
            w.Write(v);
        }

        public static void WriteShort(this ICompacted _, ref W w, short v)
        {
            w.Write(v);
        }

        public static void WriteShortArray(this ICompacted c, ref W w, short[] v)
        {
            if (v == null) { w.Write7BitEncodedInt(-1); return; } w.Write7BitEncodedInt(v.Length);
            foreach (var item in v) c.WriteShort(ref w, item);
        }

        public static void WriteString(this ICompacted _, ref W w, string v)
        {
            if (v == null) { WriteNull(ref w, true); return; } WriteNull(ref w, false);
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

        public static void WriteTimeSpan(this ICompacted _, ref W w, TimeSpan v)
        {
            w.Write(v.Ticks);
        }

        public static void WriteUint(this ICompacted _, ref W w, uint v)
        {
            w.Write(v);
        }

        public static void WriteUlong(this ICompacted _, ref W w, ulong v)
        {
            w.Write(v);
        }

        public static void WriteUshort(this ICompacted _, ref W w, ushort v)
        {
            w.Write(v);
        }
    }
}