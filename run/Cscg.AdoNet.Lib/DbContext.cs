using System;
using System.Data;
using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public abstract class DbContext<TConn> : IDisposable
        where TConn : DbConnection, new()
    {
        protected string DataSource { get; set; }

        private Lazy<TConn>? _conn;

        public virtual TConn CreateConn()
        {
            var conn = AdoTool.OpenConn<TConn>(GetConnStr());
            if (IsDatabaseEmpty(conn))
                CreateTables(conn);
            return conn;
        }

        protected abstract string GetConnStr();

        protected abstract bool IsDatabaseEmpty(TConn conn);

        protected abstract void CreateTables(TConn conn);

        public virtual TConn GetOpenConn()
        {
            if (_conn == null || _conn.Value.State != ConnectionState.Open)
            {
                DisposeOld();
                _conn = new Lazy<TConn>(CreateConn);
            }
            return _conn.Value;
        }

        private void DisposeOld()
        {
            if (_conn is not { IsValueCreated: true })
                return;
            _conn.Value.Dispose();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            DisposeOld();
        }
    }
}