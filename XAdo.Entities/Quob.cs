using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Descriptor;
using XAdo.Quobs.Expressions;
using XAdo.Quobs.Sql;
using XAdo.Quobs.Sql.Formatter;

namespace XAdo.Quobs
{
   public class Quob<T> : IQuob<T>, IEnumerable<T>, ISqlBuilder
   {
      private readonly ISqlFormatter _formatter;
      private readonly ISqlExecuter _executer;

      private readonly QueryDescriptor
         _descriptor;

      public Quob(ISqlFormatter formatter, ISqlExecuter executer)
      {
         _formatter = formatter;
         _executer = executer;
         _descriptor = new QueryDescriptor(){TableName = formatter.FormatTable(typeof(T))};
      }

      public virtual MappedQuob<TMapped> Select<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var result = PrepareMapExpression(mapExpression);
         return new MappedQuob<TMapped>(_formatter,_executer,result.BinderExpression.Compile(),_descriptor.Clone(),result);
      }

      public virtual Quob<T> Discriminate(Expression<Func<T, bool>> whereClause)
      {
         var result = new WhereClauseCompiler(_formatter).Compile(whereClause);
         _descriptor.AddJoins(result.Joins.Values);
         _descriptor.DiscriminatorClausePredicates.Add(result.SqlWhereClause);
         foreach (var arg in result.Arguments)
         {
            _descriptor.DiscriminatorArguments[arg.Key] = arg.Value;
         }
         return this;
      }
      public virtual Quob<T> Where(Expression<Func<T, bool>> whereClause)
      {
         var result = new WhereClauseCompiler(_formatter).Compile(whereClause);
         _descriptor.AddJoins(result.Joins.Values);
         _descriptor.WhereClausePredicates.Add(result.SqlWhereClause);
         foreach (var arg in result.Arguments)
         {
            _descriptor.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }
      public virtual Quob<T> Skip(int skip)
      {
         _descriptor.Skip = skip;
         return this;
      }
      public virtual Quob<T> Take(int take)
      {
         _descriptor.Take = take;
         return this;
      }

      public virtual Quob<T> OrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(false, false, expressions);
      }
      public virtual Quob<T> OrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(false, true, expressions);
      }
      public virtual Quob<T> AddOrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(true, false, expressions);
      }
      public virtual Quob<T> AddOrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(true, true, expressions);
      }

      public virtual List<T> ToList(out long count)
      {
         return GetEnumerable(out count).ToList();
      }
      public virtual T[] ToArray(out long count)
      {
         return GetEnumerable(out count).ToArray();
      }
      protected virtual IEnumerable<T> GetEnumerable(out long count)
      {
         EnsureColumnsSelected();
         using (var w = new StringWriter())
         {
            _descriptor.WriteCount(w);
            w.Write(_formatter.StatementSeperator);
            if (_descriptor.IsPaged())
            {
               _formatter.FormatPageQuery(w, _descriptor);
            }
            else
            {
               _descriptor.WriteSelect(w);
            }
            return _executer.ExecuteQuery<T>(w.GetStringBuilder().ToString(), GetArguments(), out count);
         }
      }

      private Quob<T> OrderBy(bool keepOrder, bool descending, params Expression<Func<T, object>>[] expressions)
      {
         if (!keepOrder)
         {
            _descriptor.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var d = expression.GetMemberInfo().GetColumnDescriptor();
            _descriptor.OrderColumns.Add(new QueryDescriptor.OrderColumnDescriptor(_formatter.FormatColumn(d), descending));
         }
         return this;
      }

      private BinderExpressionCompiler.CompileResult<TMapped> PrepareMapExpression<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var compiler = new BinderExpressionCompiler();
         var result = compiler.Compile<TMapped>(mapExpression);
         var joins =
            result.Joins.Select(j => new QueryDescriptor.JoinDescriptor(j.Expression, j.JoinType))
               .Where(j => !_descriptor.Joins.Contains(j));
         _descriptor.Joins.AddRange(joins);
         _descriptor.SelectColumns.AddRange(result.Columns.Select(c => new QueryDescriptor.SelectColumnDescriptor(_formatter.FormatColumn(c),null)));
         _descriptor.EnsureSelectColumnsAreAliased();
         return result;
      }

      IEnumerator<T> IEnumerable<T>.GetEnumerator()
      {
         return GetEnumerator();
      }
      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
      protected virtual IEnumerator<T> GetEnumerator()
      {
         EnsureColumnsSelected();
         return _executer.ExecuteQuery<T>(GetSql(), GetArguments()).GetEnumerator();
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

      protected virtual void EnsureColumnsSelected()
      {
         if (!_descriptor.SelectColumns.Any())
         {
            foreach (var c in typeof(T).GetTableDescriptor().Columns)
            {
               _descriptor.SelectColumns.Add(new QueryDescriptor.SelectColumnDescriptor(_formatter.FormatColumn(c),_formatter.FormatIdentifier(c.Member.Name)));
            }
         }
      }
   }
}
