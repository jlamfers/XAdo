using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Expressions;
using XAdo.Quobs.Sql;
using XAdo.Quobs.Sql.Formatter;

namespace XAdo.Quobs.Meta
{
   public interface IQuob<T>
   {
      
   }

   public class MappedQuob<T> : IEnumerable<T>
   {
      private readonly ISqlFormatter _formatter;
      private readonly ISqlExecuter _executer;
      private readonly Func<IDataRecord, T> _binder;
      private readonly SqlDescriptor.QueryDescriptor _descriptor;
      private readonly BinderExpressionCompiler.CompileResult<T> _binderCompileResult;

      internal MappedQuob(ISqlFormatter formatter, ISqlExecuter executer, Func<IDataRecord,T> binder, SqlDescriptor.QueryDescriptor descriptor, BinderExpressionCompiler.CompileResult<T> binderCompileResult )
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
         descriptor.Arguments[SqlDescriptor.Constants.ParNameSkip] = skip;
         return new MappedQuob<T>(_formatter, _executer, _binder, descriptor, _binderCompileResult);
      }
      public MappedQuob<T> Take(int take)
      {
         var descriptor = _descriptor.Clone();
         descriptor.Arguments[SqlDescriptor.Constants.ParNameTake] = take;
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
            descriptor.OrderColumns.Add(new SqlDescriptor.OrderColumnDescriptor(_formatter.FormatIdentifier(mappedTo.Name),descending));
         }
         return new MappedQuob<T>(_formatter, _executer, _binder, descriptor, _binderCompileResult);
      }

      public IEnumerator<T> GetEnumerator()
      {
         using (var w = new StringWriter())
         {
            _descriptor.WriteSelect(w);
            var sql = w.GetStringBuilder().ToString();
            return _executer.ExecuteQuery<T>(sql, _binder, _descriptor.Arguments).GetEnumerator();
         }
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }

   public class Quob<T> : IQuob<T>
   {
      private readonly ISqlFormatter _formatter;
      private readonly ISqlExecuter _executer;

      private readonly SqlDescriptor.QueryDescriptor
         _descriptor;

      public Quob(ISqlFormatter formatter, ISqlExecuter executer)
      {
         _formatter = formatter;
         _executer = executer;
         _descriptor = new SqlDescriptor.QueryDescriptor(){TableName = formatter.FormatTable(typeof(T))};
      }

      public MappedQuob<TMapped> Select<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var result = PrepareMapExpression(mapExpression);
         return new MappedQuob<TMapped>(_formatter,_executer,result.BinderExpression.Compile(),_descriptor.Clone(),result);
      }
      public Quob<T> Discriminate(Expression<Func<T, bool>> whereClause)
      {
         var result = new WhereClauseCompiler(_formatter).Compile(whereClause);
         var joins =
            result.Joins.Values.Select(j => new SqlDescriptor.JoinDescriptor(j.Expression, j.JoinType))
               .Where(j => !_descriptor.Joins.Contains(j));
         _descriptor.Joins.AddRange(joins);
         _descriptor.DiscriminatorPredicates.Add(result.SqlWhereClause);
         foreach (var arg in result.Arguments)
         {
            _descriptor.DiscriminatorArguments[arg.Key] = arg.Value;
         }
         return this;
      }
      public Quob<T> Where(Expression<Func<T, bool>> whereClause)
      {
         var result = new WhereClauseCompiler(_formatter).Compile(whereClause);
         var joins =
            result.Joins.Values.Select(j => new SqlDescriptor.JoinDescriptor(j.Expression, j.JoinType))
               .Where(j => !_descriptor.Joins.Contains(j));
         _descriptor.Joins.AddRange(joins);
         _descriptor.WhereClausePredicates.Add(result.SqlWhereClause);
         foreach (var arg in result.Arguments)
         {
            _descriptor.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }


      private BinderExpressionCompiler.CompileResult<TMapped> PrepareMapExpression<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var compiler = new BinderExpressionCompiler();
         var result = compiler.Compile<TMapped>(mapExpression);
         var joins =
            result.Joins.Select(j => new SqlDescriptor.JoinDescriptor(j.Expression, j.JoinType))
               .Where(j => !_descriptor.Joins.Contains(j));
         _descriptor.Joins.AddRange(joins);
         _descriptor.SelectColumns.AddRange(result.Columns.Select(c => new SqlDescriptor.SelectColumnDescriptor(_formatter.FormatColumn(c,true))));
         return result;
      }
   }
}
