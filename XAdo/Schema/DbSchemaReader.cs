using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace XAdo.Schema
{
   public class DbSchemaReader
   {
      public class SchemaTable
      {
         public string TABLE_TYPE { get; set; }
         public string TABLE_SCHEMA { get; set; }
         public string TABLE_NAME { get; set; }
      }

      public class SchemaFKey
      {
         public string FK_TABLE_SCHEMA { get; set; }
         public string FK_TABLE_NAME { get; set; }
         public string FK_COLUMN_NAME { get; set; }
         public string REF_TABLE_SCHEMA { get; set; }
         public string REF_TABLE_NAME { get; set; }
         public string REF_COLUMN_NAME { get; set; }
      }

      public virtual DbDatabase Read(string connectionString, string providerInvariantName)
      {
         IEnumerable<SchemaTable> tables;
         IEnumerable<SchemaFKey> fkeys;

         var context = new AdoContext(i => i.SetConnectionString(connectionString, providerInvariantName));
         using (var session = context.CreateSession())
         {
            tables = session.Query<SchemaTable>(AllTablesSql);
            fkeys = session.Query<SchemaFKey>(AllFKeysSql);
         }

         

         var f = DbProviderFactories.GetFactory(providerInvariantName);

         using (var cn = f.CreateConnection())
         {
            cn.ConnectionString = connectionString;
            cn.Open();
            var db = new DbDatabase(cn.Database);
            foreach (var table in tables)
            {
               var tablename = FormatTableName(table.TABLE_SCHEMA, table.TABLE_NAME);
               var sql = "SELECT * FROM " + tablename + " WHERE 1 = 2";
               using (var command = f.CreateCommand())
               {
                  command.Connection = cn;
                  command.CommandText = sql;
                  var adapter = f.CreateDataAdapter();
                  adapter.SelectCommand = command;
                  var ds = new DataSet();
                  adapter.FillSchema(ds, SchemaType.Mapped, tablename);

                  var dbTable = new DbTable(db,table.TABLE_SCHEMA,table.TABLE_NAME,table.TABLE_TYPE=="VIEW");

                  foreach (DataColumn dataColumn in ds.Tables[0].Columns)
                  {
                     var ispkey = dataColumn.AutoIncrement || dataColumn.Table.PrimaryKey.Contains(dataColumn);
                     db.Columns.Add(new DbColumn(db, table.TABLE_NAME, table.TABLE_SCHEMA, dataColumn.ColumnName, dataColumn.DataType, ispkey, dataColumn.AutoIncrement,dataColumn.AllowDBNull,dataColumn.Unique,dataColumn.DefaultValue,dataColumn.MaxLength));
                  }

                  db.Tables.Add(dbTable);
               }
            }
            foreach (var fkey in fkeys)
            {
               db.FKeys.Add(new DbFKey(db,fkey.FK_TABLE_SCHEMA,fkey.FK_TABLE_NAME,fkey.REF_TABLE_SCHEMA,fkey.REF_TABLE_NAME,fkey.FK_COLUMN_NAME,fkey.REF_COLUMN_NAME));
            }
            return db.AsReadOnly();
         }
      }

      protected virtual string FormatTableName(string schema, string name)
      {
         return string.Format("\"{0}\".\"{1}\"", schema, name);
      }

      protected virtual string AllFKeysSql
      {
         get
         {
            return @"
SELECT  
     KCU1.TABLE_SCHEMA AS FK_TABLE_SCHEMA 
    ,KCU1.TABLE_NAME AS FK_TABLE_NAME 
    ,KCU1.COLUMN_NAME AS FK_COLUMN_NAME 
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
    TABLE_SCHEMA,TABLE_TYPE,TABLE_NAME 
    FROM INFORMATION_SCHEMA.TABLES

UNION 

SELECT 
    TABLE_SCHEMA,'VIEW' ,TABLE_NAME
    FROM INFORMATION_SCHEMA.VIEWS

ORDER BY 
    TABLE_TYPE, TABLE_SCHEMA, TABLE_NAME";
         }
      }
   }
}
