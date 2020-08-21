using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace AGStateMachine.MutatorsStore.Wrappers
{
    /// <summary>
    /// TODO : CHECK IT! TEST IT!
    /// </summary>
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class CwtDictionaryWrapper<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        private readonly ConditionalWeakTable<TKey, TValue> _cwt;

        public CwtDictionaryWrapper( ConditionalWeakTable<TKey, TValue> cwt)
        {
            _cwt = cwt;
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => ((IEnumerable<KeyValuePair<TKey,TValue>>)_cwt).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _cwt.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            _cwt.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new System.NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            return _cwt.Remove(item.Key);
        }

        public int Count => _cwt.Count();
        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            _cwt.Add(key, value);
        }

        public bool ContainsKey(TKey key)
        {
            return _cwt.TryGetValue(key, out var _);
        }

        public bool Remove(TKey key)
        {
            return _cwt.Remove(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            return _cwt.TryGetValue(key, out value);
        }

        public TValue this[TKey key]
        {
            get
            {
                if (this.TryGetValue(key, out var value))
                    return value;
                throw new KeyNotFoundException();
            }
            set => this.Add(key, value);
        }

        public ICollection<TKey> Keys => _cwt.Select(s => s.Key).ToArray();
        public ICollection<TValue> Values => _cwt.Select(s => s.Value).ToArray();
    }
}