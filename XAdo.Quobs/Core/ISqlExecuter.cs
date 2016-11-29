using System;
using System.Collections.Generic;
using System.Data;
using XAdo.Quobs.Dialect;

namespace XAdo.Quobs.Core
{
   public interface ISqlExecuter
   {
      T ExecuteScalar<T>(string sql, object args);
      IEnumerable<T> ExecuteQuery<T>(string sql, object args);
      IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord,T> binder, object args);

      IEnumerable<T> ExecuteQuery<T>(string sql, object args, out int count);
      IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> binder, object args, out int count);
      int Execute(string sql, object args);
      bool HasSqlQueue { get; }
      bool EnqueueSql(string sql,object args);
      ISqlFormatter GetSqlFormatter();
   }
}
