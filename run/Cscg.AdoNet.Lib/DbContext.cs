using System;
using System.Data;
using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public abstract class DbContext : IDisposable
    {
        protected string DataSource { get; set; }

        protected abstract string GetConnStr();

        public abstract DbConnection GetDbConn();

        protected abstract void DisposeOld();

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeOld();
        }
    }

    public abstract class DbContext<TConn> : DbContext
        where TConn : DbConnection, new()
    {
        private Lazy<TConn>? _conn;

        public virtual TConn CreateConn()
        {
            var conn = AdoTool.OpenConn<TConn>(GetConnStr());
            if (IsDatabaseEmpty(conn))
                CreateTables(conn);
            return conn;
        }

        protected abstract bool IsDatabaseEmpty(TConn conn);

        protected abstract void CreateTables(TConn conn);

        public override DbConnection GetDbConn() => GetOpenConn();

        public virtual TConn GetOpenConn()
        {
            if (_conn == null || _conn.Value.State != ConnectionState.Open)
            {
                DisposeOld();
                _conn = new Lazy<TConn>(CreateConn);
            }
            return _conn.Value;
        }

        protected override void DisposeOld()
        {
            if (_conn is not { IsValueCreated: true })
                return;
            _conn.Value.Dispose();
        }
    }
}