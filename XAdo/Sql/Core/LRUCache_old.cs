using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace XAdo.Sql.Core
{
   /// <summary>
   /// Thread safe serializable LRU cache. It implements IDictionary and IDictionary&lt;TKey,TValue>
   /// Capacity is set on construction. On any addition the Least Recently Used item is removed if count equals capacity.
   /// On deserialization the cache is purged.
   /// </summary>
   [Serializable]
   public class LRUCache<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
   {

      private const int DefaultCapacity = 500;

      [Serializable]
      private class Bucket
      {
         public TKey Key;
         public TValue Value;
         public int Index;

         public Bucket WithIndex(int i)
         {
            Index = i;
            return this;
         }
      }

      [Serializable]
      public class LRUEventArgs : EventArgs
      {
         public LRUEventArgs(KeyValuePair<TKey, TValue> bucket)
         {
            Bucket = bucket;
         }
         public KeyValuePair<TKey, TValue> Bucket { get; private set; }
      }

      public event EventHandler<LRUEventArgs> Purge;

      private readonly int
          _capacity;

      private readonly Dictionary<TKey, Bucket>
          _dict = new Dictionary<TKey, Bucket>();

      private readonly List<Bucket>
          _list = new List<Bucket>();

      private readonly object _syncRoot = new object();

      public LRUCache(int capacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
      {
         _capacity = capacity;
         if (comparer != null)
         {
            _dict = new Dictionary<TKey, Bucket>(comparer);
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
            Bucket bucket;
            if (!_dict.TryGetValue(key, out bucket))
            {
               return false;
            }
            value = bucket.Value;
            ShiftLeft(bucket.Index);
            SetLast(bucket);
         }
         return true;
      }

      public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
      {
         lock (_syncRoot)
         {
            TValue value;
            if (TryGetValue(key, out value))
            {
               return value;
            }
         }

         // invoke factory outside locks => it may be invoked more than once
         var v = factory(key);

         lock (_syncRoot)
         {
            TValue value;
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
         bool needPurge;
         lock (_syncRoot)
         {
            needPurge = (_capacity > 0 && _dict.Count >= _capacity);
            var bucket = new Bucket { Key = key, Value = value, Index = _list.Count };
            _list.Add(bucket);
            _dict.Add(key, bucket);
         }

         if (needPurge)
         {
            var removed = PurgeOne();
            if (Purge != null)
            {
               ThreadPool.QueueUserWorkItem(state => OnPurge(new KeyValuePair<TKey, TValue>(removed.Key, removed.Value)));
            }
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
            Bucket bucket;
            if (!_dict.TryGetValue(key, out bucket))
            {
               return false;
            }
            value = bucket.Value;
            ShiftLeft(bucket.Index);
            _list.RemoveAt(_list.Count - 1);
            _dict.Remove(bucket.Key);
            return true;
         }
      }

      private void OnPurge(KeyValuePair<TKey, TValue> removedBucket)
      {
         var handler = Purge;
         if (handler != null)
         {
            handler(this, new LRUEventArgs(removedBucket));
         }
      }

      private Bucket PurgeOne()
      {
         lock (_syncRoot)
         {
            if (_list.Count == 0) return null;
            var bucket = _list[0];
            _dict.Remove(bucket.Key);
            ShiftLeft(0);
            _list.RemoveAt(_list.Count - 1);
            return bucket;
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
               return _dict.Values.Select(v => v.Value).ToArray();
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
         lock (_syncRoot)
         {
            Add(item.Key, item.Value);
         }
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
         lock (_syncRoot)
         {
            if (Contains(item))
            {
               return Remove(item.Key);
            }
         }
         return false;
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

      private void ShiftLeft(int fromIndex)
      {
         for (var i = fromIndex; i < (_list.Count - 1); i++)
         {
            _list[i] = _list[i + 1].WithIndex(i);
         }

      }

      private void SetLast(Bucket bucket)
      {
         var lastIndex = _list.Count - 1;
         _list[lastIndex] = bucket.WithIndex(lastIndex);
      }
   }


}