using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Dialect;

namespace XAdo.Quobs
{
   public class Crob<T> : ISqlBuilder
   {

      private static DbSchemaDescriptor.ColumnDescriptor _identityColumn = typeof (T).GetTableDescriptor().Columns.FirstOrDefault(c => c.IsAutoIncrement);


      private readonly ISqlFormatter _formatter;
      private readonly ISqlExecuter _executer;
      private bool _argumentsAsLiterls;
      private SetExpressionCompiler.CompileResult _compileResult;

      public Crob(ISqlExecuter executer)
      {
         _formatter = executer.GetSqlFormatter();
         _executer = executer;
      }

      public virtual Crob<T> WithArgumentsAsLiterals()
      {
         _argumentsAsLiterls = true;
         return this;
      }

      public virtual Crob<T> From(Expression<Func<T>> expression)
      {
         var compiler = new SetExpressionCompiler(_formatter);
         _compileResult =  compiler.Compile(expression,_argumentsAsLiterls);
         return this;
      }

      private bool _hasIdentityReturn;
      public virtual object Apply()
      {
         if (_compileResult == null) return -1L;

         _hasIdentityReturn = _identityColumn != null && !string.IsNullOrEmpty(_formatter.SqlDialect.SelectLastIdentity) && !_executer.HasSqlQueue;

         var sql = GetSql();
         var args = GetArguments();
         object result = null;

         if (!_executer.HasSqlQueue)
         {
            result = _hasIdentityReturn ? _executer.ExecuteScalar<object>(sql, args) : _executer.Execute(sql, args);
         }
         else
         {
            _executer.EnqueueSql(sql, args);
         }

         _compileResult = null;
         return result;
      }

      protected virtual string GetSql()
      {
         if (_compileResult == null)
         {
            return null;
         }
         using (var sw = new StringWriter())
         {
            sw.Write("INSERT INTO ");
            sw.Write(_compileResult.TableName);
            sw.Write(" (");
            var comma = "";
            foreach (var c in _compileResult.Assignments)
            {
               sw.Write(comma);
               _formatter.FormatIdentifier(sw, c.Item1.Name);
               comma = ", ";
            }
            sw.WriteLine(")");

            sw.Write("VALUES (");
            comma = "";
            foreach (var c in _compileResult.Assignments)
            {
               sw.Write(comma);
               sw.Write(c.Item2);
               comma = ",";
            }
            sw.WriteLine(")");
            if (_hasIdentityReturn)
            {
               sw.WriteLine(_formatter.SqlDialect.StatementSeperator);
               _formatter.WriteSelectLastIdentity(sw,_identityColumn.Member.GetMemberType());
               sw.WriteLine(_formatter.SqlDialect.SelectLastIdentity);
            }
            return sw.GetStringBuilder().ToString();
         }
      }

      protected virtual IDictionary<string, object> GetArguments()
      {
         var result = new Dictionary<string, object>();
         return _argumentsAsLiterls 
            ? result 
            : (_compileResult != null 
               ? _compileResult.Arguments 
               : result);
      }

      string ISqlBuilder.GetSql()
      {
         return GetSql();
      }

      IDictionary<string, object> ISqlBuilder.GetArguments()
      {
         return GetArguments();
      }
   }
}
