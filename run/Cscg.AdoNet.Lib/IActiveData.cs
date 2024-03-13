using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public interface IActiveData<in TReader, in TCommand> :
        IActiveRead<TReader>, IActiveWrite<TCommand>
        where TReader : DbDataReader
        where TCommand : DbCommand
    {
    }

    public interface IActiveRead<in TReader>
        where TReader : DbDataReader
    {
        void ReadSql(TReader reader, string key, int index);
    }

    public interface IActiveWrite<in TCommand>
        where TCommand : DbCommand
    {
        void WriteSql(TCommand command);
    }
}