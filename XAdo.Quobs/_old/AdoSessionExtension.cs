﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Dialect;
using XAdo.Quobs.SqlObjects;


namespace XAdo.Quobs
{
   public static class AdoSessionExtension
   {
      private class SqlExecuter : ISqlConnection
      {
         private readonly IAdoSession _session;

         public SqlExecuter(IAdoSession session)
         {
            _session = session;
         }

         public T ExecuteScalar<T>(string sql, object args)
         {
            return _session.ExecuteScalar<T>(sql, args);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, object args)
         {
            return _session.Query<T>(sql, args, false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, object args, out int count)
         {
            var reader = _session.QueryMultiple(sql, args);
            count = reader.Read<int>().Single();
            return reader.Read<T>(false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> factory, object args)
         {
            return _session.Query(sql, factory, args, false);
         }

         public IEnumerable<T> ExecuteQuery<T>(string sql, Func<IDataRecord, T> factory,
            object args, out int count)
         {
            var countBinder = new Func<IDataRecord, int>(r => r.GetInt32(0));
            var factories = new Delegate[] {countBinder, factory};
            var result = _session.QueryMultiple(sql, factories, args);
            count = result.Read<int>().Single();
            return result.Read<T>(false);
         }

         public int Execute(string sql, object args)
         {
            return _session.Execute(sql, args);
         }

         public bool HasSqlBatch
         {
            get { return _session.HasSqlBatch; }
         }

         public void AddToSqlBatch(string sql, object args, Action<object> callback)
         {
            _session.AddSqlBatchItem(new BatchItem(sql, args, callback));
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
      public static T From<T>(this IAdoSession self, T quob) where T : IQuob
      {
         return (T)quob.Attach(new SqlExecuter(self));
      }
      public static Upob<T> Update<T>(this IAdoSession self)
      {
         return new Upob<T>(new SqlExecuter(self));
      }
      public static Crob<T> Insert<T>(this IAdoSession self)
      {
         return new Crob<T>(new SqlExecuter(self));
      }
      public static Deob<T> Delete<T>(this IAdoSession self)
      {
         return new Deob<T>(new SqlExecuter(self));
      }

      public static int? Update<T>(this IAdoSession self, T entity)
         where T: class, IDbTable
      {
         return new TableClassPersister<T>(new SqlExecuter(self)).Update(entity);
      }
      public static object Insert<T>(this IAdoSession self, T entity)
         where T : class, IDbTable
      {
         return new TableClassPersister<T>(new SqlExecuter(self)).Insert(entity);
      }
      public static long? Delete<T>(this IAdoSession self, T entity)
         where T : class, IDbTable
      {
         return new TableClassPersister<T>(new SqlExecuter(self)).Delete(entity);
      }

      //public static QueryableQuob<T> AsQueryable<T>(this BaseQuob<T> self)
      //{
      //   return new QueryableQuob<T>((IQuob) self);
      //}

      public static IAdoContextInitializer SetSqlFormatter(this IAdoContextInitializer self, ISqlFormatter formatter)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (formatter == null) throw new ArgumentNullException("formatter");
         self.SetItem("quobs.sql.formatter", formatter);
         self.SetSqlStatementSeperator(formatter.SqlDialect.StatementSeperator);
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