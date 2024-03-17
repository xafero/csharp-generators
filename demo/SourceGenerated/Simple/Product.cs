using System;
using Cscg.Compactor.Lib;

namespace SourceGenerated.Simple
{
    [Compacted]
    public partial class Product
    {
        public string Name { get; set; }

        public DateTime ExpiryDate { get; set; }

        public decimal Price { get; set; }

        public string[] Sizes { get; set; }
    }
}