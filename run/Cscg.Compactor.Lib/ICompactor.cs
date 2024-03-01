using System;
using System.Collections.Generic;

namespace Cscg.Compactor.Lib
{
    public interface ICompactor
    {
        float ReadFloat();
        string ReadString();
        int ReadInt();
        bool ReadBool();
        long ReadLong();
        short ReadShort();
        byte ReadByte();
        byte[] ReadByteArray();
        char ReadChar();
        char[] ReadCharArray();
        Half ReadHalf();
        decimal ReadDecimal();
        double ReadDouble();
        sbyte ReadSbyte();
        ushort ReadUshort();
        uint ReadUint();
        ulong ReadUlong();
        DateTimeOffset ReadDateTimeOffset();
        DateTime ReadDateTime();
        TimeSpan ReadTimeSpan();
        Guid ReadGuid();
        bool? ReadNullableBool();
        int? ReadNullableInt();
        DateTime? ReadNullableDateTime();
        void WriteFloat(float value);
        void WriteString(string value);
        void WriteInt(int value);
        void WriteBool(bool value);
        void WriteLong(long value);
        void WriteShort(short value);
        void WriteByte(byte value);
        void WriteByteArray(byte[] value);
        void WriteChar(char value);
        void WriteCharArray(char[] value);
        void WriteHalf(Half value);
        void WriteDecimal(decimal value);
        void WriteDouble(double value);
        void WriteSbyte(sbyte value);
        void WriteUshort(ushort value);
        void WriteUint(uint value);
        void WriteUlong(ulong value);
        void WriteDateTimeOffset(DateTimeOffset value);
        void WriteDateTime(DateTime value);
        void WriteTimeSpan(TimeSpan value);
        void WriteGuid(Guid value);
        void WriteNullableBool(bool? value);
        void WriteNullableInt(int? value);
        void WriteNullableDateTime(DateTime? value);
        T ReadIntEnum<T>() where T : Enum;
        T ReadExact<T>();
        void WriteIntEnum<T>(T value) where T : Enum;
        void WriteExact<T>(T value);
        short[] ReadShortArray();
        void WriteShortArray(short[] values);
        void WriteList<T>(IEnumerable<T> values);
        void WriteOneOf<T>(T values);
        void WriteDict<T>(IEnumerable<KeyValuePair<string, T>> values);
        List<T> ReadList<T>();
        T ReadOneOf<T>();
        Dictionary<string, T> ReadDict<T>();
        double? ReadNullableDouble();
        void WriteNullableDouble(double? value);
        void WriteStartObject();
        void WriteEndObject();
        void WriteProperty(string name);
        void WriteStartArray();
        void WriteEndArray();
    }
}