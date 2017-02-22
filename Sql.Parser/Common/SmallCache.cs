using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Sql.Parser.Common
{
   public class SmallCache<TKey, TValue> : ICache<TKey, TValue>
   {
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

      private ConcurrentDictionary<TKey, TValue> _dict;

      public TValue GetOrAdd(TKey key, Func<TKey, TValue> factory)
      {
         if (_dict != null)
         {
            return _dict.GetOrAdd(key, factory);
         }

         TValue value;
         if (TryGet(_lock1, ref _key1, ref _value1, key, factory, out value)) return value;
         if (TryGet(_lock2, ref _key2, ref _value2, key, factory, out value)) return value;
         if (TryGet(_lock3, ref _key3, ref _value3, key, factory, out value)) return value;
         if (TryGet(_lock4, ref _key4, ref _value4, key, factory, out value)) return value;

         lock (_lock5)
         {
            if (_dict == null)
            {
               _dict = _dict ?? new ConcurrentDictionary<TKey, TValue>(new[]
                        {
                           new KeyValuePair<TKey, TValue>(_key1, _value1),
                           new KeyValuePair<TKey, TValue>(_key2, _value2),
                           new KeyValuePair<TKey, TValue>(_key3, _value3),
                           new KeyValuePair<TKey, TValue>(_key4, _value4),
                        });

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
         return _dict.GetOrAdd(key, factory);
      }


      private bool TryGet(object @lock, ref TKey key, ref TValue value, TKey arg, Func<TKey, TValue> factory, out TValue outValue)
      {
         outValue = default(TValue);
         if (_dict != null) return false;
         lock (@lock)
         {
            return _dict == null && TryGet(ref key, ref value, arg, factory, out outValue);
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

   }
}