using System.IO;
using Cscg.Core.Model;
using Cscg.Tests.Tools;
using Xunit;
using XmlTool = Cscg.Core.XmlTool<Cscg.Core.Model.CTypes>;

namespace Cscg.Tests
{
    public class ModelTest
    {
        [Fact]
        public void TestSample()
        {
            const string tmp = "model_sample.xml";

            var cTypes = new CTypes
            {
                Types =
                [
                    new()
                    {
                        Name = "Sample", Kind = TypeKind.Class, Parent = "Testing",
                        Members =
                        [
                            new CMember
                            {
                                Name = "Size", Type = "int", Kind = MemberKind.Property,
                                Attributes = [new CAttribute { Type = "Key", Params = [] }]
                            }
                        ],
                        Attributes =
                        [
                            new()
                            {
                                Type = "Description", Params = [new() { Value = "42", Name = "Count" }]
                            }
                        ]
                    },
                    new()
                    {
                        Name = "Rest", Kind = TypeKind.Class, Parent = "Super",
                        Members = [],
                        Attributes = []
                    }
                ]
            };

            var jsonIn = DebugTool.ToJson(cTypes);
            string jsonOut;

            using (var fileOut = File.CreateText(tmp))
                XmlTool.Write(cTypes, fileOut);

            using (var fileIn = File.OpenText(tmp))
            {
                var model = XmlTool.Read(fileIn);
                Assert.Equal(2, model.Types.Count);

                jsonOut = DebugTool.ToJson(model);
            }

            Assert.Equal(jsonIn, jsonOut);
        }
    }
}