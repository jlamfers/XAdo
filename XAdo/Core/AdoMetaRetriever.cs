using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Runtime.InteropServices;
using XAdo.Core.Interface;

namespace XAdo.Core
{
   public static class AdoMetaRetriever
   {
      private static ConcurrentDictionary<string,IList<AdoColumnMeta>> 
         _cache = new ConcurrentDictionary<string, IList<AdoColumnMeta>>();

      public static IList<AdoColumnMeta> QueryMetaForSql(this IAdoSession self, string sql)
      {
         var providerName = self.Context.ProviderName ?? ConfigurationManager.ConnectionStrings[self.Context.ConnectionStringName].ProviderName;
         var key = providerName + ":" + sql;

         IList<AdoColumnMeta> list;
         if (_cache.TryGetValue(key, out list))
         {
            return list;
         }
         using (var inner = self.Context.CreateSession())
         {
            var cn = (DbConnection)self.CastTo<IAdoConnectionProvider>().Connection;
            var f = DbProviderFactories.GetFactory(providerName);
            cn.Open();
            list = cn.QueryMeta(sql, f,null);
         }
         return _cache.GetOrAdd(key, x => list);
      }

      public static IList<AdoColumnMeta> QueryMetaForTable(this IAdoSession self, string tablename)
      {
         var providerName = self.Context.ProviderName ?? ConfigurationManager.ConnectionStrings[self.Context.ConnectionStringName].ProviderName;
         var sql = "SELECT * FROM " + tablename + " WHERE (1=2)";
         var key = providerName + ":" + sql;

         IList<AdoColumnMeta> list;
         if (_cache.TryGetValue(key, out list))
         {
            return list;
         }

         using (var inner = self.Context.CreateSession())
         {
            var cn = (DbConnection)inner.CastTo<IAdoConnectionProvider>().Connection;
            var f = DbProviderFactories.GetFactory(providerName);
            cn.Open();
            list = cn.QueryMeta(sql, f, tablename);
         }

         return _cache.GetOrAdd(key, x => list);
      }

      private static IList<AdoColumnMeta> QueryMeta(this DbConnection self, string sql, DbProviderFactory f, string tablename)
      {
         using (var command = self.CreateCommand())
         {
            command.CommandText = sql;
            var adapter = f.CreateDataAdapter();
            adapter.SelectCommand = command;
            var ds = new DataSet();
            if (tablename != null)
            {
               adapter.FillSchema(ds, SchemaType.Mapped, tablename);
            }
            else
            {
               adapter.FillSchema(ds, SchemaType.Mapped);
            }

            var resultList = new List<AdoColumnMeta>();

            foreach (DataColumn dc in ds.Tables[0].Columns)
            {
               var ispkey = dc.AutoIncrement || dc.Table.PrimaryKey.Contains(dc);
               resultList.Add(new AdoColumnMeta
               {
                  AllowDBNull = dc.AllowDBNull,
                  AutoIncrement = dc.AutoIncrement,
                  ColumnName = dc.ColumnName,
                  DataType = dc.DataType,
                  DefaultValue = dc.DefaultValue,
                  MaxLength = dc.MaxLength,
                  PKey = ispkey,
                  Unique = dc.Unique
               });
            }

            return resultList.AsReadOnly();
         }
      }

   }
}
