using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs
{
   public static class AdoSessionExtension
   {
      private class SqlExecuter : ISqlExecuter
      {
         private readonly IAdoSession _session;

         public SqlExecuter(IAdoSession session)
         {
            _session = session;
         }

         public T ExecuteScalar<T>(string sql, IDictionary<string, object> args)
         {
            return _session.ExecuteScalar<T>(sql, args);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args)
         {
            return _session.Query<T>(sql, args, false);
         }
         public IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args, out int count)
         {
            var reader = _session.QueryMultiple(sql, args);
            count = reader.Read<int>().Single();
            return reader.Read<T>(false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> factory, IDictionary<string, object> args)
         {
            return _session.Query(sql, factory, args, false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> factory, IDictionary<string, object> args, out int count)
         {
            var countBinder = new Func<IDataRecord, int>(r => r.GetInt32(0));
            var factories = new Delegate[] {countBinder, factory};
            var result = _session.QueryMultiple(sql, factories, args);
            count = result.Read<int>().Single();
            return result.Read<T>(false);
         }
      }

      public static Quob<T> From<T>(this IAdoSession self)
      {
         const string key = "quobs.sql.formatter";
         object formatter;

         if (!self.Context.Items.TryGetValue(key, out formatter))
         {
            throw new QuobException("Missing SQL formatter. You need to specify a SQL formatter on your AdoContext initialization (using the initializer parameter), e.g., i => i.SetItem(\""+key+"\",new Ms2012SqlFormatter())");
         }

         if (!(formatter is ISqlFormatter))
         {
            throw new QuobException("Invalid SQL formatter: the SQL formmater must implement interface type " + typeof(ISqlFormatter));
         }

         return new Quob<T>(formatter.CastTo<ISqlFormatter>(), new SqlExecuter(self));
      }
   }
}
