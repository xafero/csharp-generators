using System;

namespace Cscg.AdoNet.Lib
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class ForeignAttribute : Attribute
    {
        public string? Table { get; set; }
        public string? Column { get; set; }
        public bool? Unique { get; set; }
    }
}