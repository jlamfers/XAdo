﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Sql;
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

         public void Execute(string sql, IDictionary<string, object> args)
         {
            _session.Execute(sql, args);
         }
      }

      public static Quob<T> From<T>(this IAdoSession self)
      {
         const string key = "quobs.sql.formatter";
         object formatter;

         if (!self.Context.Items.TryGetValue(key, out formatter))
         {
            throw new QuobException(
               "Missing SQL formatter. You need to specify a SQL formatter on your AdoContext initialization (using the initializer parameter), e.g., i => i.SetItem(\"" +
               key + "\",new MySqlFormatter())");
         }

         if (!(formatter is ISqlFormatter))
         {
            throw new QuobException("Invalid SQL formatter: the SQL formmater must implement interface type " +
                                    typeof (ISqlFormatter));
         }

         return new Quob<T>(formatter.CastTo<ISqlFormatter>(), new SqlExecuter(self));
      }
      public static Upob<T> Update<T>(this IAdoSession self, bool argumentsAsLiterals = false)
      {
         const string key = "quobs.sql.formatter";
         object formatter;

         if (!self.Context.Items.TryGetValue(key, out formatter))
         {
            throw new QuobException(
               "Missing SQL formatter. You need to specify a SQL formatter on your AdoContext initialization (using the initializer parameter), e.g., i => i.SetItem(\"" +
               key + "\",new MySqlFormatter())");
         }

         if (!(formatter is ISqlFormatter))
         {
            throw new QuobException("Invalid SQL formatter: the SQL formmater must implement interface type " +
                                    typeof(ISqlFormatter));
         }

         return new Upob<T>(formatter.CastTo<ISqlFormatter>(), new SqlExecuter(self), argumentsAsLiterals);
      }

      public static int UpdateFrom<T>(this IAdoSession self, Expression<Func<T>> expression)
      {
         return -1;
      }
      public static object InsertFrom<T>(this IAdoSession self, Expression<Func<T>> expression)
      {
         return -1;
      }
      public static object DeleteFrom<T>(this IAdoSession self, Expression<Func<T>> expression)
      {
         return -1;
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
         self.SetItem("quobs.sql.formatter", formatter);
         return self;
      }
      public static ISqlFormatter GetSqlFormatter(this AdoContext self)
      {
         return self.Items["quobs.sql.formatter"].CastTo<ISqlFormatter>();
      }
      public static ISqlFormatter GetSqlFormatter(this IAdoSession self)
      {
         return self.Context.GetSqlFormatter();
      }

   }
}
