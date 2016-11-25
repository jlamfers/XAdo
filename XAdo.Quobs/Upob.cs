using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Dialect;

namespace XAdo.Quobs
{
   public class Upob<T> : ISqlBuilder
   {
      private static readonly HashSet<string> 
         KeyColumns = new HashSet<string>(typeof(T).GetTableDescriptor().Columns.Where(c => c.IsPKey).Select(c => c.Name));

      private readonly ISqlFormatter _formatter;
      private readonly ISqlExecuter _executer;
      private readonly bool _argumentsAsLiterls;
      private UpdateExpressionCompiler.CompileResult _compileResult;
      private SqlBuilderContext _sqlBuilderContext;
      private Expression<Func<T>> _expression;

      public Upob(ISqlExecuter executer, bool argumentsAsLiterls)
      {
         _formatter = executer.GetSqlFormatter();
         _executer = executer;
         _argumentsAsLiterls = argumentsAsLiterls;
      }

      public virtual Upob<T> Set(Expression<Func<T>> expression)
      {
         _expression = expression;
         var compiler = new UpdateExpressionCompiler(_formatter);
         _compileResult =  compiler.Compile(expression,_argumentsAsLiterls);
         return this;
      }
      public virtual Upob<T> Where(Expression<Func<T, bool>> expression)
      {
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new SqlBuilderContext(_formatter)
         {
            ArgumentsAsLiterals = _argumentsAsLiterls
         };
         _sqlBuilderContext =  sqlBuilder.BuildSql(context, expression);
         return this;
      }
      public virtual object Apply(bool enforceExecute = false)
      {
         if (_compileResult == null) return null;

         var sql = GetSql();
         var args = GetArguments();
         object result = null;

         if (enforceExecute || !_executer.HasUnitOfWork)
         {
            result = _executer.Execute(sql, args);
         }
         else
         {
            _executer.RegisterWork(sql, args);
         }

         _compileResult = null;
         _sqlBuilderContext = null;
         return result;
      }

      protected virtual string GetSql()
      {
         if (_compileResult == null)
         {
            return null;
         }
         if (_sqlBuilderContext == null)
         {
            var cols = _compileResult.KeyConstraint.Select(k => k.Item1.Name).ToArray();
            if (cols.Length != KeyColumns.Count || cols.Any(c => !KeyColumns.Contains(c)))
            {
               throw new QuobException(string.Format("Missing pkey columns in update: {0}. Add pkey columns or else use where-clause.",_expression));
            }
         }
         using (var sw = new StringWriter())
         {
            sw.Write("UPDATE ");
            sw.WriteLine(_compileResult.TableName);
            sw.Write("SET");
            var comma = "";
            foreach (var c in _compileResult.Assignments)
            {
               if (_sqlBuilderContext == null && KeyColumns.Contains(c.Item1.Name))
               {
                  continue;
               }
               sw.WriteLine(comma);
               sw.Write("  ");
               _formatter.FormatIdentifier(sw,c.Item1.Name);
               sw.Write(" = ");
               sw.Write(c.Item2);
               comma = ",";
            }
            sw.WriteLine();
            sw.Write("WHERE ");
            if (_sqlBuilderContext != null)
            {
               sw.Write(_sqlBuilderContext.ToString());
            }
            else
            {
               var and = "";
               foreach (var c in _compileResult.KeyConstraint)
               {
                  sw.Write(and);
                  _formatter.FormatIdentifier(sw, c.Item1.Name);
                  sw.Write(" = ");
                  sw.Write(c.Item2);
                  and = " AND ";
               }
            }
            return sw.GetStringBuilder().ToString();
         }
      }

      protected virtual IDictionary<string, object> GetArguments()
      {
         var result = new Dictionary<string, object>();
         if (_argumentsAsLiterls) return result;
         if (_compileResult != null)
         {
            foreach (var kv in _compileResult.Arguments)
            {
               result.Add(kv.Key,kv.Value);
            }
         }
         if (_sqlBuilderContext != null)
         {
            foreach (var kv in _sqlBuilderContext.Arguments)
            {
               result.Add(kv.Key, kv.Value);
            }
         }
         return result;
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
