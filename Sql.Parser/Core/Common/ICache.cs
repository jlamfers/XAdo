using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace Sql.Parser.Common
{
   public interface ICache<TKey, TValue> : IEnumerable<KeyValuePair<TKey,TValue>>
   {
      TValue GetOrAdd(TKey key, Func<TKey, TValue> factory);
      int Count { get; }
   }
}
