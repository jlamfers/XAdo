using System;
using System.Collections;
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

   public class Quob<T> : BaseQuob<T>, IQuob
   {
      public Quob(ISqlExecuter executer, bool argumentsAsLiterals)
         : base(executer.GetSqlFormatter(), executer, new QueryDescriptor { FromTableName = typeof(T).GetTableDescriptor().Format(executer.GetSqlFormatter()) }, null, argumentsAsLiterals)
      {
      }

      public virtual MappedQuob<TMapped> Select<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var result = PrepareMapExpression<TMapped>(mapExpression);
         return new MappedQuob<TMapped>(Formatter, Executer, result.BinderExpression.Compile(), Descriptor, result, Joins,_argumentsAsLiterals);
      }

      public virtual Quob<T> Distinct()
      {
         Descriptor.Distict = true;
         return this;
      }

      public virtual Quob<T> ArgumentsAsLiterals()
      {
         _argumentsAsLiterals = true;
         return this;
      } 


      public virtual Quob<T> Where(Expression<Func<T, bool>> whereClause)
      {
         if (whereClause == null) return this;
         this.CastTo<IQuob>().Where(whereClause);
         return this;
      }
      public virtual Quob<T> Having(Expression<Func<T, bool>> havingClause)
      {
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new QuobContext(Formatter,Joins){ArgumentsAsLiterals = _argumentsAsLiterals};

         sqlBuilder.BuildSql(context, havingClause);
         Descriptor.AddJoins(context.QuobJoins);
         Descriptor.HavingClausePredicates.Add(context.ToString());
         foreach (var arg in context.Arguments)
         {
            Descriptor.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }

      public virtual Quob<T> Skip(int skip)
      {
         Descriptor.Skip = skip;
         return this;
      }
      public virtual Quob<T> Take(int take)
      {
         Descriptor.Take = take;
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

      protected virtual Quob<T> OrderBy(bool keepOrder, bool descending, params Expression<Func<T, object>>[] expressions)
      {
         this.CastTo<IQuob>().OrderBy(keepOrder, descending, expressions.Cast<Expression>().ToArray());
         return this;
      }

      public virtual bool Any(Expression<Func<T, bool>> predicate)
      {
         return Clone().Where(predicate).Any();
      }

      public virtual Quob<T> Union(ISqlBuilder sqlBuilder)
      {
         Descriptor.Unions.Add(sqlBuilder);
         return this;
      } 

      public virtual Quob<T> GroupBy(params Expression<Func<T, object>>[] expressions)
      {
         foreach (var expression in expressions)
         {
            var sqlBuilder = new SqlExpressionBuilder();
            var context = new QuobContext(Formatter, Joins);

            sqlBuilder.BuildSql(context, expression);
            Descriptor.AddJoins(context.QuobJoins);
            Descriptor.GroupByColumns.Add(context.ToString());
            return this;
         }
         return this;
      }


      private BinderExpressionCompiler.CompileResult<TMapped> PrepareMapExpression<TMapped>(LambdaExpression mapExpression)
      {
         var compiler = new BinderExpressionCompiler(Formatter);
         var result = compiler.Compile<TMapped>(mapExpression,Joins);
         Descriptor.AddJoins(result.Joins);
         Descriptor.SelectColumns.AddRange(result.Columns.Select(c => new QueryDescriptor.SelectColumnDescriptor(c.Sql, c.Alias)).Distinct());
         Descriptor.EnsureSelectColumnsAreAliased();
         return result;
      }

      protected override IEnumerable<T> GetEnumerable(out int count)
      {
         EnsureColumnsSelected();
         using (var w = new StringWriter())
         {
            Formatter.WriteCount(w, Descriptor);
            w.Write(Formatter.SqlDialect.StatementSeperator);
            if (Descriptor.IsPaged())
            {
               Formatter.WritePagedSelect(w, Descriptor);
            }
            else
            {
               Formatter.WriteSelect(w, Descriptor);
            }
            return Executer.ExecuteQuery<T>(w.GetStringBuilder().ToString(), GetArguments(), out count);
         }
      }
      protected override IEnumerable<T> GetEnumerable()
      {
         EnsureColumnsSelected();
         return Executer.ExecuteQuery<T>(GetSql(), GetArguments());
      }

      protected override BaseQuob<T> CloneQuob()
      {
         return new Quob<T>(Executer, _argumentsAsLiterals) { Descriptor = Descriptor.Clone(), Joins = Joins.Select(j => new DbSchemaDescriptor.JoinPath(j.Joins.Select(x => new DbSchemaDescriptor.JoinDescriptor(x.JoinInfo, x.JoinType)))).ToList() };
      }

      public Quob<T> Clone()
      {
         return (Quob<T>)CloneQuob();
      } 

      protected virtual void EnsureColumnsSelected()
      {
         if (!Descriptor.SelectColumns.Any())
         {
            foreach (var c in typeof(T).GetTableDescriptor().Columns)
            {
               Descriptor.SelectColumns.Add(new QueryDescriptor.SelectColumnDescriptor(c.Format(Formatter),Formatter.FormatIdentifier(c.Member.Name)));
            }
         }
      }

      #region IQuob

      private interface ISelectHelper
      {
         IQuob Select(IQuob quob, LambdaExpression expression);
      }
      private class SelectHelper<TMapped> : ISelectHelper
      {
         public IQuob Select(IQuob quob, LambdaExpression expression)
         {
            var q = (Quob<T>)quob;
            var result = q.PrepareMapExpression<TMapped>(expression);
            return new MappedQuob<TMapped>(q.Formatter, q.Executer, result.BinderExpression.Compile(), q.Descriptor, result, q.Joins,q._argumentsAsLiterals);
         }
      }


      IQuob IQuob.Where(Expression expression)
      {
         if (expression == null) return this;
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new QuobContext(Formatter, Joins);

         sqlBuilder.BuildSql(context, expression);
         Descriptor.WhereClausePredicates.Add(context.ToString());
         foreach (var arg in context.Arguments)
         {
            Descriptor.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }

      IQuob IQuob.OrderBy(bool keepOrder, bool @descending, params Expression[] expressions)
      {
         if (!keepOrder)
         {
            Descriptor.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var sqlBuilder = new SqlExpressionBuilder();
            var context = new QuobContext(Formatter, Joins);

            sqlBuilder.BuildSql(context, expression);
            Descriptor.AddJoins(context.QuobJoins);
            Descriptor.OrderColumns.Add(new QueryDescriptor.OrderColumnDescriptor(context.ToString(), descending));
            return this;
         }
         return this;
      }

      IQuob IQuob.Select(LambdaExpression expression)
      {
         var t = typeof (SelectHelper<>);
         t = t.MakeGenericType(typeof(T), expression.Body.Type);
         var helper = t.CreateInstance<ISelectHelper>();
         return helper.Select(this, expression);
      }

      IQuob IQuob.Connect(ISqlExecuter executer)
      {
         var clone = Clone();
         clone.Executer = executer;
         return clone;
      }

      IQuob IQuob.Distinct()
      {
         return Distinct();
      }

      IQuob IQuob.Skip(int skip)
      {
         return Skip(skip);
      }

      IQuob IQuob.Take(int take)
      {
         return Take(take);
      }

      IEnumerable IQuob.ToEnumerable()
      {
         return ToEnumerable();
      }

      #endregion
   }
}
