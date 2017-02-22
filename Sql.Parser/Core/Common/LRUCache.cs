using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Sql.Parser.Common
{
   public class LRUCache<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, ICache<TKey,TValue>
   {

      private const int DefaultCapacity = 500;

      private readonly int
          _capacity;

      private readonly Dictionary<TKey, LinkedListNode<KeyValuePair<TKey,TValue>>>
          _dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();

      private readonly LinkedList<KeyValuePair<TKey,TValue>>
          _list = new LinkedList<KeyValuePair<TKey, TValue>>();

      private readonly object _syncRoot = new object();

      public LRUCache(int capacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
      {
         _capacity = capacity;
         if (comparer != null)
         {
            _dict = new Dictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
         }
      }
      public LRUCache(IEnumerable<KeyValuePair<TKey, TValue>> other, int capacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
         : this(capacity, comparer)
      {
         var otherLRUCache = other as LRUCache<TKey, TValue>;
         if (otherLRUCache != null)
         {
            foreach (var kv in otherLRUCache._list)
            {
               // keep order
               Add(kv.Key, kv.Value);
            }
         }
         else
         {
            foreach (var kv in other)
            {
               Add(kv.Key, kv.Value);
            }
         }
      }

      public int Capacity
      {
         get { return _capacity; }
      }

      public bool TryGetValue(TKey key, out TValue value)
      {
         value = default(TValue);
         lock (_syncRoot)
         {
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (!_dict.TryGetValue(key, out node))
            {
               return false;
            }
            value = node.Value.Value;
            _list.Remove(node);
            _list.AddLast(node);
         }
         return true;
      }

      public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
      {
         TValue value;
         if (TryGetValue(key, out value))
         {
            return value;
         }

         // invoke factory outside locks => it may be invoked more than once
         var v = factory(key);

         lock (_syncRoot)
         {
            if (TryGetValue(key, out value))
            {
               return value;
            }
            Add(key, v);
            return v;
         }
      }

      public void Add(TKey key, TValue value)
      {
         lock (_syncRoot)
         {
            if ((_capacity > 0 && _dict.Count >= _capacity))
            {
               PurgeOne();
            }
            var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key,value));
            _list.AddLast(node);
            _dict.Add(key, node);
         }

      }

      public bool TryAdd(TKey key, TValue value)
      {
         lock (_syncRoot)
         {
            if (_dict.ContainsKey(key))
            {
               return false;
            }
            Add(key, value);
            return true;
         }
      }

      public bool TryRemove(TKey key, out TValue value)
      {
         value = default(TValue);

         lock (_syncRoot)
         {
            LinkedListNode<KeyValuePair<TKey, TValue>> node;
            if (!_dict.TryGetValue(key, out node))
            {
               return false;
            }
            _list.Remove(node);
            return true;
         }
      }

      private void PurgeOne()
      {
         lock (_syncRoot)
         {
            if (_list.Count == 0) return;
            var first = _list.First;
            _list.RemoveFirst();
            _dict.Remove(first.Value.Key);
         }
      }

      public bool ContainsKey(TKey key)
      {
         lock (_syncRoot)
         {
            return _dict.ContainsKey(key);
         }
      }

      public bool Remove(TKey key)
      {
         TValue value;
         return TryRemove(key, out value);
      }

      public TValue this[TKey key]
      {
         get
         {
            TValue value;
            if (TryGetValue(key, out value))
            {
               return value;
            }
            throw new KeyNotFoundException("key not found in LRU cache");
         }
         set
         {
            lock (_syncRoot)
            {
               if (ContainsKey(key))
               {
                  Remove(key);
               }
               Add(key, value);
            }
         }
      }

      public ICollection<TKey> Keys
      {
         get
         {
            lock (_syncRoot)
            {
               return _dict.Keys.ToArray();
            }
         }
      }

      public ICollection<TValue> Values
      {
         get
         {
            lock (_syncRoot)
            {
               return _dict.Values.Select(v => v.Value.Value).ToArray();
            }
         }
      }

      public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
      {
         lock (_syncRoot)
         {
            return _list.Select(b => new KeyValuePair<TKey, TValue>(b.Key, b.Value)).GetEnumerator();
         }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public void Add(KeyValuePair<TKey, TValue> item)
      {
         Add(item.Key, item.Value);
      }

      public void Clear()
      {
         lock (_syncRoot)
         {
            _dict.Clear();
            _list.Clear();
         }
      }

      public bool Contains(KeyValuePair<TKey, TValue> item)
      {
         lock (_syncRoot)
         {
            TValue value;
            return TryGetValue(item.Key, out value) && Equals(item.Value, value);
         }
      }

      public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
      {
         lock (_syncRoot)
         {
            _list.Select(b => new KeyValuePair<TKey, TValue>(b.Key, b.Value)).ToArray().CopyTo(array, arrayIndex);
         }
      }

      public bool Remove(KeyValuePair<TKey, TValue> item)
      {
         return Remove(item.Key);
      }

      public int Count
      {
         get
         {
            lock (_syncRoot)
            {
               return _dict.Count;
            }
         }
      }

      public bool IsReadOnly { get { return false; } }

      void IDictionary.Add(object key, object value)
      {
         Add((TKey)key, (TValue)value);
      }

      void IDictionary.Clear()
      {
         Clear();
      }

      bool IDictionary.Contains(object key)
      {
         return ContainsKey((TKey)key);
      }

      IDictionaryEnumerator IDictionary.GetEnumerator()
      {
         lock (_syncRoot)
         {
            return _dict.ToDictionary(k => k, v => v).GetEnumerator();
         }
      }

      bool IDictionary.IsFixedSize
      {
         get { return false; }
      }

      bool IDictionary.IsReadOnly
      {
         get { return IsReadOnly; }
      }

      ICollection IDictionary.Keys
      {
         get { return (ICollection)Keys; }
      }

      void IDictionary.Remove(object key)
      {
         Remove((TKey)key);
      }

      ICollection IDictionary.Values
      {
         get { return (ICollection)Values; }
      }

      object IDictionary.this[object key]
      {
         get
         {
            lock (_syncRoot)
            {
               return ContainsKey((TKey)key) ? (object)this[(TKey)key] : null;
            }
         }
         set { this[(TKey)key] = (TValue)value; }
      }

      void ICollection.CopyTo(Array array, int index)
      {
         lock (_syncRoot)
         {
            ((IDictionary)_dict).CopyTo(array, index);
         }
      }

      int ICollection.Count
      {
         get { return Count; }
      }

      bool ICollection.IsSynchronized
      {
         get { return true; }
      }

      object ICollection.SyncRoot
      {
         get { return _syncRoot; }
      }

   }


}