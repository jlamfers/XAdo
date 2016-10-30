using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using XAdo.Quobs.Descriptor;
using XAdo.Quobs.Expressions;
using XAdo.Quobs.Sql;
using XAdo.Quobs.Sql.Formatter;

namespace XAdo.Quobs
{
   public class MappedQuob<T> : IEnumerable<T>, ISqlBuilder
   {
      private readonly ISqlFormatter _formatter;
      private readonly ISqlExecuter _executer;
      private readonly Func<IDataRecord, T> _binder;
      private readonly QueryDescriptor _descriptor;
      private readonly BinderExpressionCompiler.CompileResult<T> _binderCompileResult;

      internal MappedQuob(ISqlFormatter formatter, ISqlExecuter executer, Func<IDataRecord,T> binder, QueryDescriptor descriptor, BinderExpressionCompiler.CompileResult<T> binderCompileResult )
      {
         _formatter = formatter;
         _executer = executer;
         _binder = binder;
         _descriptor = descriptor;
         _binderCompileResult = binderCompileResult;
      }
      public MappedQuob<T> Where(Expression<Func<T, bool>> whereClause)
      {
         var substitutedWhereClause = new ExpressionSwapper().Substitute(whereClause, _binderCompileResult.MemberMap,_binderCompileResult.OrigParameter);
         var result = new WhereClauseCompiler(_formatter).Compile(substitutedWhereClause);
         var descriptor = _descriptor.Clone(true);
         descriptor.WhereClausePredicates.Add(result.SqlWhereClause);
         foreach (var arg in result.Arguments)
         {
            descriptor.Arguments[arg.Key] = arg.Value;
         }
         return new MappedQuob<T>(_formatter,_executer,_binder, descriptor, _binderCompileResult);
      }
      public MappedQuob<T> Skip(int skip)
      {
         var descriptor = _descriptor.Clone();
         descriptor.Skip = skip;
         return new MappedQuob<T>(_formatter, _executer, _binder, descriptor, _binderCompileResult);
      }
      public MappedQuob<T> Take(int take)
      {
         var descriptor = _descriptor.Clone();
         descriptor.Take = take;
         return new MappedQuob<T>(_formatter, _executer, _binder, descriptor, _binderCompileResult);
      }

      public MappedQuob<T> OrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(false, false, expressions);
      }
      public MappedQuob<T> OrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(false, true, expressions);
      }
      public MappedQuob<T> AddOrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(true, false, expressions);
      }
      public MappedQuob<T> AddOrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(true, true, expressions);
      }

      private MappedQuob<T> OrderBy(bool keepOrder, bool descending, params Expression<Func<T, object>>[] expressions)
      {
         var descriptor = _descriptor.Clone();
         if (!keepOrder)
         {
            descriptor.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var m = expression.GetMemberInfo();
            var mappedTo = _binderCompileResult.MemberMap[m].GetMemberInfo();
            descriptor.OrderColumns.Add(new QueryDescriptor.OrderColumnDescriptor(_formatter.FormatColumn(mappedTo.GetColumnDescriptor()),descending));
         }
         return new MappedQuob<T>(_formatter, _executer, _binder, descriptor, _binderCompileResult);
      }

      public IEnumerator<T> GetEnumerator()
      {
         return _executer.ExecuteQuery(GetSql(), _binder, GetArguments()).GetEnumerator();
      }
      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }

      string ISqlBuilder.GetSql()
      {
         return GetSql();
      }
      IDictionary<string, object> ISqlBuilder.GetArguments()
      {
         return GetArguments();
      }
      protected virtual string GetSql()
      {
         using (var w = new StringWriter())
         {
            if (_descriptor.IsPaged())
            {
               _formatter.FormatPageQuery(w, _descriptor);
            }
            else
            {
               _descriptor.WriteSelect(w);
            }
            return w.GetStringBuilder().ToString();
         }
      }
      protected virtual IDictionary<string, object> GetArguments()
      {
         return _descriptor.GetArguments();
      }
   }
}