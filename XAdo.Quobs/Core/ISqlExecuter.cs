using System;
using System.Collections.Generic;
using System.Data;
using XAdo.Quobs.Dialect;

namespace XAdo.Quobs.Core
{
   public interface ISqlExecuter
   {
      T ExecuteScalar<T>(string sql, IDictionary<string, object> args);
      IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args);
      IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord,T> binder, IDictionary<string, object> args);

      IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args, out int count);
      IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> binder, IDictionary<string, object> args, out int count);
      int Execute(string sql, IDictionary<string, object> args);
      bool HasSqlQueue { get; }
      bool EnqueueSql(string sql, IDictionary<string,object> args);
      ISqlFormatter GetSqlFormatter();
   }
}
