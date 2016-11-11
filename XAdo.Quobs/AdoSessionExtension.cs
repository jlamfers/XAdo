using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Formatters;

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
            return _session.Query<T>(sql, factory, args, false);
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
         return new Quob<T>(new Ms2012SqlFormatter(new MemberFormatter()), new SqlExecuter(self));
      }
   }
}
