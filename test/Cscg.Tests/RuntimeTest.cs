using System;
using System.Collections.Generic;
using System.IO;
using Cscg.AutoGen;
using Cscg.Compactor.Lib;
using SourceGenerated;
using SourceGenerated.Complex;
using SourceGenerated.Simple;
using Xunit;
using Xunit.Abstractions;
using DvS = SourceGenerated.Simple.DisplayValues;
using static Cscg.Tests.Tools.DebugTool;
using static SourceGenerated.Complex.Zoos;

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
        [InlineData("d", "e")]
        [InlineData("d", "s")]
        [InlineData("a", "e")]
        [InlineData("a", "s")]
        public void TestBinary(string cla, string mode)
        {
            ICompacted input = cla == "d"
                ? (mode == "e" ? new DvS() : CreateCdv())
                : (mode == "e" ? new Zoo() : CreateZoo());
            byte[] bytes;
            using (var mem = new MemoryStream())
            {
                input.WriteBinary(mem);
                bytes = mem.ToArray();
            }
            ICompacted output = cla == "d"
                ? new DvS()
                : new Zoo();
            using (var mem = new MemoryStream(bytes))
                output.ReadBinary(mem);

            var expected = ToJson(input);
            var actual = ToJson(output);
            _out.WriteLine(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("d", "e")]
        [InlineData("d", "s")]
        [InlineData("a", "e")]
        [InlineData("a", "s")]
        public void TestConcise(string cla, string mode)
        {
            ICompacted input = cla == "d"
                ? (mode == "e" ? new DvS() : CreateCdv())
                : (mode == "e" ? new Zoo() : CreateZoo());
            byte[] bytes;
            using (var mem = new MemoryStream())
            {
                input.WriteCbor(mem);
                bytes = mem.ToArray();
            }
            ICompacted output = cla == "d"
                ? new DvS()
                : new Zoo();
            using (var mem = new MemoryStream(bytes))
                output.ReadCbor(mem);

            var expected = ToJson(input);
            var actual = ToJson(output);
            _out.WriteLine(ToCborJson(bytes));
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("d", "e")]
        [InlineData("d", "s")]
        [InlineData("a", "e")]
        [InlineData("a", "s")]
        public void TestJson(string cla, string mode)
        {
            ICompacted input = cla == "d"
                ? (mode == "e" ? new DvS() : CreateCdv())
                : (mode == "e" ? new Zoo() : CreateZoo());
            byte[] bytes;
            using (var mem = new MemoryStream())
            {
                input.WriteJson(mem);
                bytes = mem.ToArray();
            }

            File.WriteAllBytes($"hello_{cla}_{mode}.json", bytes);



            ICompacted output = cla == "d"
                ? new DvS()
                : new Zoo();
            using (var mem = new MemoryStream(bytes))
                output.ReadJson(mem);

            var expected = ToJson(input);
            var actual = ToJson(output);
            _out.WriteLine(actual);
            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData("d", "e")]
        [InlineData("d", "s")]
        [InlineData("a", "e")]
        [InlineData("a", "s")]
        public void TestXml(string cla, string mode)
        {
            ICompacted input = cla == "d"
                ? (mode == "e" ? new DvS() : CreateCdv())
                : (mode == "e" ? new Zoo() : CreateZoo());
            byte[] bytes;
            using (var mem = new MemoryStream())
            {
                input.WriteXml(mem);
                bytes = mem.ToArray();
            }

            File.WriteAllBytes($"hello_{cla}_{mode}.xml", bytes);




            ICompacted output = cla == "d"
                ? new DvS()
                : new Zoo();
            using (var mem = new MemoryStream(bytes))
                output.ReadXml(mem);

            var expected = ToJson(input);
            var actual = ToJson(output);
            _out.WriteLine(actual);
            Assert.Equal(expected, actual);
        }

        private static DvS CreateCdv()
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
                TimeOff = DateTimeOffset.UtcNow,
                TimeStamp = DateTime.UtcNow,
                Owner = new Person { FirstName = "Hans", SecondName = "Gruber" },
                ShowStatusBar = true,
                HalfSize = Half.Epsilon,
                FreeDay = WeekDays.Friday,
                Image = [1, 3, 23, 42, 3, 1, 35, 7, 48, 9],
                VeryShort = 225,
                AutoSaveKill = 2299,
                Duration = TimeSpan.FromHours(4.23),
                Letter = 'w',
                Letters = ['d', 'z', 'x'],
                MaybeBool = true,
                MaybeDate = DateTime.UnixEpoch.AddYears(28),
                MaybeInt = -902,
                OneFlag = 39,
                Unique = Guid.NewGuid(),
                Vulcan = -36
            };
    }
}