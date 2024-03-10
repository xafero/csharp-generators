using System;

namespace Cscg.AdoNet.Lib
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class MappingAttribute : Attribute
    {
        public string? First { get; set; }
        public string? Second { get; set; }
    }
}