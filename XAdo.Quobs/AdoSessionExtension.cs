using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Dialect;
using XAdo.Quobs.Linq;

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

         public IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> factory,
            IDictionary<string, object> args)
         {
            return _session.Query(sql, factory, args, false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> factory,
            IDictionary<string, object> args, out int count)
         {
            var countBinder = new Func<IDataRecord, int>(r => r.GetInt32(0));
            var factories = new Delegate[] {countBinder, factory};
            var result = _session.QueryMultiple(sql, factories, args);
            count = result.Read<int>().Single();
            return result.Read<T>(false);
         }

         public int Execute(string sql, IDictionary<string, object> args)
         {
            return _session.Execute(sql, args);
         }

         public bool HasUnitOfWork
         {
            get { return _session.UnitOfWork != null; }
         }

         public bool RegisterWork(string sql, IDictionary<string, object> args)
         {
            if (_session.UnitOfWork != null)
            {
               _session.UnitOfWork.Register(sql, args);
               return true;
            }
            return false;
         }

         public ISqlFormatter GetSqlFormatter()
         {
            return _session.GetSqlFormatter();
         }
      }

      public static Quob<T> From<T>(this IAdoSession self)
      {
         return new Quob<T>(new SqlExecuter(self), false);
      }
      public static Upob<T> Update<T>(this IAdoSession self)
      {
         return new Upob<T>(new SqlExecuter(self));
      }
      public static Crob<T> Create<T>(this IAdoSession self)
      {
         return new Crob<T>(new SqlExecuter(self));
      }
      public static Deob<T> Delete<T>(this IAdoSession self)
      {
         return new Deob<T>(new SqlExecuter(self));
      }

      public static T Connect<T>(this IAdoSession self, T quob)
         where T : IQuob
      {
         return (T) quob.Connect(new SqlExecuter(self));
      }

      public static QueryableQuob<T> AsQueryable<T>(this BaseQuob<T> self)
      {
         return new QueryableQuob<T>((IQuob) self);
      }

      public static IAdoContextInitializer SetSqlFormatter(this IAdoContextInitializer self, ISqlFormatter formatter)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (formatter == null) throw new ArgumentNullException("formatter");
         self.SetItem("quobs.sql.formatter", formatter);
         self.SetUnitOfWorkStatementSeperator(formatter.SqlDialect.StatementSeperator);
         return self;
      }
      public static ISqlFormatter GetSqlFormatter(this AdoContext self, bool throwException = true)
      {
         const string key = "quobs.sql.formatter";
         object formatter;

         if (!self.Items.TryGetValue(key, out formatter))
         {
            if (!throwException)
            {
               return null;
            }
            throw new QuobException(
               "Missing SQL formatter. You need to specify a SQL formatter on your AdoContext initialization (using the initializer parameter), e.g., i => i.SetItem(\"" +
               key + "\",new MySqlFormatter())");
         }

         if (!(formatter is ISqlFormatter))
         {
            throw new QuobException("Invalid SQL formatter: the SQL formmater must implement interface type " +
                                    typeof(ISqlFormatter));
         }

         return formatter.CastTo<ISqlFormatter>();
      }
      public static ISqlFormatter GetSqlFormatter(this IAdoSession self)
      {
         return self.Context.GetSqlFormatter();
      }

   }
}
