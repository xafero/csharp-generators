using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public interface IActiveWrite<in TCommand>
        where TCommand : DbCommand
    {
        void WriteSql(TCommand command);
    }
}