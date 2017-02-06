using System;
using System.Collections.Generic;
using System.Data;
using XAdo.SqlObjects.Dialects;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public partial interface ISqlConnection
   {
      T ExecuteScalar<T>(string sql, object args);

      IEnumerable<T> ExecuteQuery<T>(string sql, object args);
      IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord,T> binder, object args);

      IEnumerable<T> ExecuteQuery<T>(string sql, object args, out int count);
      IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> binder, object args, out int count);

      int Execute(string sql, object args);

      bool HasSqlBatch { get; }
      void AddToSqlBatch(string sql,object args, Action<object> callback);

      ISqlFormatter GetSqlFormatter();
   }


}
