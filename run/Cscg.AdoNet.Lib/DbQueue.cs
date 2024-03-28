using System.Collections;
using System.Collections.Generic;

namespace Cscg.AdoNet.Lib
{
    public sealed class DbQueue<TEntity> : IEnumerable<TEntity>
        where TEntity : class
    {
        private readonly IDictionary<object, TEntity> _queue;
        private int _index;

        public DbQueue()
        {
            _queue = new Dictionary<object, TEntity>();
            _index = 0;
        }

        public void Enqueue(TEntity item)
        {
            if (item is IHasId<int> hi && hi.Id != default)
                _queue[hi.Id] = item;
            else if (item is IHasId<string> { Id: not null } hs)
                _queue[hs.Id] = item;
            else if (!_queue.Values.Contains(item))
                _queue[$"#{++_index}"] = item;
        }

        public void Clear() => _queue.Clear();
        public IEnumerator<TEntity> GetEnumerator() => _queue.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public override string ToString() => $"{_queue.Count} items";
    }
}