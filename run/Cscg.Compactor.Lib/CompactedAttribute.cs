using System;

namespace Cscg.Compactor.Lib
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CompactedAttribute : Attribute
    {
        public CompactedAttribute()
        {
        }

        public CompactedAttribute(DataFormat format)
        {
            Format = format;
        }

        public DataFormat Format { get; set; }
    }
}