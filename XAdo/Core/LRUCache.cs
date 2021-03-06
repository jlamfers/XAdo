﻿using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace XAdo.Core
{
   public class LRUCache<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary
   {

      private const int DefaultCapacity = 500;

      private readonly int
          _capacity;

      private readonly ConcurrentDictionary<TKey, LinkedListNode<KeyValuePair<TKey,TValue>>>
          _dict = new ConcurrentDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>();

      private readonly LinkedList<KeyValuePair<TKey,TValue>>
          _list = new LinkedList<KeyValuePair<TKey, TValue>>();

      public LRUCache(string capacityKey, int defaultCapacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
      {
         int configuredValue;
         _capacity = capacityKey != null && int.TryParse(ConfigurationManager.AppSettings[capacityKey], out configuredValue) ? configuredValue : defaultCapacity;

         if (comparer != null)
         {
            _dict = new ConcurrentDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
         }
      }
      public LRUCache(int capacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
      {
         _capacity = capacity;
         if (comparer != null)
         {
            _dict = new ConcurrentDictionary<TKey, LinkedListNode<KeyValuePair<TKey, TValue>>>(comparer);
         }
      }
      public LRUCache(IEnumerable<KeyValuePair<TKey, TValue>> other, string capacityKey = null, int capacity = DefaultCapacity, IEqualityComparer<TKey> comparer = null)
         : this(capacityKey,capacity, comparer)
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
         LinkedListNode<KeyValuePair<TKey, TValue>> node;
         if (!_dict.TryGetValue(key, out node))
         {
            return false;
         }
         lock (_list)
         {
            value = node.Value.Value;
            if (node.List != null)
            {
               _list.Remove(node);
               _list.AddLast(node);
            }
         }
         return true;
      }

      public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
      {
         LinkedListNode<KeyValuePair<TKey, TValue>> node=null;

         var result = _dict.GetOrAdd(key, k => node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(k, factory(k))));

         var removeKey = default(TKey);

         lock (_list)
         {
            if (ReferenceEquals(result, node))
            {
               _list.AddLast(result);
               if ((_capacity > 0 && _dict.Count >= _capacity))
               {
                  var first = _list.First;
                  if (first != null)
                  {
                     _list.RemoveFirst();
                     removeKey = first.Value.Key;
                  }
               }
            }
            else
            {
               if (result.List != null)
               {
                  _list.Remove(result);
                  _list.AddLast(result);
               }
            }
         }
         if (!Equals(removeKey, default(TKey)))
         {
            _dict.TryRemove(removeKey, out node);
         }
         return result.Value.Value;
      }

      public void Add(TKey key, TValue value)
      {
         if (!TryAdd(key, value))
         {
            throw new InvalidOperationException("key already exists");
         }
      }

      public bool TryAdd(TKey key, TValue value)
      {
         var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key,value));
         if (!_dict.TryAdd(key, node))
         {
            return false;
         }
         var removeKey = default(TKey);
         lock (_list)
         {
            _list.AddLast(node);
            if ((_capacity > 0 && _dict.Count >= _capacity))
            {
               var first = _list.First;
               if (first != null)
               {
                  _list.RemoveFirst();
                  removeKey = first.Value.Key;
               }
            }
         }
         if (!Equals(removeKey, default(TKey)))
         {
            _dict.TryRemove(removeKey, out node);
         }
         return true;
      }

      public bool TryRemove(TKey key, out TValue value)
      {
         value = default(TValue);

         LinkedListNode<KeyValuePair<TKey, TValue>> node;
         if (!_dict.TryRemove(key, out node))
         {
            return false;
         }

         lock (_list)
         {
            if (node.List != null)
            {
               _list.Remove(node);
            }
         }

         return true;
    
      }

      public bool ContainsKey(TKey key)
      {
         return _dict.ContainsKey(key);
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
            var node = new LinkedListNode<KeyValuePair<TKey, TValue>>(new KeyValuePair<TKey, TValue>(key, value));
            var removedValue = default(LinkedListNode<KeyValuePair<TKey, TValue>>);
            _dict.AddOrUpdate(key, k => node, (k, v) =>
            {
               removedValue = v;
               return node;
            });
            var removeKey = default(TKey);
            lock (_list)
            {
               if (removedValue != null && removedValue.List != null)
               {
                  _list.Remove(removedValue);
               }
               _list.AddLast(node);
               if ((_capacity > 0 && _dict.Count >= _capacity))
               {
                  var first = _list.First;
                  if (first != null)
                  {
                     _list.RemoveFirst();
                     removeKey = first.Value.Key;
                  }
               }
            }
            if (!Equals(removeKey, default(TKey)))
            {
               _dict.TryRemove(removeKey, out node);
            }
         }
      }

      public ICollection<TKey> Keys
      {
         get
         {
            return _dict.Keys.ToArray();
         }
      }

      public ICollection<TValue> Values
      {
         get
         {
            return _dict.Values.Select(v => v.Value.Value).ToArray();
          }
      }

      public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
      {
         return _dict.ToDictionary(e => e.Key, e => e.Value.Value.Value).GetEnumerator();
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
         lock (_list)
         {
            _dict.Clear();
            _list.Clear();
         }

      }

      public bool Contains(KeyValuePair<TKey, TValue> item)
      {
         TValue value;
         return TryGetValue(item.Key, out value) && Equals(item.Value, value);
      }

      public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
      {
         _dict.Select(b => new KeyValuePair<TKey, TValue>(b.Key, b.Value.Value.Value)).ToArray().CopyTo(array, arrayIndex);
      }

      public bool Remove(KeyValuePair<TKey, TValue> item)
      {
         return Remove(item.Key);
      }

      public int Count
      {
         get
         {
            return _dict.Count;
            
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
         return _dict.ToDictionary(k => k, v => v.Value.Value.Value).GetEnumerator();
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
            TValue value;
            return TryGetValue((TKey) key, out value) ? (object)value : null;
         }
         set { this[(TKey)key] = (TValue)value; }
      }

      void ICollection.CopyTo(Array array, int index)
      {
         var tmp = new KeyValuePair<TKey, TValue>[array.Length];
         CopyTo(tmp, index);
         for (var i = 0; i < array.Length; i++)
         {
            array.SetValue(tmp[i],i);
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

      private readonly object _syncroot = new object();
      object ICollection.SyncRoot
      {
         get { return _syncroot; }
      }

   }


}