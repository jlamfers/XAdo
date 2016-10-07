using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace XAdo.Quobs.Schema
{
   public class DbSchemaReader
   {
      public class InformationSchemaTable
      {
         public string TABLE_TYPE { get; set; }
         public string TABLE_SCHEMA { get; set; }
         public string TABLE_NAME { get; set; }
      }

      public class InformationSchemaFKey
      {

         public string FK_CONSTRAINT_NAME { get; set; }
         public string FK_TABLE_SCHEMA { get; set; }
         public string FK_TABLE_NAME { get; set; }
         public string FK_COLUMN_NAME { get; set; }
         public string REF_CONSTRAINT_NAME { get; set; }
         public string REF_TABLE_SCHEMA { get; set; }
         public string REF_TABLE_NAME { get; set; }
         public string REF_COLUMN_NAME { get; set; }
      }

      public virtual DbSchema Read(string connectionString, string providerInvariantName)
      {
         var f = DbProviderFactories.GetFactory(providerInvariantName);

         using (var cn = f.CreateConnection())
         {
            cn.ConnectionString = connectionString;
            cn.Open();
            return BuildDbSchema(cn, f);
         }
      }

      protected virtual string FormatTableName(string schema, string name)
      {
         return string.Format("\"{0}\".\"{1}\"", schema, name);
      }

      protected virtual DbSchema BuildDbSchema(DbConnection cn, DbProviderFactory f)
      {
         var db = new DbSchema(cn.Database);

         var tables = ReadInformationSchemaTables(cn);
         var fkeys = ReadInformationSchemaFKeys(cn);

         foreach (var table in tables)
         {
            BuildDbTableSchema(cn, f, db, table);
         }

         foreach (var fkey in fkeys)
         {
            db.FKeys.Add(new FKeySchemaItem(db, fkey.FK_CONSTRAINT_NAME, fkey.FK_TABLE_SCHEMA, fkey.FK_TABLE_NAME, fkey.FK_COLUMN_NAME, fkey.REF_CONSTRAINT_NAME, fkey.REF_TABLE_SCHEMA, fkey.REF_TABLE_NAME, fkey.REF_COLUMN_NAME));
         }

         return db.AsReadOnly();

      }

      protected virtual IList<InformationSchemaTable> ReadInformationSchemaTables(DbConnection cn)
      {
         using (var c = cn.CreateCommand())
         {
            var tables = new List<InformationSchemaTable>();
            c.CommandText = AllTablesSql;
            using (var r = c.ExecuteReader())
            {
               while (r.Read())
               {
                  tables.Add(new InformationSchemaTable
                  {
                     TABLE_TYPE = r.GetString(0),
                     TABLE_SCHEMA = r.GetString(1),
                     TABLE_NAME = r.GetString(2)
                  });
               }
            }
            return tables;
         }

      }
      protected virtual IList<InformationSchemaFKey> ReadInformationSchemaFKeys(DbConnection cn)
      {
         using (var c = cn.CreateCommand())
         {
            var fkeys = new List<InformationSchemaFKey>();
            c.CommandText = AllFKeysSql;
            using (var r = c.ExecuteReader())
            {
               while (r.Read())
               {
                  fkeys.Add(new InformationSchemaFKey
                  {
                     FK_CONSTRAINT_NAME = r.GetString(0),
                     FK_TABLE_SCHEMA = r.GetString(1),
                     FK_TABLE_NAME = r.GetString(2),
                     FK_COLUMN_NAME = r.GetString(3),
                     REF_CONSTRAINT_NAME = r.GetString(4),
                     REF_TABLE_SCHEMA = r.GetString(5),
                     REF_TABLE_NAME = r.GetString(6),
                     REF_COLUMN_NAME = r.GetString(7)
                  });
               }
            }
            return fkeys;
         }
      }

      protected virtual void BuildDbTableSchema(DbConnection cn, DbProviderFactory f, DbSchema schema, InformationSchemaTable table)
      {
         var tablename = FormatTableName(table.TABLE_SCHEMA, table.TABLE_NAME);
         var sql = "SELECT * FROM " + tablename + " WHERE 1 = 2";
         using (var command = cn.CreateCommand())
         {
            command.CommandText = sql;
            var adapter = f.CreateDataAdapter();
            adapter.SelectCommand = command;
            var ds = new DataSet();
            adapter.FillSchema(ds, SchemaType.Mapped, tablename);

            var dbTable = new TableSchemaItem(schema, table.TABLE_SCHEMA, table.TABLE_NAME, table.TABLE_TYPE == "VIEW");
            schema.Tables.Add(dbTable);

            foreach (DataColumn dataColumn in ds.Tables[0].Columns)
            {
               var ispkey = dataColumn.AutoIncrement || dataColumn.Table.PrimaryKey.Contains(dataColumn);
               schema.Columns.Add(new ColumnSchemaItem(schema, table.TABLE_NAME, table.TABLE_SCHEMA, dataColumn.ColumnName,dataColumn.DataType, ispkey, dataColumn.AutoIncrement, dataColumn.AllowDBNull, dataColumn.Unique,dataColumn.DefaultValue, dataColumn.MaxLength));
            }

         }
      }
      protected virtual string AllFKeysSql
      {
         get
         {
            return @"
SELECT  
     KCU1.CONSTRAINT_NAME AS FK_CONSTRAINT_NAME 
    ,KCU1.TABLE_SCHEMA AS FK_TABLE_SCHEMA 
    ,KCU1.TABLE_NAME AS FK_TABLE_NAME 
    ,KCU1.COLUMN_NAME AS FK_COLUMN_NAME 
    ,KCU2.CONSTRAINT_NAME AS REF_CONSTRAINT_NAME 
    ,KCU2.TABLE_SCHEMA AS REF_TABLE_SCHEMA 
    ,KCU2.TABLE_NAME AS REF_TABLE_NAME 
    ,KCU2.COLUMN_NAME AS REF_COLUMN_NAME 
FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS AS RC 

INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU1 
    ON KCU1.CONSTRAINT_CATALOG = RC.CONSTRAINT_CATALOG  
    AND KCU1.CONSTRAINT_SCHEMA = RC.CONSTRAINT_SCHEMA 
    AND KCU1.CONSTRAINT_NAME = RC.CONSTRAINT_NAME 

INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS KCU2 
    ON KCU2.CONSTRAINT_CATALOG = RC.UNIQUE_CONSTRAINT_CATALOG  
    AND KCU2.CONSTRAINT_SCHEMA = RC.UNIQUE_CONSTRAINT_SCHEMA 
    AND KCU2.CONSTRAINT_NAME = RC.UNIQUE_CONSTRAINT_NAME 
    AND KCU2.ORDINAL_POSITION = KCU1.ORDINAL_POSITION ";
         }
      }

      protected virtual string AllTablesSql
      {
         get
         {
            return @"
SELECT 
    TABLE_TYPE, TABLE_SCHEMA, TABLE_NAME 
    FROM INFORMATION_SCHEMA.TABLES

UNION 

SELECT 
    'VIEW', TABLE_SCHEMA, TABLE_NAME
    FROM INFORMATION_SCHEMA.VIEWS

ORDER BY 
    TABLE_TYPE, TABLE_SCHEMA, TABLE_NAME";
         }
      }

   }
}
