using Cscg.Compactor.Lib.Binary;
using SourceGenerated.Simple;
using Xunit;
using Xunit.Abstractions;
using System;
using Cscg.Compactor.Lib.Cbor;
using Cscg.Compactor.Lib.Json;
using Cscg.Compactor.Lib.Xml;
using static Cscg.Tests.Tools.DebugTool;

namespace Cscg.Tests
{
    public class CompactTest
    {
        private readonly ITestOutputHelper _out;

        public CompactTest(ITestOutputHelper @out)
        {
            _out = @out;
        }

        [Theory]
        [InlineData("j", 99)]
        [InlineData("x", 217)]
        [InlineData("b", 120)]
        [InlineData("c", 96)]
        public void TestStringConvert(string mode, int strLen)
        {
            var input = new Product
            {
                Name = "Apple",
                ExpiryDate = new DateTime(2008, 12, 28),
                Price = 3.99M,
                Sizes = ["Small", "Medium", "Large"]
            };

            var output = mode switch
            {
                "j" => JsonCompactor.SerializeObjectS(input),
                "x" => XmlCompactor.SerializeObjectS(input),
                "b" => BinaryCompactor.SerializeObjectS(input),
                "c" => CborCompactor.SerializeObjectS(input),
                _ => throw new ArgumentException(mode)
            };
            Assert.Equal(strLen, output.Length);

            var reread = mode switch
            {
                "j" => JsonCompactor.DeserializeObjectS<Product>(output),
                "x" => XmlCompactor.DeserializeObjectS<Product>(output),
                "b" => BinaryCompactor.DeserializeObjectS<Product>(output),
                "c" => CborCompactor.DeserializeObjectS<Product>(output),
                _ => throw new ArgumentException(mode)
            };
            Assert.NotNull(reread);

            Assert.Equal(input.Name, reread.Name);
            Assert.Equal(input.ExpiryDate, reread.ExpiryDate);
            Assert.Equal(input.Price, reread.Price);
            Assert.Equal(input.Sizes, reread.Sizes);

            var expected = ToJson(input);
            var actual = ToJson(reread);
            _out.WriteLine(actual);
            Assert.Equal(expected, actual);
        }
    }
}