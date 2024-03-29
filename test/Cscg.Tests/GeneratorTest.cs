using System.Data.Common;
using System.Formats.Cbor;
using Cscg.AdoNet;
using Cscg.Compactor;
using Cscg.Constants;
using Cscg.StubCreator;
using Cscg.Tests.Tools;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json.Linq;
using Xunit;
using static Cscg.Tests.Tools.TestHelper;

namespace Cscg.Tests
{
    public class GeneratorTest
    {
        [Fact]
        public void TestBinary()
        {
            var gen = new CompactGenerator();
            var (_, source) = GetLocalFile("Simple/DisplayValues.cs");

            var input = source.CreateCompilation(addedRefs: [GetMetaRef<CborReader>()]);
            var (output, run) = input.RunGenerators(out var dia, [gen], []);

            dia.CheckNoError(output, 3);
            run.CheckNoError(2);
        }

        [Fact]
        public void TestAdo()
        {
            var gen = new AdoGenerator();
            var (_, source) = GetLocalFile("Sql/Person.cs");

            var input = source.CreateCompilation(addedRefs: [GetMetaRef<DbConnection>(), GetMetaRef<SqliteBlob>()]);
            var (output, run) = input.RunGenerators(out var dia, [gen], []);

            dia.CheckNoError(output, 9);
            run.CheckNoError(8);
        }

        [Fact]
        public void TestConcise()
        {
            var gen = new CompactGenerator();
            var (_, source) = GetLocalFile("Complex/Animals.cs");

            var input = source.CreateCompilation(addedRefs: [GetMetaRef<CborReader>()]);
            var (output, run) = input.RunGenerators(out var dia, [gen], []);

            dia.CheckNoError(output, 9);
            run.CheckNoError(8);
        }

        [Fact]
        public void TestStub()
        {
            var gen = new StubGenerator();
            var (_, source) = GetLocalFile("Program.st.cs");
            AddedMemoryText at = GetLocalFile("res/Newtonsoft.Json.Bson.xml");

            var input = source.CreateCompilation(addedRefs: [GetMetaRef<JsonCloneSettings>()]);
            var (output, run) = input.RunGenerators(out var dia, [gen], [at]);

            dia.CheckNoError(output, 2);
            run.CheckNoError(1);
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