using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;

namespace XAdo.Core
{
   
   [DebuggerTypeProxy(typeof(IDictionaryDebugView<,>))]
   [DebuggerDisplay("Count = {Count}")]
   [Serializable]
   public class ConcurrentLRUDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary, IReadOnlyDictionary<TKey, TValue>
   {
      private LinkedList<Node> _nodes = new LinkedList<Node>();

      private sealed class Tables
      {
         internal readonly Node[] _buckets; // A singly-linked list for each bucket.
         internal readonly object[] _locks; // A set of locks, each guarding a section of the table.
         internal volatile int[] _countPerLock; // The number of elements guarded by each lock.

         internal Tables(Node[] buckets, object[] locks, int[] countPerLock)
         {
            _buckets = buckets;
            _locks = locks;
            _countPerLock = countPerLock;
         }
      }

      [NonSerialized]
      private volatile Tables _tables; // Internal tables of the dictionary
      private IEqualityComparer<TKey> _comparer; // Key equality comparer
      [NonSerialized]
      private readonly bool _growLockArray; // Whether to dynamically increase the size of the striped lock
      [NonSerialized]
      private int _budget; // The maximum number of elements per lock before a resize operation is triggered

      private KeyValuePair<TKey, TValue>[] _serializationArray; // Used for custom serialization
      private int _serializationConcurrencyLevel; // used to save the concurrency level in serialization
      private int _serializationCapacity; // used to save the capacity in serialization

      // The default capacity, i.e. the initial # of buckets. When choosing this value, we are making
      // a trade-off between the size of a very small dictionary, and the number of resizes when
      // constructing a large dictionary. Also, the capacity should not be divisible by a small prime.
      private const int DefaultCapacity = 31;

      // The maximum size of the striped lock that will not be exceeded when locks are automatically
      // added as the dictionary grows. However, the user is allowed to exceed this limit by passing
      // a concurrency level larger than MaxLockNumber into the constructor.
      private const int MaxLockNumber = 1024;

      // Whether TValue is a type that can be written atomically (i.e., with no danger of torn reads)
      private static readonly bool s_isValueWriteAtomic = IsValueWriteAtomic();

      
      private static bool IsValueWriteAtomic()
      {
         //
         // Section 12.6.6 of ECMA CLI explains which types can be read and written atomically without
         // the risk of tearing.
         //
         // See http://www.ecma-international.org/publications/files/ECMA-ST/Ecma-335.pdf
         //
         Type valueType = typeof(TValue);
         if (!valueType.IsValueType)
         {
            return true;
         }

         switch (Type.GetTypeCode(valueType))
         {
            case TypeCode.Boolean:
            case TypeCode.Byte:
            case TypeCode.Char:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.SByte:
            case TypeCode.Single:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
               return true;
            case TypeCode.Int64:
            case TypeCode.Double:
            case TypeCode.UInt64:
               return IntPtr.Size == 8;
            default:
               return false;
         }
      }

      
      public ConcurrentLRUDictionary() : this(DefaultConcurrencyLevel, DefaultCapacity, true, EqualityComparer<TKey>.Default) { }

      
      public ConcurrentLRUDictionary(int concurrencyLevel, int capacity) : this(concurrencyLevel, capacity, false, EqualityComparer<TKey>.Default) { }

      
      public ConcurrentLRUDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection) : this(collection, EqualityComparer<TKey>.Default) { }

      
      public ConcurrentLRUDictionary(IEqualityComparer<TKey> comparer) : this(DefaultConcurrencyLevel, DefaultCapacity, true, comparer) { }

      
      public ConcurrentLRUDictionary(IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
         : this(comparer)
      {
         if (collection == null) throw new ArgumentNullException("collection");

         InitializeFromCollection(collection);
      }

      
      public ConcurrentLRUDictionary(
          int concurrencyLevel, IEnumerable<KeyValuePair<TKey, TValue>> collection, IEqualityComparer<TKey> comparer)
         : this(concurrencyLevel, DefaultCapacity, false, comparer)
      {
         if (collection == null) throw new ArgumentNullException("collection");

         InitializeFromCollection(collection);
      }

      private void InitializeFromCollection(IEnumerable<KeyValuePair<TKey, TValue>> collection)
      {
         TValue dummy;
         foreach (KeyValuePair<TKey, TValue> pair in collection)
         {
            if (pair.Key == null) ThrowKeyNullException();

            if (!TryAddInternal(pair.Key, _comparer.GetHashCode(pair.Key), pair.Value, false, false, out dummy))
            {
               throw new ArgumentException("ConcurrentDictionary_SourceContainsDuplicateKeys");
            }
         }

         if (_budget == 0)
         {
            _budget = _tables._buckets.Length / _tables._locks.Length;
         }
      }

      
      public ConcurrentLRUDictionary(int concurrencyLevel, int capacity, IEqualityComparer<TKey> comparer)
         : this(concurrencyLevel, capacity, false, comparer)
      {
      }

      internal ConcurrentLRUDictionary(int concurrencyLevel, int capacity, bool growLockArray, IEqualityComparer<TKey> comparer)
      {
         if (concurrencyLevel < 1)
         {
            throw new ArgumentOutOfRangeException("concurrencyLevel", "ConcurrentDictionary_ConcurrencyLevelMustBePositive");
         }
         if (capacity < 0)
         {
            throw new ArgumentOutOfRangeException("capacity", "ConcurrentDictionary_CapacityMustNotBeNegative");
         }
         if (comparer == null) throw new ArgumentNullException("comparer");

         // The capacity should be at least as large as the concurrency level. Otherwise, we would have locks that don't guard
         // any buckets.
         if (capacity < concurrencyLevel)
         {
            capacity = concurrencyLevel;
         }

         object[] locks = new object[concurrencyLevel];
         for (int i = 0; i < locks.Length; i++)
         {
            locks[i] = new object();
         }

         int[] countPerLock = new int[locks.Length];
         Node[] buckets = new Node[capacity];
         _tables = new Tables(buckets, locks, countPerLock);

         _comparer = comparer;
         _growLockArray = growLockArray;
         _budget = buckets.Length / locks.Length;
      }

      /// <summary>Get the data array to be serialized.</summary>
      [OnSerializing]
      private void OnSerializing(StreamingContext context)
      {
         Tables tables = _tables;

         // save the data into the serialization array to be saved
         _serializationArray = ToArray();
         _serializationConcurrencyLevel = tables._locks.Length;
         _serializationCapacity = tables._buckets.Length;
      }

      /// <summary>Clear the serialized state.</summary>
      [OnSerialized]
      private void OnSerialized(StreamingContext context)
      {
         _serializationArray = null;
      }

      /// <summary>Construct the dictionary from a previously serialized one</summary>
      [OnDeserialized]
      private void OnDeserialized(StreamingContext context)
      {
         KeyValuePair<TKey, TValue>[] array = _serializationArray;

         var buckets = new Node[_serializationCapacity];
         var countPerLock = new int[_serializationConcurrencyLevel];
         var locks = new object[_serializationConcurrencyLevel];
         for (int i = 0; i < locks.Length; i++)
         {
            locks[i] = new object();
         }
         _tables = new Tables(buckets, locks, countPerLock);

         InitializeFromCollection(array);
         _serializationArray = null;
      }

      
      public bool TryAdd(TKey key, TValue value)
      {
         if (key == null) ThrowKeyNullException();
         TValue dummy;
         return TryAddInternal(key, _comparer.GetHashCode(key), value, false, true, out dummy);
      }

      
      public bool ContainsKey(TKey key)
      {
         if (key == null) ThrowKeyNullException();

         TValue throwAwayValue;
         return TryGetValue(key, out throwAwayValue);
      }

      
      public bool TryRemove(TKey key, out TValue value)
      {
         if (key == null) ThrowKeyNullException();

         return TryRemoveInternal(key, out value, false, default(TValue));
      }

      
      private bool TryRemoveInternal(TKey key, out TValue value, bool matchValue, TValue oldValue)
      {
         int hashcode = _comparer.GetHashCode(key);
         while (true)
         {
            Tables tables = _tables;

            int bucketNo, lockNo;
            GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables._buckets.Length, tables._locks.Length);

            lock (tables._locks[lockNo])
            {
               // If the table just got resized, we may not be holding the right lock, and must retry.
               // This should be a rare occurrence.
               if (tables != _tables)
               {
                  continue;
               }

               Node prev = null;
               for (Node curr = tables._buckets[bucketNo]; curr != null; curr = curr._next)
               {
                  Debug.Assert((prev == null && curr == tables._buckets[bucketNo]) || prev._next == curr);

                  if (hashcode == curr._hashcode && _comparer.Equals(curr._key, key))
                  {
                     if (matchValue)
                     {
                        bool valuesMatch = EqualityComparer<TValue>.Default.Equals(oldValue, curr._value);
                        if (!valuesMatch)
                        {
                           value = default(TValue);
                           return false;
                        }
                     }

                     if (prev == null)
                     {
                        Volatile.Write<Node>(ref tables._buckets[bucketNo], curr._next);
                     }
                     else
                     {
                        prev._next = curr._next;
                     }

                     value = curr._value;
                     tables._countPerLock[lockNo]--;
                     return true;
                  }
                  prev = curr;
               }
            }

            value = default(TValue);
            return false;
         }
      }

      
      public bool TryGetValue(TKey key, out TValue value)
      {
         if (key == null) ThrowKeyNullException();
         return TryGetValueInternal(key, _comparer.GetHashCode(key), out value);
      }

      private bool TryGetValueInternal(TKey key, int hashcode, out TValue value)
      {
         Debug.Assert(_comparer.GetHashCode(key) == hashcode);

         // We must capture the _buckets field in a local variable. It is set to a new table on each table resize.
         Tables tables = _tables;

         int bucketNo = GetBucket(hashcode, tables._buckets.Length);

         // We can get away w/out a lock here.
         // The Volatile.Read ensures that the load of the fields of 'n' doesn't move before the load from buckets[i].
         Node n = Volatile.Read<Node>(ref tables._buckets[bucketNo]);

         while (n != null)
         {
            if (hashcode == n._hashcode && _comparer.Equals(n._key, key))
            {
               value = n._value;
               return true;
            }
            n = n._next;
         }

         value = default(TValue);
         return false;
      }

      
      public bool TryUpdate(TKey key, TValue newValue, TValue comparisonValue)
      {
         if (key == null) ThrowKeyNullException();
         return TryUpdateInternal(key, _comparer.GetHashCode(key), newValue, comparisonValue);
      }

      public bool IfContains(TKey key, TValue value, Action action)
      {
         int hashcode = _comparer.GetHashCode(key);

         IEqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;

         while (true)
         {
            int bucketNo;
            int lockNo;

            Tables tables = _tables;
            GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables._buckets.Length, tables._locks.Length);

            lock (tables._locks[lockNo])
            {
               // If the table just got resized, we may not be holding the right lock, and must retry.
               // This should be a rare occurrence.
               if (tables != _tables)
               {
                  continue;
               }

               // Try to find this key in the bucket
               Node prev = null;
               for (Node node = tables._buckets[bucketNo]; node != null; node = node._next)
               {
                  Debug.Assert((prev == null && node == tables._buckets[bucketNo]) || prev._next == node);
                  if (hashcode == node._hashcode && _comparer.Equals(node._key, key))
                  {
                     if (valueComparer.Equals(node._value, value))
                     {
                        action();
                        return true;
                     }

                     return false;
                  }

                  prev = node;
               }

               //didn't find the key
               return false;
            }
         }
      }

      
      private bool TryUpdateInternal(TKey key, int hashcode, TValue newValue, TValue comparisonValue)
      {
         Debug.Assert(_comparer.GetHashCode(key) == hashcode);

         IEqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;

         while (true)
         {
            int bucketNo;
            int lockNo;

            Tables tables = _tables;
            GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables._buckets.Length, tables._locks.Length);

            lock (tables._locks[lockNo])
            {
               // If the table just got resized, we may not be holding the right lock, and must retry.
               // This should be a rare occurrence.
               if (tables != _tables)
               {
                  continue;
               }

               // Try to find this key in the bucket
               Node prev = null;
               for (Node node = tables._buckets[bucketNo]; node != null; node = node._next)
               {
                  Debug.Assert((prev == null && node == tables._buckets[bucketNo]) || prev._next == node);
                  if (hashcode == node._hashcode && _comparer.Equals(node._key, key))
                  {
                     if (valueComparer.Equals(node._value, comparisonValue))
                     {
                        if (s_isValueWriteAtomic)
                        {
                           node._value = newValue;
                        }
                        else
                        {
                           Node newNode = new Node(node._key, newValue, hashcode, node._next);

                           if (prev == null)
                           {
                              tables._buckets[bucketNo] = newNode;
                           }
                           else
                           {
                              prev._next = newNode;
                           }
                        }

                        return true;
                     }

                     return false;
                  }

                  prev = node;
               }

               //didn't find the key
               return false;
            }
         }
      }

      /// <summary>
      /// Removes all keys and values from the <see cref="ConcurrentLRUDictionary{TKey,TValue}"/>.
      /// </summary>
      public void Clear()
      {
         int locksAcquired = 0;
         try
         {
            AcquireAllLocks(ref locksAcquired);

            Tables newTables = new Tables(new Node[DefaultCapacity], _tables._locks, new int[_tables._countPerLock.Length]);
            _tables = newTables;
            _budget = Math.Max(1, newTables._buckets.Length / newTables._locks.Length);
         }
         finally
         {
            ReleaseLocks(0, locksAcquired);
         }
      }

      
      void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int index)
      {
         if (array == null) throw new ArgumentNullException("array");
         if (index < 0) throw new ArgumentOutOfRangeException("index", "ConcurrentDictionary_IndexIsNegative");

         int locksAcquired = 0;
         try
         {
            AcquireAllLocks(ref locksAcquired);

            int count = 0;

            for (int i = 0; i < _tables._locks.Length && count >= 0; i++)
            {
               count += _tables._countPerLock[i];
            }

            if (array.Length - count < index || count < 0) //"count" itself or "count + index" can overflow
            {
               throw new ArgumentException("ConcurrentDictionary_ArrayNotLargeEnough");
            }

            CopyToPairs(array, index);
         }
         finally
         {
            ReleaseLocks(0, locksAcquired);
         }
      }

      
      public KeyValuePair<TKey, TValue>[] ToArray()
      {
         int locksAcquired = 0;
         try
         {
            AcquireAllLocks(ref locksAcquired);
            int count = 0;
            checked
            {
               for (int i = 0; i < _tables._locks.Length; i++)
               {
                  count += _tables._countPerLock[i];
               }
            }

            if (count == 0)
            {
               return new KeyValuePair<TKey, TValue>[0];
            }

            KeyValuePair<TKey, TValue>[] array = new KeyValuePair<TKey, TValue>[count];
            CopyToPairs(array, 0);
            return array;
         }
         finally
         {
            ReleaseLocks(0, locksAcquired);
         }
      }

      
      private void CopyToPairs(KeyValuePair<TKey, TValue>[] array, int index)
      {
         Node[] buckets = _tables._buckets;
         for (int i = 0; i < buckets.Length; i++)
         {
            for (Node current = buckets[i]; current != null; current = current._next)
            {
               array[index] = new KeyValuePair<TKey, TValue>(current._key, current._value);
               index++; //this should never flow, CopyToPairs is only called when there's no overflow risk
            }
         }
      }

      
      private void CopyToEntries(DictionaryEntry[] array, int index)
      {
         Node[] buckets = _tables._buckets;
         for (int i = 0; i < buckets.Length; i++)
         {
            for (Node current = buckets[i]; current != null; current = current._next)
            {
               array[index] = new DictionaryEntry(current._key, current._value);
               index++;  //this should never flow, CopyToEntries is only called when there's no overflow risk
            }
         }
      }

      
      private void CopyToObjects(object[] array, int index)
      {
         Node[] buckets = _tables._buckets;
         for (int i = 0; i < buckets.Length; i++)
         {
            for (Node current = buckets[i]; current != null; current = current._next)
            {
               array[index] = new KeyValuePair<TKey, TValue>(current._key, current._value);
               index++; //this should never flow, CopyToObjects is only called when there's no overflow risk
            }
         }
      }

      
      public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
      {
         Node[] buckets = _tables._buckets;

         for (int i = 0; i < buckets.Length; i++)
         {
            // The Volatile.Read ensures that the load of the fields of 'current' doesn't move before the load from buckets[i].
            Node current = Volatile.Read<Node>(ref buckets[i]);

            while (current != null)
            {
               yield return new KeyValuePair<TKey, TValue>(current._key, current._value);
               current = current._next;
            }
         }
      }

      
      private bool TryAddInternal(TKey key, int hashcode, TValue value, bool updateIfExists, bool acquireLock, out TValue resultingValue)
      {
         Debug.Assert(_comparer.GetHashCode(key) == hashcode);

         while (true)
         {
            int bucketNo, lockNo;

            Tables tables = _tables;
            GetBucketAndLockNo(hashcode, out bucketNo, out lockNo, tables._buckets.Length, tables._locks.Length);

            bool resizeDesired = false;
            bool lockTaken = false;
            try
            {
               if (acquireLock)
                  Monitor.Enter(tables._locks[lockNo], ref lockTaken);

               // If the table just got resized, we may not be holding the right lock, and must retry.
               // This should be a rare occurrence.
               if (tables != _tables)
               {
                  continue;
               }

               // Try to find this key in the bucket
               Node prev = null;
               for (Node node = tables._buckets[bucketNo]; node != null; node = node._next)
               {
                  Debug.Assert((prev == null && node == tables._buckets[bucketNo]) || prev._next == node);
                  if (hashcode == node._hashcode && _comparer.Equals(node._key, key))
                  {
                     // The key was found in the dictionary. If updates are allowed, update the value for that key.
                     // We need to create a new node for the update, in order to support TValue types that cannot
                     // be written atomically, since lock-free reads may be happening concurrently.
                     if (updateIfExists)
                     {
                        if (s_isValueWriteAtomic)
                        {
                           node._value = value;
                        }
                        else
                        {
                           Node newNode = new Node(node._key, value, hashcode, node._next);
                           if (prev == null)
                           {
                              tables._buckets[bucketNo] = newNode;
                           }
                           else
                           {
                              prev._next = newNode;
                           }
                        }
                        resultingValue = value;
                     }
                     else
                     {
                        resultingValue = node._value;
                     }
                     return false;
                  }
                  prev = node;
               }

               // The key was not found in the bucket. Insert the key-value pair.
               Volatile.Write<Node>(ref tables._buckets[bucketNo], new Node(key, value, hashcode, tables._buckets[bucketNo]));
               checked
               {
                  tables._countPerLock[lockNo]++;
               }

               //
               // If the number of elements guarded by this lock has exceeded the budget, resize the bucket table.
               // It is also possible that GrowTable will increase the budget but won't resize the bucket table.
               // That happens if the bucket table is found to be poorly utilized due to a bad hash function.
               //
               if (tables._countPerLock[lockNo] > _budget)
               {
                  resizeDesired = true;
               }
            }
            finally
            {
               if (lockTaken)
                  Monitor.Exit(tables._locks[lockNo]);
            }

            //
            // The fact that we got here means that we just performed an insertion. If necessary, we will grow the table.
            //
            // Concurrency notes:
            // - Notice that we are not holding any locks at when calling GrowTable. This is necessary to prevent deadlocks.
            // - As a result, it is possible that GrowTable will be called unnecessarily. But, GrowTable will obtain lock 0
            //   and then verify that the table we passed to it as the argument is still the current table.
            //
            if (resizeDesired)
            {
               GrowTable(tables);
            }

            resultingValue = value;
            return true;
         }
      }

      
      public TValue this[TKey key]
      {
         get
         {
            TValue value;
            if (!TryGetValue(key, out value))
            {
               ThrowKeyNotFoundException();
            }
            return value;
         }
         set
         {
            if (key == null) ThrowKeyNullException();
            TValue dummy;
            TryAddInternal(key, _comparer.GetHashCode(key), value, true, true, out dummy);
         }
      }

      // These exception throwing sites have been extracted into their own NoInlining methods
      // as these are uncommonly needed and when inlined are observed to prevent the inlining
      // of important methods like TryGetValue and ContainsKey.

      [MethodImpl(MethodImplOptions.NoInlining)]
      private static void ThrowKeyNotFoundException()
      {
         throw new KeyNotFoundException();
      }

      [MethodImpl(MethodImplOptions.NoInlining)]
      private static void ThrowKeyNullException()
      {
         throw new ArgumentNullException("key");
      }

      
      public int Count
      {
         get
         {
            int acquiredLocks = 0;
            try
            {
               // Acquire all locks
               AcquireAllLocks(ref acquiredLocks);

               return GetCountInternal();
            }
            finally
            {
               // Release locks that have been acquired earlier
               ReleaseLocks(0, acquiredLocks);
            }
         }
      }

      
      private int GetCountInternal()
      {
         int count = 0;

         // Compute the count, we allow overflow
         for (int i = 0; i < _tables._countPerLock.Length; i++)
         {
            count += _tables._countPerLock[i];
         }

         return count;
      }

      
      public TValue GetOrAdd(TKey key, Func<TKey, TValue> valueFactory)
      {
         if (key == null) ThrowKeyNullException();
         if (valueFactory == null) throw new ArgumentNullException("valueFactory");

         int hashcode = _comparer.GetHashCode(key);

         TValue resultingValue;
         if (!TryGetValueInternal(key, hashcode, out resultingValue))
         {
            TryAddInternal(key, hashcode, valueFactory(key), false, true, out resultingValue);
         }
         return resultingValue;
      }

      
      public TValue GetOrAdd<TArg>(TKey key, Func<TKey, TArg, TValue> valueFactory, TArg factoryArgument)
      {
         if (key == null) throw new ArgumentNullException("key");
         if (valueFactory == null) throw new ArgumentNullException("valueFactory");

         int hashcode = _comparer.GetHashCode(key);

         TValue resultingValue;
         if (!TryGetValueInternal(key, hashcode, out resultingValue))
         {
            TryAddInternal(key, hashcode, valueFactory(key, factoryArgument), false, true, out resultingValue);
         }
         return resultingValue;
      }

      
      public TValue GetOrAdd(TKey key, TValue value)
      {
         if (key == null) ThrowKeyNullException();

         int hashcode = _comparer.GetHashCode(key);

         TValue resultingValue;
         if (!TryGetValueInternal(key, hashcode, out resultingValue))
         {
            TryAddInternal(key, hashcode, value, false, true, out resultingValue);
         }
         return resultingValue;
      }

      
      public TValue AddOrUpdate<TArg>(
          TKey key, Func<TKey, TArg, TValue> addValueFactory, Func<TKey, TValue, TArg, TValue> updateValueFactory, TArg factoryArgument)
      {
         if (key == null) throw new ArgumentNullException("key");
         if (addValueFactory == null) throw new ArgumentNullException("addValueFactory");
         if (updateValueFactory == null) throw new ArgumentNullException("updateValueFactory");

         int hashcode = _comparer.GetHashCode(key);

         while (true)
         {
            TValue oldValue;
            if (TryGetValueInternal(key, hashcode, out oldValue))
            {
               // key exists, try to update
               TValue newValue = updateValueFactory(key, oldValue, factoryArgument);
               if (TryUpdateInternal(key, hashcode, newValue, oldValue))
               {
                  return newValue;
               }
            }
            else
            {
               // key doesn't exist, try to add
               TValue resultingValue;
               if (TryAddInternal(key, hashcode, addValueFactory(key, factoryArgument), false, true, out resultingValue))
               {
                  return resultingValue;
               }
            }
         }
      }

      
      public TValue AddOrUpdate(TKey key, Func<TKey, TValue> addValueFactory, Func<TKey, TValue, TValue> updateValueFactory)
      {
         if (key == null) ThrowKeyNullException();
         if (addValueFactory == null) throw new ArgumentNullException("addValueFactory");
         if (updateValueFactory == null) throw new ArgumentNullException("updateValueFactory");

         int hashcode = _comparer.GetHashCode(key);

         while (true)
         {
            TValue oldValue;
            if (TryGetValueInternal(key, hashcode, out oldValue))
            {
               // key exists, try to update
               TValue newValue = updateValueFactory(key, oldValue);
               if (TryUpdateInternal(key, hashcode, newValue, oldValue))
               {
                  return newValue;
               }
            }
            else
            {
               // key doesn't exist, try to add
               TValue resultingValue;
               if (TryAddInternal(key, hashcode, addValueFactory(key), false, true, out resultingValue))
               {
                  return resultingValue;
               }
            }
         }
      }

      
      public TValue AddOrUpdate(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateValueFactory)
      {
         if (key == null) ThrowKeyNullException();
         if (updateValueFactory == null) throw new ArgumentNullException("updateValueFactory");

         int hashcode = _comparer.GetHashCode(key);

         while (true)
         {
            TValue oldValue;
            if (TryGetValueInternal(key, hashcode, out oldValue))
            {
               // key exists, try to update
               TValue newValue = updateValueFactory(key, oldValue);
               if (TryUpdateInternal(key, hashcode, newValue, oldValue))
               {
                  return newValue;
               }
            }
            else
            {
               // key doesn't exist, try to add
               TValue resultingValue;
               if (TryAddInternal(key, hashcode, addValue, false, true, out resultingValue))
               {
                  return resultingValue;
               }
            }
         }
      }

      
      public bool IsEmpty
      {
         get
         {
            int acquiredLocks = 0;
            try
            {
               // Acquire all locks
               AcquireAllLocks(ref acquiredLocks);

               for (int i = 0; i < _tables._countPerLock.Length; i++)
               {
                  if (_tables._countPerLock[i] != 0)
                  {
                     return false;
                  }
               }
            }
            finally
            {
               // Release locks that have been acquired earlier
               ReleaseLocks(0, acquiredLocks);
            }

            return true;
         }
      }

      #region IDictionary<TKey,TValue> members

      
      void IDictionary<TKey, TValue>.Add(TKey key, TValue value)
      {
         if (!TryAdd(key, value))
         {
            throw new ArgumentException("ConcurrentDictionary_KeyAlreadyExisted");
         }
      }

      
      bool IDictionary<TKey, TValue>.Remove(TKey key)
      {
         TValue throwAwayValue;
         return TryRemove(key, out throwAwayValue);
      }

      
      public ICollection<TKey> Keys
      {
         get { return GetKeys(); }
      }

      
      IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
      {
         get { return GetKeys(); }
      }

      
      public ICollection<TValue> Values
      {
         get { return GetValues(); }
      }

      
      IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
      {
         get { return GetValues(); }
      }
      #endregion

      #region ICollection<KeyValuePair<TKey,TValue>> Members

      
      void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> keyValuePair)
      {
         ((IDictionary<TKey, TValue>)this).Add(keyValuePair.Key, keyValuePair.Value);
      }

      
      bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> keyValuePair)
      {
         TValue value;
         if (!TryGetValue(keyValuePair.Key, out value))
         {
            return false;
         }
         return EqualityComparer<TValue>.Default.Equals(value, keyValuePair.Value);
      }

      
      bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly
      {
         get { return false; }
      }

      
      bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> keyValuePair)
      {
         if (keyValuePair.Key == null) throw new ArgumentNullException("keyValuePair", "ConcurrentDictionary_ItemKeyIsNull");

         TValue throwAwayValue;
         return TryRemoveInternal(keyValuePair.Key, out throwAwayValue, true, keyValuePair.Value);
      }

      #endregion

      #region IEnumerable Members

      
      IEnumerator IEnumerable.GetEnumerator()
      {
         return ((ConcurrentLRUDictionary<TKey, TValue>)this).GetEnumerator();
      }

      #endregion

      #region IDictionary Members

      
      void IDictionary.Add(object key, object value)
      {
         if (key == null) ThrowKeyNullException();
         if (!(key is TKey)) throw new ArgumentException("ConcurrentDictionary_TypeOfKeyIncorrect");

         TValue typedValue;
         try
         {
            typedValue = (TValue)value;
         }
         catch (InvalidCastException)
         {
            throw new ArgumentException("ConcurrentDictionary_TypeOfValueIncorrect");
         }

         ((IDictionary<TKey, TValue>)this).Add((TKey)key, typedValue);
      }

      
      bool IDictionary.Contains(object key)
      {
         if (key == null) ThrowKeyNullException();

         return (key is TKey) && this.ContainsKey((TKey)key);
      }

     
      IDictionaryEnumerator IDictionary.GetEnumerator()
      {
         return new DictionaryEnumerator(this);
      }

      
      bool IDictionary.IsFixedSize
      {
         get { return false; }
      }

      
      bool IDictionary.IsReadOnly
      {
         get { return false; }
      }

      
      ICollection IDictionary.Keys
      {
         get { return GetKeys(); }
      }

      
      void IDictionary.Remove(object key)
      {
         if (key == null) ThrowKeyNullException();

         TValue throwAwayValue;
         if (key is TKey)
         {
            TryRemove((TKey)key, out throwAwayValue);
         }
      }

      
      ICollection IDictionary.Values
      {
         get { return GetValues(); }
      }

      
      object IDictionary.this[object key]
      {
         get
         {
            if (key == null) ThrowKeyNullException();

            TValue value;
            if (key is TKey && TryGetValue((TKey)key, out value))
            {
               return value;
            }

            return null;
         }
         set
         {
            if (key == null) ThrowKeyNullException();

            if (!(key is TKey)) throw new ArgumentException("ConcurrentDictionary_TypeOfKeyIncorrect");
            if (!(value is TValue)) throw new ArgumentException("ConcurrentDictionary_TypeOfValueIncorrect");

            ((ConcurrentLRUDictionary<TKey, TValue>)this)[(TKey)key] = (TValue)value;
         }
      }

      #endregion

      #region ICollection Members

      
      void ICollection.CopyTo(Array array, int index)
      {
         if (array == null) throw new ArgumentNullException("array");
         if (index < 0) throw new ArgumentOutOfRangeException("index", "ConcurrentDictionary_IndexIsNegative");

         int locksAcquired = 0;
         try
         {
            AcquireAllLocks(ref locksAcquired);
            Tables tables = _tables;

            int count = 0;

            for (int i = 0; i < tables._locks.Length && count >= 0; i++)
            {
               count += tables._countPerLock[i];
            }

            if (array.Length - count < index || count < 0) //"count" itself or "count + index" can overflow
            {
               throw new ArgumentException("ConcurrentDictionary_ArrayNotLargeEnough");
            }

            // To be consistent with the behavior of ICollection.CopyTo() in Dictionary<TKey,TValue>,
            // we recognize three types of target arrays:
            //    - an array of KeyValuePair<TKey, TValue> structs
            //    - an array of DictionaryEntry structs
            //    - an array of objects

            KeyValuePair<TKey, TValue>[] pairs = array as KeyValuePair<TKey, TValue>[];
            if (pairs != null)
            {
               CopyToPairs(pairs, index);
               return;
            }

            DictionaryEntry[] entries = array as DictionaryEntry[];
            if (entries != null)
            {
               CopyToEntries(entries, index);
               return;
            }

            object[] objects = array as object[];
            if (objects != null)
            {
               CopyToObjects(objects, index);
               return;
            }

            throw new ArgumentException("ConcurrentDictionary_ArrayIncorrectType", "array");
         }
         finally
         {
            ReleaseLocks(0, locksAcquired);
         }
      }

      
      bool ICollection.IsSynchronized
      {
         get { return false; }
      }

      
      object ICollection.SyncRoot
      {
         get
         {
            throw new NotSupportedException("ConcurrentCollection_SyncRoot_NotSupported");
         }
      }

      #endregion

      /// <summary>
      /// Replaces the bucket table with a larger one. To prevent multiple threads from resizing the
      /// table as a result of races, the Tables instance that holds the table of buckets deemed too
      /// small is passed in as an argument to GrowTable(). GrowTable() obtains a lock, and then checks
      /// the Tables instance has been replaced in the meantime or not.
      /// </summary>
      private void GrowTable(Tables tables)
      {
         const int MaxArrayLength = 0X7FEFFFFF;
         int locksAcquired = 0;
         try
         {
            // The thread that first obtains _locks[0] will be the one doing the resize operation
            AcquireLocks(0, 1, ref locksAcquired);

            // Make sure nobody resized the table while we were waiting for lock 0:
            if (tables != _tables)
            {
               // We assume that since the table reference is different, it was already resized (or the budget
               // was adjusted). If we ever decide to do table shrinking, or replace the table for other reasons,
               // we will have to revisit this logic.
               return;
            }

            // Compute the (approx.) total size. Use an Int64 accumulation variable to avoid an overflow.
            long approxCount = 0;
            for (int i = 0; i < tables._countPerLock.Length; i++)
            {
               approxCount += tables._countPerLock[i];
            }

            //
            // If the bucket array is too empty, double the budget instead of resizing the table
            //
            if (approxCount < tables._buckets.Length / 4)
            {
               _budget = 2 * _budget;
               if (_budget < 0)
               {
                  _budget = int.MaxValue;
               }
               return;
            }


            // Compute the new table size. We find the smallest integer larger than twice the previous table size, and not divisible by
            // 2,3,5 or 7. We can consider a different table-sizing policy in the future.
            int newLength = 0;
            bool maximizeTableSize = false;
            try
            {
               checked
               {
                  // Double the size of the buckets table and add one, so that we have an odd integer.
                  newLength = tables._buckets.Length * 2 + 1;

                  // Now, we only need to check odd integers, and find the first that is not divisible
                  // by 3, 5 or 7.
                  while (newLength % 3 == 0 || newLength % 5 == 0 || newLength % 7 == 0)
                  {
                     newLength += 2;
                  }

                  Debug.Assert(newLength % 2 != 0);

                  if (newLength > MaxArrayLength)
                  {
                     maximizeTableSize = true;
                  }
               }
            }
            catch (OverflowException)
            {
               maximizeTableSize = true;
            }

            if (maximizeTableSize)
            {
               newLength = MaxArrayLength;

               // We want to make sure that GrowTable will not be called again, since table is at the maximum size.
               // To achieve that, we set the budget to int.MaxValue.
               //
               // (There is one special case that would allow GrowTable() to be called in the future: 
               // calling Clear() on the ConcurrentDictionary will shrink the table and lower the budget.)
               _budget = int.MaxValue;
            }

            // Now acquire all other locks for the table
            AcquireLocks(1, tables._locks.Length, ref locksAcquired);

            object[] newLocks = tables._locks;

            // Add more locks
            if (_growLockArray && tables._locks.Length < MaxLockNumber)
            {
               newLocks = new object[tables._locks.Length * 2];
               Array.Copy(tables._locks, 0, newLocks, 0, tables._locks.Length);
               for (int i = tables._locks.Length; i < newLocks.Length; i++)
               {
                  newLocks[i] = new object();
               }
            }

            Node[] newBuckets = new Node[newLength];
            int[] newCountPerLock = new int[newLocks.Length];

            // Copy all data into a new table, creating new nodes for all elements
            for (int i = 0; i < tables._buckets.Length; i++)
            {
               Node current = tables._buckets[i];
               while (current != null)
               {
                  Node next = current._next;
                  int newBucketNo, newLockNo;
                  GetBucketAndLockNo(current._hashcode, out newBucketNo, out newLockNo, newBuckets.Length, newLocks.Length);

                  newBuckets[newBucketNo] = new Node(current._key, current._value, current._hashcode, newBuckets[newBucketNo]);

                  checked
                  {
                     newCountPerLock[newLockNo]++;
                  }

                  current = next;
               }
            }

            // Adjust the budget
            _budget = Math.Max(1, newBuckets.Length / newLocks.Length);

            // Replace tables with the new versions
            _tables = new Tables(newBuckets, newLocks, newCountPerLock);
         }
         finally
         {
            // Release all locks that we took earlier
            ReleaseLocks(0, locksAcquired);
         }
      }

      /// <summary>
      /// Computes the bucket for a particular key. 
      /// </summary>
      private static int GetBucket(int hashcode, int bucketCount)
      {
         int bucketNo = (hashcode & 0x7fffffff) % bucketCount;
         Debug.Assert(bucketNo >= 0 && bucketNo < bucketCount);
         return bucketNo;
      }

      /// <summary>
      /// Computes the bucket and lock number for a particular key. 
      /// </summary>
      private static void GetBucketAndLockNo(int hashcode, out int bucketNo, out int lockNo, int bucketCount, int lockCount)
      {
         bucketNo = (hashcode & 0x7fffffff) % bucketCount;
         lockNo = bucketNo % lockCount;

         Debug.Assert(bucketNo >= 0 && bucketNo < bucketCount);
         Debug.Assert(lockNo >= 0 && lockNo < lockCount);
      }

      /// <summary>
      /// The number of concurrent writes for which to optimize by default.
      /// </summary>
      private static int DefaultConcurrencyLevel
      {
         get { return Environment.ProcessorCount; }
      }

      /// <summary>
      /// Acquires all locks for this hash table, and increments locksAcquired by the number
      /// of locks that were successfully acquired. The locks are acquired in an increasing
      /// order.
      /// </summary>
      private void AcquireAllLocks(ref int locksAcquired)
      {
         
         // First, acquire lock 0
         AcquireLocks(0, 1, ref locksAcquired);

         // Now that we have lock 0, the _locks array will not change (i.e., grow),
         // and so we can safely read _locks.Length.
         AcquireLocks(1, _tables._locks.Length, ref locksAcquired);
         Debug.Assert(locksAcquired == _tables._locks.Length);
      }

      /// <summary>
      /// Acquires a contiguous range of locks for this hash table, and increments locksAcquired
      /// by the number of locks that were successfully acquired. The locks are acquired in an
      /// increasing order.
      /// </summary>
      private void AcquireLocks(int fromInclusive, int toExclusive, ref int locksAcquired)
      {
         Debug.Assert(fromInclusive <= toExclusive);
         object[] locks = _tables._locks;

         for (int i = fromInclusive; i < toExclusive; i++)
         {
            bool lockTaken = false;
            try
            {
               Monitor.Enter(locks[i], ref lockTaken);
            }
            finally
            {
               if (lockTaken)
               {
                  locksAcquired++;
               }
            }
         }
      }

      /// <summary>
      /// Releases a contiguous range of locks.
      /// </summary>
      private void ReleaseLocks(int fromInclusive, int toExclusive)
      {
         Debug.Assert(fromInclusive <= toExclusive);

         for (int i = fromInclusive; i < toExclusive; i++)
         {
            Monitor.Exit(_tables._locks[i]);
         }
      }

      /// <summary>
      /// Gets a collection containing the keys in the dictionary.
      /// </summary>
      private ReadOnlyCollection<TKey> GetKeys()
      {
         int locksAcquired = 0;
         try
         {
            AcquireAllLocks(ref locksAcquired);

            int count = GetCountInternal();
            if (count < 0) throw new OutOfMemoryException();

            List<TKey> keys = new List<TKey>(count);
            for (int i = 0; i < _tables._buckets.Length; i++)
            {
               Node current = _tables._buckets[i];
               while (current != null)
               {
                  keys.Add(current._key);
                  current = current._next;
               }
            }

            return new ReadOnlyCollection<TKey>(keys);
         }
         finally
         {
            ReleaseLocks(0, locksAcquired);
         }
      }

      /// <summary>
      /// Gets a collection containing the values in the dictionary.
      /// </summary>
      private ReadOnlyCollection<TValue> GetValues()
      {
         int locksAcquired = 0;
         try
         {
            AcquireAllLocks(ref locksAcquired);

            int count = GetCountInternal();
            if (count < 0) throw new OutOfMemoryException();

            List<TValue> values = new List<TValue>(count);
            for (int i = 0; i < _tables._buckets.Length; i++)
            {
               Node current = _tables._buckets[i];
               while (current != null)
               {
                  values.Add(current._value);
                  current = current._next;
               }
            }

            return new ReadOnlyCollection<TValue>(values);
         }
         finally
         {
            ReleaseLocks(0, locksAcquired);
         }
      }

      /// <summary>
      /// A node in a singly-linked list representing a particular hash table bucket.
      /// </summary>
      [Serializable]
      private sealed class Node
      {
         internal readonly TKey _key;
         internal TValue _value;
         internal volatile Node _next;
         internal readonly int _hashcode;

         internal Node(TKey key, TValue value, int hashcode, Node next)
         {
            _key = key;
            _value = value;
            _next = next;
            _hashcode = hashcode;
         }
      }

      /// <summary>
      /// A private class to represent enumeration over the dictionary that implements the 
      /// IDictionaryEnumerator interface.
      /// </summary>
      [Serializable]
      private sealed class DictionaryEnumerator : IDictionaryEnumerator
      {
         IEnumerator<KeyValuePair<TKey, TValue>> _enumerator; // Enumerator over the dictionary.

         internal DictionaryEnumerator(ConcurrentLRUDictionary<TKey, TValue> dictionary)
         {
            _enumerator = dictionary.GetEnumerator();
         }

         public DictionaryEntry Entry
         {
            get { return new DictionaryEntry(_enumerator.Current.Key, _enumerator.Current.Value); }
         }

         public object Key
         {
            get { return _enumerator.Current.Key; }
         }

         public object Value
         {
            get { return _enumerator.Current.Value; }
         }

         public object Current
         {
            get { return Entry; }
         }

         public bool MoveNext()
         {
            return _enumerator.MoveNext();
         }

         public void Reset()
         {
            _enumerator.Reset();
         }
      }
   }

   internal sealed class IDictionaryDebugView<K, V>
   {
      private readonly IDictionary<K, V> _dictionary;

      public IDictionaryDebugView(IDictionary<K, V> dictionary)
      {
         if (dictionary == null)
            throw new ArgumentNullException("dictionary");

         _dictionary = dictionary;
      }

      [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
      public KeyValuePair<K, V>[] Items
      {
         get
         {
            KeyValuePair<K, V>[] items = new KeyValuePair<K, V>[_dictionary.Count];
            _dictionary.CopyTo(items, 0);
            return items;
         }
      }
   }
}