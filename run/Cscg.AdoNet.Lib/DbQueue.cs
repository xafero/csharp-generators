using System.Collections;
using System.Collections.Generic;

namespace Cscg.AdoNet.Lib
{
    public abstract class DbQueue
    {
        public abstract IEnumerable<object> AsEnumerable();
    }

    public sealed class DbQueue<TEntity> : DbQueue, IEnumerable<TEntity>
        where TEntity : class
    {
        private readonly IDictionary<object, TEntity> _queue;
        private int _idx;

        public DbQueue()
        {
            _queue = new Dictionary<object, TEntity>();
            _idx = 0;
        }

        public void Enqueue(TEntity item)
        {
            if (item is IHasId<int> hi && hi.Id != default)
                _queue[hi.Id] = item;
            else if (item is IHasId<string> { Id: not null } hs)
                _queue[hs.Id] = item;
            else if (item is IHasId<(string, string)> hm)
                _queue[hm.Id] = item;
            else
                _queue[++_idx] = item;
        }

        public void Clear() => _queue.Clear();
        public override IEnumerable<object> AsEnumerable() => this;
        public IEnumerator<TEntity> GetEnumerator() => _queue.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => $"{_queue.Count} items";
    }
}