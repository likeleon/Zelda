using System;
using System.Collections;
using System.Collections.Generic;

namespace Zelda.Game.Primitives
{
    class Cache<T, U> : IReadOnlyDictionary<T, U>
    {
        public int Count => _cache.Count;
        public IEnumerable<T> Keys => _cache.Keys;
        public IEnumerable<U> Values => _cache.Values;

        readonly Dictionary<T, U> _cache;
        readonly Func<T, U> _loader;

        public Cache(Func<T, U> loader, IEqualityComparer<T> c)
        {
            if (loader == null)
                throw new ArgumentNullException(nameof(loader), "loader should not be null");
            
            _loader = loader;
            _cache = new Dictionary<T, U>(c);
        }

        public Cache(Func<T, U> loader)
            : this(loader, EqualityComparer<T>.Default) { }

        public U this[T key]
        {
            get
            {
                U result;
                if (!_cache.TryGetValue(key, out result))
                    _cache.Add(key, result = _loader(key));
                return result;
            }
        }

        public bool ContainsKey(T key) { return _cache.ContainsKey(key); }
        public bool TryGetValue(T key, out U value) { return _cache.TryGetValue(key, out value); }
        public IEnumerator<KeyValuePair<T, U>> GetEnumerator() { return _cache.GetEnumerator(); }
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
    }
}
