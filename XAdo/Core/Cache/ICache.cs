using System;
using System.Collections.Generic;

namespace XAdo.Core.Cache
{
   public interface ICache<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
   {
      TValue GetOrAdd(TKey key, Func<TKey, TValue> factory);
      int Count { get; }
   }
}
