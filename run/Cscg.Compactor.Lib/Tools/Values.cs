using System;

namespace Cscg.Compactor.Lib
{
    public static class Values
    {
        public static (long time, double off) Split(this DateTimeOffset value)
        {
            var time = value.DateTime.ToBinary();
            var off = value.Offset.TotalMinutes;
            return (time, off);
        }

        public static DateTimeOffset Unite(long t, double o)
        {
            var time = DateTime.FromBinary(t);
            var off = TimeSpan.FromMinutes(o);
            return new DateTimeOffset(time, off);
        }
    }
}