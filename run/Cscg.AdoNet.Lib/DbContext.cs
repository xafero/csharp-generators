using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public abstract class DbContext : IDisposable
    {
        protected string DataSource { get; set; }

        protected abstract string GetConnStr();

        public abstract DbConnection GetDbConn();

        public abstract void Enqueue(object obj);

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

        protected abstract IEnumerable<(DbQueue, DbSet)> LoopQueues();
        protected abstract void ClearQueues();

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

        public void SaveChanges()
        {
            using var conn = GetDbConn();
            try
            {
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "BEGIN TRANSACTION;";
                    cmd.ExecuteNonQuery();
                }
                foreach (var (queue, set) in LoopQueues())
                {
                    var waiting = new Queue<object>(queue.AsEnumerable());
                    while (waiting.Count >= 1 && waiting.Dequeue() is { } obj)
                        set.Save(obj);
                }
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "COMMIT;";
                    cmd.ExecuteNonQuery();
                }
                ClearQueues();
            }
            catch (Exception)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "ROLLBACK;";
                cmd.ExecuteNonQuery();
            }
        }
    }
}