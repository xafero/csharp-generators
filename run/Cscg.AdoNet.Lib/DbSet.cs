namespace Cscg.AdoNet.Lib
{
    public abstract class DbSet<TEntity>
        where TEntity : class
    {
        public DbContext Context { get; protected set; }

        public abstract void Add(TEntity entity);

        public abstract void Save(TEntity entity);
    }
}