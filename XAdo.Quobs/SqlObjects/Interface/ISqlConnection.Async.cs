using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using XAdo.SqlObjects.Dialects;

namespace XAdo.SqlObjects.SqlObjects.Interface
{

   public partial interface ISqlConnection
   {
      Task<T> ExecuteScalarAsync<T>(string sql, object args);

      Task<List<T>> ExecuteQueryAsync<T>(string sql, object args);
      Task<List<T>> ExecuteQueryAsync<T>(string sql, Func<IDataRecord, T> binder, object args);

      Task<AsyncPagedResult<T>> ExecutePagedQueryAsync<T>(string sql, object args);
      Task<AsyncPagedResult<T>> ExecutePagedQueryAsync<T>(string sql, Func<IDataRecord, T> binder, object args);

      Task<int> ExecuteAsync(string sql, object args);

   }

}
