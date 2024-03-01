using System;
using Cscg.Compactor.Lib;

namespace SourceGenerated.Simple
{
    [Compacted(DataFormat.Cbor)]
    public sealed partial class Person
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
    }

    public enum WeekDays
    {
        Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
    }

    [Compacted(DataFormat.Cbor)]
    public sealed partial class DisplayValues
    {
        public float AspectRatio { get; set; }
        public string TempDirectory { get; set; }
        public int AutoSaveTime { get; set; }
        public bool ShowStatusBar { get; set; }
        public long DiskSize { get; set; }
        public short AutoSaveKill { get; set; }
        public byte OneFlag { get; set; }
        public byte[] Image { get; set; }
        public char Letter { get; set; }
        public char[] Letters { get; set; }
        public Half HalfSize { get; set; }
        public decimal Money { get; set; }
        public WeekDays FreeDay { get; set; }
        public Person Owner { get; set; }
        public double Police { get; set; }
        public sbyte Vulcan { get; set; }
        public ushort VeryShort { get; set; }
        public uint VeryMid { get; set; }
        public ulong VeryLong { get; set; }
        public DateTimeOffset TimeOff { get; set; }
        public DateTime TimeStamp { get; set; }
        public TimeSpan Duration { get; set; }
        public Guid Unique { get; set; }
        public bool? MaybeBool { get; set; }
        public int? MaybeInt { get; set; }
        public DateTime? MaybeDate { get; set; }
    }
}