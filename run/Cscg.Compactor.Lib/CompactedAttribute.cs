﻿using System;

namespace Cscg.Compactor.Lib
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CompactedAttribute : Attribute
    {
        public CompactedAttribute(DataFormat format = DataFormat.All)
        {
            Format = format;
        }

        public DataFormat Format { get; set; }
    }
}