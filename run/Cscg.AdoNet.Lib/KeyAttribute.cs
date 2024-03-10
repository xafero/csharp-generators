using System;

namespace Cscg.AdoNet.Lib
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class KeyAttribute : Attribute
    {
    }
}