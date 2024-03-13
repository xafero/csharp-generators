using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public interface IActiveRead<in TReader>
        where TReader : DbDataReader
    {
        void ReadSql(TReader reader, string key, int index);
    }
}