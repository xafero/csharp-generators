using System;

namespace Cscg.AdoNet.Lib
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class TableAttribute : Attribute
    {
        public string? Name { get; set; }
    }
}