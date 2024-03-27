namespace Cscg.AdoNet.Lib
{
    public interface IDbContextFactory<out TContext>
        where TContext : DbContext
    {
        TContext Create();
    }
}