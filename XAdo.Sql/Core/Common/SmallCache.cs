using System;
using System.Collections;
using System.Collections.Generic;
using XAdo.Core.Cache;

namespace XAdo.Sql.Core.Common
{
   internal class SmallCache<TKey, TValue> : ICache<TKey, TValue>
   {
      private readonly string _capacityKey;
      private readonly int _defaultCapacity;
      private readonly IEqualityComparer<TKey> _comparer;
      private readonly object _lock1 = new object();
      private readonly object _lock2 = new object();
      private readonly object _lock3 = new object();
      private readonly object _lock4 = new object();
      private readonly object _lock5 = new object();

      private TKey _key1;
      private TKey _key2;
      private TKey _key3;
      private TKey _key4;

      private TValue _value1;
      private TValue _value2;
      private TValue _value3;
      private TValue _value4;

      private LRUCache<TKey, TValue> _lruCache;

      public SmallCache(string capacityKey, int defaultCapacity = 100, IEqualityComparer<TKey> comparer = null)
      {
         _capacityKey = capacityKey;
         _defaultCapacity = defaultCapacity;
         _comparer = comparer;
      }
      public SmallCache(int defaultCapacity = 100)
      {
         _defaultCapacity = defaultCapacity;
      }

      public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
      {
         if (_lruCache != null)
         {
            return _lruCache.GetOrAdd(key, factory);
         }

         TValue value;
         if (TryGet(_lock1, ref _key1, ref _value1, key, factory, out value)) return value;
         if (TryGet(_lock2, ref _key2, ref _value2, key, factory, out value)) return value;
         if (TryGet(_lock3, ref _key3, ref _value3, key, factory, out value)) return value;
         if (TryGet(_lock4, ref _key4, ref _value4, key, factory, out value)) return value;

         lock (_lock5)
         {
            if (_lruCache == null)
            {
               _lruCache = _lruCache ?? new LRUCache<TKey, TValue>(new[]
                        {
                           new KeyValuePair<TKey, TValue>(_key1, _value1),
                           new KeyValuePair<TKey, TValue>(_key2, _value2),
                           new KeyValuePair<TKey, TValue>(_key3, _value3),
                           new KeyValuePair<TKey, TValue>(_key4, _value4),
                        },_capacityKey,_defaultCapacity,_comparer);

               _key1 = default(TKey);
               _key2 = default(TKey);
               _key3 = default(TKey);
               _key4 = default(TKey);

               _value1 = default(TValue);
               _value2 = default(TValue);
               _value3 = default(TValue);
               _value4 = default(TValue);

            }
         }
         return _lruCache.GetOrAdd(key, factory);
      }

      public int Count
      {
         get
         {
            if (_lruCache != null) return _lruCache.Count;
            if (!Equals(_key4,default(TKey))) return 4;
            if (!Equals(_key3, default(TKey))) return 3;
            if (!Equals(_key4, default(TKey))) return 2;
            if (!Equals(_key1, default(TKey))) return 1;
            return 0;
         }
      }

      private bool TryGet(object @lock, ref TKey key, ref TValue value, TKey arg, Func<TKey, TValue> factory, out TValue outValue)
      {
         outValue = default(TValue);
         if (_lruCache != null) return false;
         lock (@lock)
         {
            return _lruCache == null && TryGet(ref key, ref value, arg, factory, out outValue);
         }
      }

      private static bool TryGet(ref TKey key, ref TValue value, TKey arg, Func<TKey,TValue> factory, out TValue outValue)
      {
         outValue = default(TValue);

         if (Equals(key, arg))
         {
            outValue = value;
            return true;
         }

         if (!Equals(default(TKey), key)) return false;

         key = arg;
         value = outValue = factory(arg);
         return true;
      }

      public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
      {
         if (_lruCache != null) return _lruCache.GetEnumerator();
         var list = new List<KeyValuePair<TKey, TValue>>();
         lock (_lock1)
         {
            if (!Equals(_key1, default(TKey)))
               list.Add(new KeyValuePair<TKey, TValue>(_key1, _value1));
            else return list.GetEnumerator();
         }
         lock (_lock2)
         {
            if (!Equals(_key2, default(TKey))) list.Add(new KeyValuePair<TKey, TValue>(_key2, _value2));
            else return list.GetEnumerator();
         }
         lock (_lock3)
         {
            if (!Equals(_key3, default(TKey))) list.Add(new KeyValuePair<TKey, TValue>(_key3, _value3));
            else return list.GetEnumerator();
         }
         lock (_lock4)
         {
            if (!Equals(_key4, default(TKey))) list.Add(new KeyValuePair<TKey, TValue>(_key4, _value4));
            else return list.GetEnumerator();
         }
         return _lruCache != null ? _lruCache.GetEnumerator() : list.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}