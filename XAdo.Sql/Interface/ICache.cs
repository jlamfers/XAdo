using System;
using System.Collections.Generic;

namespace XAdo.Quobs.Interface
{
   public interface ICache<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
   {
      TValue GetOrAdd(TKey key, Func<TKey, TValue> factory);
      int Count { get; }
   }
}
