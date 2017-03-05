using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Core
{
   public static class XAdoMetaRetriever
   {
      private static readonly LRUCache<string, IList<XAdoColumnMeta>>
         Cache = new LRUCache<string, IList<XAdoColumnMeta>>("LRUCache.XAdo.Meta.Size",2000);

      public static IList<XAdoColumnMeta> QueryMetaForSql(this IXAdoDbSession self, string sql)
      {
         var key = self.Context.GetHashCode() + ":" + sql;

         IList<XAdoColumnMeta> list;
         if (Cache.TryGetValue(key, out list))
         {
            return list;
         }
         var cp = self.CastTo<IXAdoConnectionProvider>();
         var cn = cp.Connection.CastTo<DbConnection>();
         var tr = cp.Transaction.CastTo<DbTransaction>();
         var f = DbProviderFactories.GetFactory(self.Context.ProviderName);
         if (cn.State != ConnectionState.Open)
         {
            cn.Open();
         }
         list = cn.QueryMeta(tr,sql, f, null);
         return Cache.GetOrAdd(key, x => list);
      }

      public static IList<XAdoColumnMeta> QueryMetaForTable(this IXAdoDbSession self, string tablename)
      {
         var sql = "SELECT * FROM " + tablename + " WHERE (1=2)";
         var key = self.Context.GetHashCode() + ":" + sql;

         IList<XAdoColumnMeta> list;
         if (Cache.TryGetValue(key, out list))
         {
            return list;
         }

         var cp = self.CastTo<IXAdoConnectionProvider>();
         var cn = cp.Connection.CastTo<DbConnection>();
         var tr = cp.Transaction.CastTo<DbTransaction>();
         var f = DbProviderFactories.GetFactory(self.Context.ProviderName);
         if (cn.State != ConnectionState.Open)
         {
            cn.Open();
         }
         list = cn.QueryMeta(tr,sql, f, tablename);

         return Cache.GetOrAdd(key, x => list);
      }

      private static IList<XAdoColumnMeta> QueryMeta(this DbConnection self, DbTransaction tr, string sql, DbProviderFactory f, string tablename)
      {
         using (var command = self.CreateCommand())
         {
            if (tr != null)
            {
               command.Transaction = tr;
            }
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

            var resultList = new List<XAdoColumnMeta>();

            foreach (DataColumn dc in ds.Tables[0].Columns)
            {
               var ispkey = dc.AutoIncrement || dc.Table.PrimaryKey.Contains(dc);
               resultList.Add(new XAdoColumnMeta
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
