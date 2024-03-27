using System;

namespace Cscg.AdoNet.Lib
{
    public sealed class DbContextFactory<TContext> : IDbContextFactory<TContext>
        where TContext : DbContext
    {
        private readonly Func<TContext> _func;

        public DbContextFactory(Func<TContext> func)
        {
            _func = func;
        }

        public TContext Create()
        {
            return _func();
        }
    }
}