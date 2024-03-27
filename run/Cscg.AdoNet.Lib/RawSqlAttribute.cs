using System;

namespace Cscg.AdoNet.Lib
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class RawSqlAttribute : Attribute
    {
        public string[]? Mapping { get; set; }
    }
}