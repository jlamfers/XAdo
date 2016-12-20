using System;
using System.Collections.Generic;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public interface IFetchSqlObject<T> : IReadSqlObject
   {
      List<T> FetchToList(out int count);
      List<T> FetchToList();
      T[] FetchToArray(out int count);
      T[] FetchToArray();
      IDictionary<TKey, TValue> FetchToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> elementSelector, out int count);
      IDictionary<TKey, TValue> FetchToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> elementSelector);
      IDictionary<TKey, List<TValue>> FetchToGroupedDictionary<TKey, TValue>(Func<T, TKey> groupKeySelector, Func<T, TValue> listElementSelector);
      T FetchSingleOrDefault();
      T FetchSingle();
      IEnumerable<T> FetchToEnumerable();
      IEnumerable<T> FetchToEnumerable(out int count);
   }
}