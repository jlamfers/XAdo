using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XAdo.Core;
using XAdo.Core.Interface;
using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.Dialects;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlObjects;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects
{
   public static class Extensions
   {
      private class XAdoConnection : ISqlConnection
      {
         private readonly IAdoSession _session;

         public XAdoConnection(IAdoSession session)
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
            _session.AddSqlBatchItem(new AdoSqlBatchItem(sql, args, callback));
         }

         public ISqlFormatter GetSqlFormatter()
         {
            return _session.GetSqlFormatter();
         }
      }

      public static QuerySqlObject<TTable> From<TTable>(this IAdoSession self) where TTable : IDbTable
      {
         return self
            .Context
            .GetInstance<ISqlObjectFactory>()
            .CreateReadSqlObject<TTable>(new XAdoConnection(self));
      }
      public static TMappedSqlObject From<TMappedSqlObject>(this IAdoSession self, TMappedSqlObject mappedSqlObject) where TMappedSqlObject : IMappedSqlObject
      {
         return (TMappedSqlObject)mappedSqlObject.Attach(new XAdoConnection(self));
      }
      public static UpdateSqlObject<TTable> Update<TTable>(this IAdoSession self) where TTable : IDbTable
      {
         return self
            .Context
            .GetInstance<ISqlObjectFactory>()
            .CreateUpdateSqlObject<TTable>(new XAdoConnection(self));
      }
      public static CreateSqlObject<TTable> Insert<TTable>(this IAdoSession self) where TTable : IDbTable
      {
         return self
            .Context
            .GetInstance<ISqlObjectFactory>()
            .CreateCreateSqlObject<TTable>(new XAdoConnection(self));
      }
      public static DeleteSqlObject<TTable> Delete<TTable>(this IAdoSession self) where TTable : IDbTable
      {
         return self
            .Context
            .GetInstance<ISqlObjectFactory>()
            .CreateDeleteSqlObject<TTable>(new XAdoConnection(self));
      }

      public static int? Update<TTable>(this IAdoSession self, TTable entity, Action<object> callback = null) where TTable : IDbTable
      {
         return self
                    .Context
                    .GetInstance<ISqlObjectFactory>()
                    .CreateTablePersister<TTable>(new XAdoConnection(self))
                    .Update(entity, callback);
      }
      public static object Insert<TTable>(this IAdoSession self, TTable entity, Action<object> callback = null) where TTable : IDbTable
      {
         return self
                    .Context
                    .GetInstance<ISqlObjectFactory>()
                    .CreateTablePersister<TTable>(new XAdoConnection(self))
                    .Insert(entity,callback);
      }
      public static int? Delete<TTable>(this IAdoSession self, TTable entity, Action<object> callback = null) where TTable : IDbTable
      {
         return self
                    .Context
                    .GetInstance<ISqlObjectFactory>()
                    .CreateTablePersister<TTable>(new XAdoConnection(self))
                    .Delete(entity, callback);
      }

      const string FormatterKey = "quobs.sql.formatter";

      public static IAdoContextInitializer SetSqlFormatter(this IAdoContextInitializer self, ISqlFormatter formatter)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (formatter == null) throw new ArgumentNullException("formatter");
         self.SetItem(FormatterKey, formatter);
         self.SetSqlStatementSeperator(formatter.SqlDialect.StatementSeperator);
         return self;
      }
      public static ISqlFormatter GetSqlFormatter(this AdoContext self, bool throwException = true)
      {
         object formatter;

         if (!self.Items.TryGetValue(FormatterKey, out formatter))
         {
            if (!throwException)
            {
               return null;
            }
            throw new SqlObjectsException(
               "Missing SQL formatter. You need to specify a SQL formatter on your AdoContext initialization (using the initializer parameter), e.g., i => i.SetItem(\"" +
               FormatterKey + "\",new MySqlFormatter())");
         }

         if (!(formatter is ISqlFormatter))
         {
            throw new SqlObjectsException("Invalid SQL formatter: the SQL formmater must implement interface type " +
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
