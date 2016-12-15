using System;
using System.IO;
using System.Linq;
using XAdo.Core;
using XAdo.Quobs.Core;
using XAdo.Quobs.DbSchema;
using XAdo.Quobs.DbSchema.Attributes;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.SqlObjects.Interface;
using ISqlFormatter = XAdo.Quobs.Dialects.ISqlFormatter;

namespace XAdo.Quobs.SqlObjects
{
   public class TablePersister<TTable> : ITablePersister<TTable> where TTable : IDbTable
   {

      private static readonly DbSchemaDescriptor.ColumnDescriptor 
         _identityColumn = typeof(TTable).GetTableDescriptor().Columns.FirstOrDefault(c => c.IsAutoIncrement);

      private readonly ISqlConnection _executer;
      private readonly ISqlFormatter _formatter;

      private static string 
         _sqlUpdate,
         _sqlDelete,
        _sqlInsert,
        _sqlSelectIdentity;

      private static Type _t = typeof (TTable);

      public TablePersister(ISqlConnection executer)
      {
         _executer = executer;
         _formatter = _executer.GetSqlFormatter();
      }

      public virtual int? Update(TTable entity, Action<object> callback = null)
      {
         if (entity == null) throw new ArgumentNullException("entity");
         int? result = null;
         if (!_executer.HasSqlBatch)
         {
            result = _executer.Execute(SqlUpdate, entity);
            if (callback != null)
            {
               callback(result);
            }
         }
         else
         {
            _executer.AddToSqlBatch(SqlUpdate, entity, callback);
         }
         return result;
      }
      public virtual int? Delete(TTable entity, Action<object> callback = null)
      {
         if (entity == null) throw new ArgumentNullException("entity");
         int? result = null;
         if (!_executer.HasSqlBatch)
         {
            result = _executer.Execute(SqlDelete, entity);
            if (callback != null)
            {
               callback(result);
            }
         }
         else
         {
            _executer.AddToSqlBatch(SqlDelete, entity, callback);
         }
         return result;
      }
      public virtual object Insert(TTable entity, Action<object> callback = null)
      {
         if (entity == null) throw new ArgumentNullException("entity");

         var hasIdentityReturn = _identityColumn != null && !string.IsNullOrEmpty(_formatter.SqlDialect.SelectLastIdentity);
         if (hasIdentityReturn)
         {
            var sql = SqlInsert + _formatter.SqlDialect.StatementSeperator + Environment.NewLine + SqlSelectIdentity;

            if (!_executer.HasSqlBatch)
            {
               var id = _executer.ExecuteScalar<object>(sql, entity);
               _identityColumn.Member.SetValue(entity, id);
               if (callback != null)
               {
                  callback(id);
               }
               return id;
            }
            if (callback != null)
            {
               _executer.AddToSqlBatch(SqlInsert, entity, id =>
               {
                  _identityColumn.Member.SetValue(entity, id);
                  callback(id);
               });
            }
            else
            {
               _executer.AddToSqlBatch(SqlInsert, entity, id => _identityColumn.Member.SetValue(entity, id));
            }
            return null;
         }

         if (!_executer.HasSqlBatch)
         {
            return _executer.Execute(SqlInsert, entity);
         }
         _executer.AddToSqlBatch(SqlInsert, entity, callback);
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

      protected virtual string SqlSelectIdentity
      {
         get
         {
            if (_identityColumn == null || _sqlSelectIdentity != null)
            {
               return _sqlSelectIdentity;
            }
            using (var sw = new StringWriter())
            {
               _formatter.WriteSelectLastIdentity(sw,_identityColumn.Member.GetMemberType());
               _sqlSelectIdentity = sw.GetStringBuilder().ToString();
               return _sqlSelectIdentity;
            }
         }
      }
   }
}
