using System;

namespace Sql.Parser.Common
{
   public interface ICache<TKey, TValue>
   {
      TValue GetOrAdd(TKey key, Func<TKey, TValue> factory);
   }
}
