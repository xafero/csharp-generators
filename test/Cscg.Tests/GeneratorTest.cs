using Cscg.Constants;
using Cscg.Tests.Tools;
using SqlPreparer;
using Xunit;
using static Cscg.Tests.TestHelper;

namespace Cscg.Tests
{
    public class GeneratorTest
    {
        [Fact]
        public void TestBinary()
        {
            var gen = new BinaryGenerator();
            var (_, source) = GetLocalFile("DisplayValues.cs");

            var input = source.CreateCompilation();
            var (output, run) = input.RunGenerators(out var dia, [gen], []);

            dia.CheckNoError(output, 4);
            run.CheckNoError(3);
        }

        [Fact]
        public void TestConst()
        {
            var gen = new ConstGenerator();
            var (_, source) = GetLocalFile("Program.cg.cs");
            AddedMemoryText at = GetLocalFile("res/sample.txt");

            var input = source.CreateCompilation();
            var (output, run) = input.RunGenerators(out var dia, [gen], [at]);

            dia.CheckNoError(output, 4);
            run.CheckNoError(3);
        }
    }
}