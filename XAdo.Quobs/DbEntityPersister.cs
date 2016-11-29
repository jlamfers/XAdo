using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Dialect;

namespace XAdo.Quobs
{
   public class DbEntityPersister<T>
      where T : class
   {

      private static readonly DbSchemaDescriptor.ColumnDescriptor 
         _identityColumn = typeof(T).GetTableDescriptor().Columns.FirstOrDefault(c => c.IsAutoIncrement);

      private readonly ISqlExecuter _executer;
      private readonly ISqlFormatter _formatter;

      private static string 
         _sqlUpdate,
         _sqlDelete,
        _sqlInsert;

      private static Type _t = typeof (T);

      public DbEntityPersister(ISqlExecuter executer)
      {
         _executer = executer;
         _formatter = _executer.GetSqlFormatter();
      }

      public int? Update(T entity)
      {
         if (entity == null) throw new ArgumentNullException("entity");
         if (!_executer.HasSqlQueue) return _executer.Execute(SqlUpdate, entity);
         _executer.EnqueueSql(SqlUpdate, entity);
         return null;
      }
      public int? Delete(T entity)
      {
         if (entity == null) throw new ArgumentNullException("entity");
         if (!_executer.HasSqlQueue) return _executer.Execute(SqlDelete, entity);
         _executer.EnqueueSql(SqlDelete, entity);
         return null;
      }
      public long? Insert(T entity)
      {
         if (entity == null) throw new ArgumentNullException("entity");

         var hasIdentityReturn = _identityColumn != null && !_executer.HasSqlQueue && !string.IsNullOrEmpty(_formatter.SqlDialect.SelectLastIdentity);
         if (hasIdentityReturn)
         {
            var sql = SqlInsert + _formatter.SqlDialect.StatementSeperator + Environment.NewLine +
                      _formatter.SqlDialect.SelectLastIdentity;
            var id = _executer.ExecuteScalar<long>(sql, entity);
            _identityColumn.Member.SetValue(entity,id);
            return id;
         }

         if (!_executer.HasSqlQueue) return _executer.Execute(SqlInsert, entity);
         _executer.EnqueueSql(SqlInsert, entity);
         return null;
      }


      protected virtual string SqlUpdate
      {
         get
         {
            if (_sqlUpdate != null)
            {
               return _sqlUpdate;
            }

            using (var sw = new StringWriter())
            {
               sw.Write("UPDATE ");
               _t.GetTableDescriptor().Format(sw,_formatter);
               sw.Write(" SET");
               var comma = "";
               foreach (var d in _t.GetTableDescriptor().Columns)
               {
                  if (d.IsPKey) continue;
                  sw.WriteLine(comma);
                  sw.Write("   ");
                  d.Format(sw, _formatter);
                  sw.Write(" = ");
                  _formatter.FormatParameter(sw, d.Member.Name);
                  comma = ",";
               }
               sw.WriteLine();
               sw.Write("WHERE ");
               comma = "";
               foreach (var d in _t.GetTableDescriptor().Columns.Where(x => x.IsPKey))
               {
                  sw.Write(comma);
                  d.Format(sw, _formatter);
                  sw.Write(" = ");
                  _formatter.FormatParameter(sw, d.Member.Name);
                  comma = " AND ";
               }
               _sqlUpdate = sw.GetStringBuilder().ToString();
            }

            return _sqlUpdate;
         }
      }
      protected virtual string SqlDelete
      {
         get
         {
            if (_sqlDelete != null)
            {
               return _sqlDelete;
            }

            using (var sw = new StringWriter())
            {
               sw.Write("DELETE WHERE ");
               var comma = "";
               foreach (var d in _t.GetTableDescriptor().Columns.Where(x => x.IsPKey))
               {
                  sw.Write(comma);
                  d.Format(sw, _formatter);
                  sw.Write(" = ");
                  _formatter.FormatParameter(sw, d.Member.Name);
                  comma = " AND ";
               }
               _sqlDelete = sw.GetStringBuilder().ToString();
            }

            return _sqlDelete;
         }
      }
      protected virtual string SqlInsert
      {
         get
         {
            if (_sqlInsert != null)
            {
               return _sqlInsert;
            }

            using (var sw = new StringWriter())
            {
               sw.Write("INSERT INTO ");
               _t.GetTableDescriptor().Format(sw, _formatter);
               sw.Write("( ");
               var comma = "";
               foreach (var d in _t.GetTableDescriptor().Columns)
               {
                  if (d.IsAutoIncrement) continue;
                  sw.WriteLine(comma);
                  sw.Write("   ");
                  d.Format(sw, _formatter);
                  comma = ",";
               }
               sw.WriteLine(")");
               sw.Write("VALUES (");
               comma = "";
               foreach (var d in _t.GetTableDescriptor().Columns)
               {
                  if (d.IsAutoIncrement) continue;
                  sw.WriteLine(comma);
                  sw.Write("   ");
                  _formatter.FormatParameter(sw, d.Member.Name);
                  comma = ",";
               }
               sw.WriteLine(")");
               _sqlInsert = sw.GetStringBuilder().ToString();
            }

            return _sqlInsert;
         }
      }
   }
}
