using System;
using System.Collections.Generic;
using autogen;
using System.IO;
using SourceGenerated;
using SourceGenerated.Complex;
using Xunit;
using Xunit.Abstractions;
using DvS = SourceGenerated.Simple.DisplayValues;
using DvC = SourceGenerated.Complex.DisplayValues;
using static Cscg.Tests.Tools.DebugTool;

namespace Cscg.Tests
{
    public class RuntimeTest
    {
        private readonly ITestOutputHelper _out;

        public RuntimeTest(ITestOutputHelper @out)
        {
            _out = @out;
        }

        [Fact]
        public void TestMain()
        {
            Program.Main();
        }

        [Fact]
        public void TestConst()
        {
            var input = ToDict(typeof(ConstStrings));
            var output = new SortedDictionary<string, object>
            {
                { "Flag1", true }, { "Greeting1", "Hello World" }, { "Count1", 3 },
                { "Super1", 42.934 }, { "Count2", new[] { 3, 5 } },
                { "Flag2", new[] { true, false, true } },
                { "Greeting2", new[] { "Hello World", "Not good" } },
                { "Super2", new[] { 42.934, 33.2, 17.4 } },
                { "Unix1", new DateTime(1970, 02, 03) },
                {
                    "Unix2", new[]
                    {
                        new DateTime(1970, 02, 03), new DateTime(1970, 02, 04)
                    }
                },
                { "Image1", Convert.FromBase64String("R0lGODlhAQABAAAAACw=") },
                {
                    "Image2", new[]
                    {
                        Convert.FromBase64String("R0lGODlhAQABAAAAACw="),
                        Convert.FromBase64String(
                            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=")
                    }
                }
            };

            var expected = ToJson(input);
            var actual = ToJson(output);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("e")]
        [InlineData("s")]
        public void TestBinary(string mode)
        {
            var input = mode == "e" ? new DvS() : Program.CreateSample();
            byte[] bytes;
            using (var mem = new MemoryStream())
            {
                input.WriteBinary(mem);
                bytes = mem.ToArray();
            }
            var output = new DvS();
            using (var mem = new MemoryStream(bytes))
                output.ReadBinary(mem);

            var expected = ToJson(input);
            var actual = ToJson(output);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("e")]
        [InlineData("s")]
        public void TestConcise(string mode)
        {
            var input = mode == "e" ? new DvC() : CreateCdv();
            byte[] bytes;
            using (var mem = new MemoryStream())
            {
                input.WriteCBOR(mem);
                bytes = mem.ToArray();
            }
            var output = new DvC();
            using (var mem = new MemoryStream(bytes))
                output.ReadCBOR(mem);

            var expected = ToJson(input);
            var actual = ToJson(output);
            _out.WriteLine(ToCborJson(bytes));
            Assert.Equal(expected, actual);
        }

        private static DvC CreateCdv()
            => new()
            {
                TempDirectory = "help me",
                DiskSize = 29392,
                AspectRatio = 29.345f,
                AutoSaveTime = 3020,
                Police = 200.2,
                Money = 21111.11m,
                VeryMid = 3902,
                VeryLong = 29293,
                TimeStamp = DateTimeOffset.UtcNow,
                Owner = new Person { FirstName = "Hans", SecondName = "Gruber" },
                ShowStatusBar = true,
                HalfSize = Half.Epsilon,
                WorkDay = WeekDays.Friday,
                Image = [1, 3, 23, 42, 3, 1, 35, 7, 48, 9]
            };
    }
}