using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.Dialects.Core;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects.Core
{
   public abstract class WriteSqlObject<TTable> : IWriteFromSqlObject<TTable>
      where TTable : IDbTable
   {

      protected static readonly DbSchemaDescriptor.ColumnDescriptor 
         IdentityColumn = typeof (TTable).GetTableDescriptor().Columns.FirstOrDefault(c => c.IsAutoIncrement);


      protected ISqlFormatter Formatter { get; private set; }
      protected ISqlConnection Connection { get; private set; }
      protected SetExpressionCompiler.CompileResult CompileResult { get; private set; }
      protected LambdaExpression Expression { get; private set; }
      protected bool HasIdentityReturn { get; private set; }

      public virtual IWriteFromSqlObject<TTable> From(Expression<Func<TTable>> expression)
      {
         Expression = expression;
         return this;
      }

      protected WriteSqlObject(ISqlConnection connection)
      {
         Formatter = connection.GetSqlFormatter();
         Connection = connection;
      }


      public virtual void Apply(bool literals = false, Action<object> callback = null)
      {
         if (Expression == null) return;

         var compiler = new SetExpressionCompiler(Formatter);
         CompileResult = compiler.Compile(Expression, literals);
         Expression = null;

         HasIdentityReturn = IdentityColumn != null && !string.IsNullOrEmpty(Formatter.SqlDialect.SelectLastIdentity) && callback != null;

         var sql = GetSql();
         var args = GetArguments();

         if (!Connection.HasSqlBatch)
         {
            var result = HasIdentityReturn ? Connection.ExecuteScalar<object>(sql, args) : Connection.Execute(sql, args);
            if (callback != null)
            {
               callback(result);
            }
         }
         else
         {
            Connection.AddToSqlBatch(sql, args, callback);
         }

         CompileResult = null;
      }

      protected abstract void WriteSql(TextWriter writer);

      protected virtual IDictionary<string, object> GetArguments()
      {
         return CompileResult != null 
               ? CompileResult.Arguments
               : new Dictionary<string, object>(); 
      }

      void ISqlObject.WriteSql(TextWriter writer)
      {
         WriteSql(writer);
      }

      object ISqlObject.GetArguments()
      {
         return GetArguments();
      }

      protected virtual string GetSql()
      {
         using (var sw = new StringWriter())
         {
            WriteSql(sw);
            return sw.GetStringBuilder().ToString();
         }
      }
   }
}
