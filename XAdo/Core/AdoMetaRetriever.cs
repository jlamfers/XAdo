using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using XAdo.Core.Cache;
using XAdo.Core.Interface;

namespace XAdo.Core
{
   public static class AdoMetaRetriever
   {
      private static readonly LRUCache<string, IList<AdoColumnMeta>>
         Cache = new LRUCache<string, IList<AdoColumnMeta>>("LRUCache.XAdo.Meta.Size",2000);

      public static IList<AdoColumnMeta> QueryMetaForSql(this IAdoSession self, string sql)
      {
         var key = self.Context.GetHashCode() + ":" + sql;

         IList<AdoColumnMeta> list;
         if (Cache.TryGetValue(key, out list))
         {
            return list;
         }
         var cn = (DbConnection)self.CastTo<IAdoConnectionProvider>().Connection;
         var f = DbProviderFactories.GetFactory(self.Context.ProviderName);
         if (cn.State != ConnectionState.Open)
         {
            cn.Open();
         }
         list = cn.QueryMeta(sql, f,null);
         return Cache.GetOrAdd(key, x => list);
      }

      public static IList<AdoColumnMeta> QueryMetaForTable(this IAdoSession self, string tablename)
      {
         var sql = "SELECT * FROM " + tablename + " WHERE (1=2)";
         var key = self.Context.GetHashCode() + ":" + sql;

         IList<AdoColumnMeta> list;
         if (Cache.TryGetValue(key, out list))
         {
            return list;
         }

         var cn = (DbConnection)self.CastTo<IAdoConnectionProvider>().Connection;
         var f = DbProviderFactories.GetFactory(self.Context.ProviderName);
         if (cn.State != ConnectionState.Open)
         {
            cn.Open();
         }
         list = cn.QueryMeta(sql, f, tablename);

         return Cache.GetOrAdd(key, x => list);
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
                  Unique = dc.Unique,
                  ReadOnly = dc.ReadOnly
               });
            }

            return resultList.AsReadOnly();
         }
      }


   }
}
