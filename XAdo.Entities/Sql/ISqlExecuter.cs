using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;

namespace XAdo.Quobs.Sql
{
   public interface ISqlExecuter
   {
      T ExecuteScalar<T>(string sql, IDictionary<string, object> args);
      IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args);
      IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args, string sqlCount, out long count);
      IEnumerable<T> ExecuteQuery<T>(string sql, Expression<Func<IDataRecord,T>> factory, IDictionary<string, object> args);
      IEnumerable<T> ExecuteQuery<T>(string sql, Expression<Func<IDataRecord, T>> factory, IDictionary<string, object> args, string sqlCount, out long count);
   }
}
