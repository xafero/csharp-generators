using System.Formats.Cbor;
using Cscg.Binary;
using Cscg.ConciseBinary;
using Cscg.Constants;
using Cscg.Tests.Tools;
using Xunit;
using static Cscg.Tests.Tools.TestHelper;

namespace Cscg.Tests
{
    public class GeneratorTest
    {
        [Fact]
        public void TestBinary()
        {
            var gen = new BinaryGenerator();
            var (_, source) = GetLocalFile("Simple/DisplayValues.cs");

            var input = source.CreateCompilation();
            var (output, run) = input.RunGenerators(out var dia, [gen], []);

            dia.CheckNoError(output, 6);
            run.CheckNoError(5);
        }

        [Fact]
        public void TestConcise()
        {
            var gen = new ConciseGenerator();
            var (_, source) = GetLocalFile("Complex/DisplayValues.cs");

            var input = source.CreateCompilation(addedRefs: [GetMetaRef<CborReader>()]);
            var (output, run) = input.RunGenerators(out var dia, [gen], []);

            dia.CheckNoError(output, 6);
            run.CheckNoError(5);
        }

        [Fact]
        public void TestConciseA()
        {
            var gen = new ConciseGenerator();
            var (_, source) = GetLocalFile("Complex/Animals.cs");

            var input = source.CreateCompilation(addedRefs: [GetMetaRef<CborReader>()]);
            var (output, run) = input.RunGenerators(out var dia, [gen], []);

            dia.CheckNoError(output, 12);
            run.CheckNoError(11);
        }

        [Fact]
        public void TestConst()
        {
            var gen = new ConstGenerator();
            var (_, source) = GetLocalFile("Program.cg.cs");
            AddedMemoryText at = GetLocalFile("res/sample.txt");

            var input = source.CreateCompilation();
            var (output, run) = input.RunGenerators(out var dia, [gen], [at]);

            dia.CheckNoError(output, 2);
            run.CheckNoError(1);
        }
    }
}