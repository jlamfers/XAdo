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
      protected static readonly HashSet<string> 
         KeyColumns = new HashSet<string>(typeof(T).GetTableDescriptor().Columns.Where(c => c.IsPKey).Select(c => c.Name));

      protected readonly ISqlFormatter Formatter;
      private readonly ISqlExecuter _executer;
      private bool _argumentsAsLiterls;
      protected SetExpressionCompiler.CompileResult CompileResult;
      protected SqlBuilderContext SqlBuilderContext;
      protected Expression<Func<T>> Expression;

      public Upob(ISqlExecuter executer)
      {
         Formatter = executer.GetSqlFormatter();
         _executer = executer;
      }

      public virtual Upob<T> ArgumentsAsLiterals()
      {
         _argumentsAsLiterls = true;
         return this;
      }


      public virtual Upob<T> Set(Expression<Func<T>> expression)
      {
         Expression = expression;
         var compiler = new SetExpressionCompiler(Formatter);
         CompileResult =  compiler.Compile(expression,_argumentsAsLiterls);
         return this;
      }
      public virtual Upob<T> Where(Expression<Func<T, bool>> expression)
      {
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new SqlBuilderContext(Formatter)
         {
            ArgumentsAsLiterals = _argumentsAsLiterls
         };
         SqlBuilderContext =  sqlBuilder.BuildSql(context, expression);
         return this;
      }
      public virtual object Apply(bool enforceExecute = false)
      {
         if (!HasSql()) return null;

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

         CompileResult = null;
         SqlBuilderContext = null;
         return result;
      }

      protected virtual bool HasSql()
      {
         return CompileResult != null;
      }

      protected virtual string GetSql()
      {
         if (!HasSql())
         {
            return null;
         }
         if (SqlBuilderContext == null)
         {
            var cols = CompileResult.KeyConstraint.Select(k => k.Item1.Name).ToArray();
            if (cols.Length != KeyColumns.Count || cols.Any(c => !KeyColumns.Contains(c)))
            {
               throw new QuobException(string.Format("Missing pkey columns in update: {0}. Add pkey columns or else use where-clause.",Expression));
            }
         }
         using (var sw = new StringWriter())
         {
            sw.Write("UPDATE ");
            sw.WriteLine(CompileResult.TableName);
            sw.Write("SET");
            var comma = "";
            foreach (var c in CompileResult.Assignments)
            {
               if (SqlBuilderContext == null && KeyColumns.Contains(c.Item1.Name))
               {
                  continue;
               }
               sw.WriteLine(comma);
               sw.Write("  ");
               Formatter.FormatIdentifier(sw,c.Item1.Name);
               sw.Write(" = ");
               sw.Write(c.Item2);
               comma = ",";
            }
            sw.WriteLine();
            sw.Write("WHERE ");
            if (SqlBuilderContext != null)
            {
               sw.Write(SqlBuilderContext.ToString());
            }
            else
            {
               var and = "";
               foreach (var c in CompileResult.KeyConstraint)
               {
                  sw.Write(and);
                  Formatter.FormatIdentifier(sw, c.Item1.Name);
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
         if (CompileResult != null)
         {
            foreach (var kv in CompileResult.Arguments)
            {
               result.Add(kv.Key,kv.Value);
            }
         }
         if (SqlBuilderContext != null)
         {
            foreach (var kv in SqlBuilderContext.Arguments)
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
