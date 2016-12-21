using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects.Core
{
   public abstract partial class FetchSqlObject<T> 
   {
      public async virtual Task<AsyncPagedResult<T>> FetchPagedToListAsync()
      {
         EnsureColumnsSelected();
         using (var w = new StringWriter())
         {
            Formatter.WriteCount(w, Chunks);
            w.Write(Formatter.SqlDialect.StatementSeperator);
            if (Chunks.IsPaged())
            {
               Formatter.WritePagedSelect(w, Chunks);
            }
            else
            {
               Formatter.WriteSelect(w, Chunks);
            }
            return await Connection.ExecutePagedQueryAsync<T>(w.GetStringBuilder().ToString(), GetArguments());
         }
      }

      public async virtual Task<List<T>> FetchToListAsync()
      {
         EnsureColumnsSelected();
         using (var sw = new StringWriter())
         {
            WriteSql(sw);
            return await Connection.ExecuteQueryAsync<T>(sw.GetStringBuilder().ToString(), GetArguments());
         }
      }

      public async virtual Task<IDictionary<TKey, TValue>> FetchToDictionaryAsync<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> elementSelector)
      {
         return (await FetchToListAsync()).ToDictionary(keySelector, elementSelector);

      }

      public async virtual Task<IDictionary<TKey, List<TValue>>> FetchToGroupedDictionaryAsync<TKey, TValue>(Func<T, TKey> groupKeySelector, Func<T, TValue> listElementSelector)
      {
         return FetchToGroupedDictionary(await FetchToListAsync(), groupKeySelector, listElementSelector);
      }

      public async virtual Task<T> FetchSingleOrDefaultAsync()
      {
         return (await FetchToListAsync()).SingleOrDefault();
      }

      public async virtual Task<T> FetchSingleAsync()
      {
         return (await FetchToListAsync()).Single();
      }

   }
}