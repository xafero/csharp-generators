using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public interface IActiveData<in TReader, in TCommand> :
        IActiveRead<TReader>, IActiveWrite<TCommand>
        where TReader : DbDataReader
        where TCommand : DbCommand
    {
    }
}