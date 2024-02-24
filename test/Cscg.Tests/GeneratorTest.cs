using SqlPreparer;
using Xunit;

namespace Cscg.Tests
{
    public class GeneratorTest
    {
        [Fact]
        public void TestBinary()
        {
            var gen = new BinaryGenerator();
            var (_, source) = TestHelper.GetLocalFile("DisplayValues.cs");

            var input = source.CreateCompilation();
            var (output, run) = input.RunGenerators(out var dia, [gen], []);

            dia.CheckNoError(output, 4);
            run.CheckNoError(3);
        }
    }
}