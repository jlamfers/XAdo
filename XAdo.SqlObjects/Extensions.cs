using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
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
         private readonly IXAdoDbSession _session;

         public XAdoConnection(IXAdoDbSession session)
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
            _session.AddSqlBatchItem(new XAdoSqlBatchItem(sql, args, callback));
         }

         public ISqlFormatter GetSqlFormatter()
         {
            return _session.GetSqlFormatter();
         }

         #region Async

         public async Task<T> ExecuteScalarAsync<T>(string sql, object args)
         {
            return await _session.ExecuteScalarAsync<T>(sql, args);
         }

         public async Task<List<T>> ExecuteQueryAsync<T>(string sql, object args)
         {
            return await _session.QueryAsync<T>(sql, args);
         }

         public async Task<List<T>> ExecuteQueryAsync<T>(string sql, Func<IDataRecord, T> binder, object args)
         {
            return await _session.QueryAsync<T>(sql, binder, args);
         }

         public async Task<AsyncPagedResult<T>> ExecutePagedQueryAsync<T>(string sql, object args)
         {
            var reader = await _session.QueryMultipleAsync(sql, args);
            var count = (await reader.ReadAsync<int>()).Single();
            var collection = await reader.ReadAsync<T>();
            return new AsyncPagedResult<T>
            {
               Collection = collection,
               TotalCount = count
            };
         }

         public async Task<AsyncPagedResult<T>> ExecutePagedQueryAsync<T>(string sql, Func<IDataRecord, T> binder, object args)
         {
            var countBinder = new Func<IDataRecord, int>(r => r.GetInt32(0));
            var factories = new Delegate[] { countBinder, binder };
            var result = await _session.QueryMultipleAsync(sql, factories, args);
            var count = (await result.ReadAsync<int>()).Single();
            var collection = await result.ReadAsync<T>();
            return new AsyncPagedResult<T>
            {
               Collection = collection,
               TotalCount = count
            };
         }

         public async Task<int> ExecuteAsync(string sql, object args)
         {
            return await _session.ExecuteAsync(sql, args);
         }
         #endregion
      }

      public static QuerySqlObject<TTable> From<TTable>(this IXAdoDbSession self) where TTable : IDbTable
      {
         return self
            .Context
            .GetInstance<ISqlObjectFactory>()
            .CreateReadSqlObject<TTable>(new XAdoConnection(self));
      }
      public static TMappedSqlObject Attach<TMappedSqlObject>(this IXAdoDbSession self, TMappedSqlObject mappedSqlObject) where TMappedSqlObject : IMappedSqlObject
      {
         return (TMappedSqlObject)mappedSqlObject.Attach(new XAdoConnection(self));
      }
      public static UpdateSqlObject<TTable> Update<TTable>(this IXAdoDbSession self) where TTable : IDbTable
      {
         return self
            .Context
            .GetInstance<ISqlObjectFactory>()
            .CreateUpdateSqlObject<TTable>(new XAdoConnection(self));
      }
      public static CreateSqlObject<TTable> Insert<TTable>(this IXAdoDbSession self) where TTable : IDbTable
      {
         return self
            .Context
            .GetInstance<ISqlObjectFactory>()
            .CreateCreateSqlObject<TTable>(new XAdoConnection(self));
      }
      public static DeleteSqlObject<TTable> Delete<TTable>(this IXAdoDbSession self) where TTable : IDbTable
      {
         return self
            .Context
            .GetInstance<ISqlObjectFactory>()
            .CreateDeleteSqlObject<TTable>(new XAdoConnection(self));
      }

      public static int? Update<TTable>(this IXAdoDbSession self, TTable entity, Action<object> callback = null) where TTable : IDbTable
      {
         return self
                    .Context
                    .GetInstance<ISqlObjectFactory>()
                    .CreateTablePersister<TTable>(new XAdoConnection(self))
                    .Update(entity, callback);
      }
      public static object Insert<TTable>(this IXAdoDbSession self, TTable entity, Action<object> callback = null) where TTable : IDbTable
      {
         return self
                    .Context
                    .GetInstance<ISqlObjectFactory>()
                    .CreateTablePersister<TTable>(new XAdoConnection(self))
                    .Insert(entity,callback);
      }
      public static int? Delete<TTable>(this IXAdoDbSession self, TTable entity, Action<object> callback = null) where TTable : IDbTable
      {
         return self
                    .Context
                    .GetInstance<ISqlObjectFactory>()
                    .CreateTablePersister<TTable>(new XAdoConnection(self))
                    .Delete(entity, callback);
      }

      const string FormatterKey = "quobs.sql.formatter";

      public static IXAdoContextInitializer SetSqlFormatter(this IXAdoContextInitializer self, ISqlFormatter formatter)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (formatter == null) throw new ArgumentNullException("formatter");
         self.SetItem(FormatterKey, formatter);
         return self;
      }
      public static ISqlFormatter GetSqlFormatter(this XAdoDbContext self, bool throwException = true)
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
      public static ISqlFormatter GetSqlFormatter(this IXAdoDbSession self)
      {
         return self.Context.GetSqlFormatter();
      }

   }
}
