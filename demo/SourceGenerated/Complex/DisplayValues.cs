using System;
using autogen;

namespace SourceGenerated.Complex
{
    [ConciseObj]
    public sealed partial class Person
    {
        public string FirstName { get; set; }
        public string SecondName { get; set; }
    }

    public enum WeekDays
    {
        Monday, Tuesday, Wednesday, Thursday, Friday, Saturday, Sunday
    }

    [ConciseObj]
    public sealed partial class DisplayValues
    {
        public float AspectRatio { get; set; }
        public int AutoSaveTime { get; set; }
        public WeekDays WorkDay { get; set; }
        public long DiskSize { get; set; }
        public Half HalfSize { get; set; }
        public double Police { get; set; }
        public decimal Money { get; set; }
        public Person Owner { get; set; }
        public uint VeryMid { get; set; }
        public ulong VeryLong { get; set; }
        public bool ShowStatusBar { get; set; }
        public string TempDirectory { get; set; }
        public DateTimeOffset TimeStamp { get; set; }
        public byte[] Image { get; set; }
    }
}