using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public interface IActiveData<in TReader>
        where TReader : DbDataReader
    {
        void ReadSql(TReader reader, string key, int index);
    }
}