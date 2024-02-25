using autogen;
using System.IO;
using Cscg.Tests.Tools;
using SourceGenerated;
using Xunit;

namespace Cscg.Tests
{
    public class RuntimeTest
    {
        [Fact]
        public void TestMain()
        {
            Program.Main();
        }

        [Fact]
        public void TestBinary()
        {
            Assert.Equal("Hello World", ConstStrings.Greeting1);
        }

        [Fact]
        public void TestConst()
        {
            var input = Program.CreateSample();
            byte[] bytes;
            using (var mem = new MemoryStream())
            {
                input.Write(mem);
                bytes = mem.ToArray();
            }
            var output = new DisplayValues();
            using (var mem = new MemoryStream(bytes))
                output.Read(mem);

            var expected = DebugTool.ToJson(input);
            var actual = DebugTool.ToJson(output);
            Assert.Equal(expected, actual);
        }
    }
}