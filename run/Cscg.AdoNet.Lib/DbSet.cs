namespace Cscg.AdoNet.Lib
{
    public abstract class DbSet<TEntity>
        where TEntity : class
    {
        public DbContext Context { get; protected set; }
    }
}