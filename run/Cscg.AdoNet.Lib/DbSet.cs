namespace Cscg.AdoNet.Lib
{
    public abstract class DbSet
    {
        public DbContext? Context { get; protected set; }

        public abstract void Add(object entity);

        public abstract void Save(object entity);
    }

    public abstract class DbSet<TEntity> : DbSet
        where TEntity : class
    {
        public abstract void Add(TEntity entity);

        public abstract void Save(TEntity entity);
    }
}