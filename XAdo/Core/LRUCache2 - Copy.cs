using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace XAdo.Core
{
   public class LRUCache2<TKey, TValue> : IDictionary<TKey, TValue>, IDisposable
   {
      private enum CacheActionType
      {
         Get,
         Add,
         Remove
      }

      private class CacheManager
      {
         private readonly ConcurrentDictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>
            _dict;
         private readonly LinkedList<LinkedListNode<Tuple<TKey, TValue>>>
            _list;
         private readonly int
            _capacity;

         public class CacheAction
         {
            public CacheActionType Type;
            public LinkedListNode<Tuple<TKey, TValue>> Item;
         }

         private readonly Queue<CacheAction>
            _queue = new Queue<CacheAction>();

         private readonly object _lock = new object();
         private bool _stop;
         private Task _consumer;

         public CacheManager(ConcurrentDictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>> dict, LinkedList<LinkedListNode<Tuple<TKey, TValue>>> list, int capacity)
         {
            _dict = dict;
            _list = list;
            _capacity = capacity;
            _consumer = Task.Factory.StartNew(Consume);
         }

         public void Produce(CacheAction action)
         {
            lock (_lock)
            {
               if (_consumer == null)
               {
                  _consumer = Task.Factory.StartNew(Consume);
               }

               _queue.Enqueue(action);
               Monitor.Pulse(_lock);
            }
         }

         private void Consume()
         {
            lock (_lock)
            {
               Monitor.Pulse(_lock);
               while (Monitor.Wait(_lock, TimeSpan.FromSeconds(60)))
               {
                  if (_stop)
                  {
                     return;
                  }
                  while (_queue.Any())
                  {
                     var action = _queue.Dequeue();
                     switch (action.Type)
                     {
                        case CacheActionType.Get:
                           _list.Remove(action.Item);
                           _list.AddLast(action.Item);
                           break;
                        case CacheActionType.Add:
                           _list.AddLast(action.Item);
                           if (_list.Count > _capacity)
                           {
                              var first = _list.First;
                              _list.RemoveFirst();
                              LinkedListNode<Tuple<TKey, TValue>> value;
                              _dict.TryRemove(first.Value.Value.Item1, out value);
                           }
                           break;
                        case CacheActionType.Remove:
                           _list.Remove(action.Item);
                           break;
                        default:
                           throw new ArgumentOutOfRangeException();
                     }
                  }
               }
               _consumer = null;
            }
         }

         public void Stop()
         {
            lock (_lock)
            {
               if (_stop) return;
               _stop = true;
               Monitor.Pulse(_lock);
            }
         }
      }

      private const int DefaultCapacity = 500;

      private readonly ConcurrentDictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>
         _dict = new ConcurrentDictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>();

      private readonly LinkedList<LinkedListNode<Tuple<TKey, TValue>>>
          _list = new LinkedList<LinkedListNode<Tuple<TKey, TValue>>>();

      private int _capacity = 500;

      private CacheManager _syncManager;

      public LRUCache2(string capacityKey, int defaultCapacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
      {
         int configuredValue;
         _capacity = capacityKey != null && int.TryParse(ConfigurationManager.AppSettings[capacityKey], out configuredValue) ? configuredValue : defaultCapacity;

         if (comparer != null)
         {
            _dict = new ConcurrentDictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>(comparer);
         }
         _syncManager = new CacheManager(_dict, _list, _capacity);
      }

      public LRUCache2(int capacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
      {
         _capacity = capacity;
         if (comparer != null)
         {
            _dict = new ConcurrentDictionary<TKey, LinkedListNode<Tuple<TKey, TValue>>>(comparer);
         }
         _syncManager = new CacheManager(_dict, _list, _capacity);
      }

      public LRUCache2(IEnumerable<KeyValuePair<TKey, TValue>> other, string capacityKey = null, int capacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
         : this(capacityKey, capacity, comparer)
      {
         var otherLRUCache = other as LRUCache2<TKey, TValue>;
         if (otherLRUCache != null)
         {
            foreach (var node in otherLRUCache._list)
            {
               // keep order
               AddNode(node.Value.Item1, node.Value.Item2);
            }
         }
         else
         {
            foreach (var kv in other)
            {
               AddNode(kv.Key, kv.Value);
            }
         }
      }

      public int Capacity
      {
         get { return _capacity; }
      }

      public bool ContainsKey(TKey key)
      {
         return _dict.ContainsKey(key);
      }

      public void Add(TKey key, TValue value)
      {
         if (!TryAdd(key, value))
         {
            throw new InvalidOperationException("key already exists");
         }
      }

      public bool Remove(TKey key)
      {
         return TryRemove(key);
      }

      public bool TryGetValue(TKey key, out TValue value)
      {
         LinkedListNode<Tuple<TKey, TValue>> item;
         if (_dict.TryGetValue(key, out item))
         {
            value = item.Value.Item2;
            _syncManager.Produce(new CacheManager.CacheAction { Type = CacheActionType.Get, Item = item });
            return true;
         }
         value = default(TValue);
         return false;
      }

      public TValue this[TKey key]
      {
         get
         {
            TValue value;
            if (!TryGetValue(key, out value))
            {
               throw new KeyNotFoundException("key not found in dictionary");
            }
            return value;
         }
         set
         {
            Remove(key);
            TryAdd(key, value);
         }
      }

      public ICollection<TKey> Keys
      {
         get { return _dict.Keys.ToArray(); }
      }

      public ICollection<TValue> Values
      {
         get
         {
            return _dict.Values.ToArray().Select(e => e.Value.Item2).ToArray();
         }
      }

      public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
      {
         LinkedListNode<Tuple<TKey, TValue>> added = null;
         var item = _dict.GetOrAdd(key, k => added = new LinkedListNode<Tuple<TKey, TValue>>(Tuple.Create(key, factory(key))));
         _syncManager.Produce(new CacheManager.CacheAction { Type = ReferenceEquals(item, added) ? CacheActionType.Add : CacheActionType.Get, Item = item });
         return item.Value.Item2;
      }

      public bool TryRemove(TKey key)
      {
         LinkedListNode<Tuple<TKey, TValue>> node;

         if (!_dict.TryRemove(key, out node))
            return false;
         _syncManager.Produce(new CacheManager.CacheAction { Type = CacheActionType.Remove, Item = node });
         return true;
      }
      public bool TryAdd(TKey key, TValue value)
      {
         var node = new LinkedListNode<Tuple<TKey, TValue>>(Tuple.Create(key, value));

         if (!_dict.TryAdd(key, node))
            return false;
         _syncManager.Produce(new CacheManager.CacheAction { Type = CacheActionType.Add, Item = node });
         return true;
      }

      public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
      {
         return _dict.ToDictionary(e => e.Key, e => e.Value.Value.Item2).GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      public void Add(KeyValuePair<TKey, TValue> item)
      {
         if (!TryAdd(item.Key, item.Value))
         {
            throw new InvalidOperationException("could not add key value pair");
         }
      }

      public void Clear()
      {
         _dict.Clear();
         Task.Run(() =>
         {
            lock (_list)
            {
               _list.Clear();
            }
         });
      }

      public bool Contains(KeyValuePair<TKey, TValue> item)
      {
         LinkedListNode<Tuple<TKey, TValue>> node;
         return _dict.TryGetValue(item.Key, out node) && Equals(node.Value.Item2, item.Value);
      }

      public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
      {
         _dict.ToDictionary(x => x.Key, x => x.Value).Select(b => new KeyValuePair<TKey, TValue>(b.Key, b.Value.Value.Item2)).ToArray().CopyTo(array, arrayIndex);
      }

      public bool Remove(KeyValuePair<TKey, TValue> item)
      {
         if (Contains(item))
         {
            return TryRemove(item.Key);
         }
         return false;
      }

      public int Count
      {
         get { return _dict.Count; }
      }

      public bool IsReadOnly
      {
         get { return false; }
      }

      private void AddNode(TKey key, TValue value)
      {
         if ((_capacity > 0 && _dict.Count >= _capacity))
         {
            PurgeOne();
         }
         var node = new LinkedListNode<Tuple<TKey, TValue>>(Tuple.Create(key, value));
         if (_dict.TryAdd(key, node))
         {
            _list.AddLast(node);
         }
      }

      private void PurgeOne()
      {
         if (_list.Count == 0) return;
         var first = _list.First;
         _list.RemoveFirst();
         LinkedListNode<Tuple<TKey, TValue>> value;
         _dict.TryRemove(first.Value.Value.Item1, out value);
      }

      public void Dispose()
      {
         _syncManager.Stop();
      }
   }
}
