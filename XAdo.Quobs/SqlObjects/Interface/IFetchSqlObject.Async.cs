using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public partial interface IFetchSqlObject<T> : IReadSqlObject
   {
      Task<AsyncPagedResult<T>> FetchPagedToListAsync();
      Task<List<T>> FetchToListAsync();
      Task<IDictionary<TKey, TValue>> FetchToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> elementSelector);
      Task<IDictionary<TKey, List<TValue>>> FetchToGroupedDictionaryAsync<TKey, TValue>(Func<T, TKey> groupKeySelector, Func<T, TValue> listElementSelector);
      Task<T> FetchSingleOrDefaultAsync();
      Task<T> FetchSingleAsync();
   }

}