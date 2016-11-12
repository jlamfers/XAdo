using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs
{
   public class Quob<T> : BaseQuob<T>
   {

      public Quob(ISqlFormatter formatter, ISqlExecuter executer)
         : base(formatter, executer, new QueryDescriptor() { TableName = formatter.MemberFormatter.FormatTable(formatter, typeof(T)) })
      {
      }

      public virtual MappedQuob<TMapped> Select<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var result = PrepareMapExpression(mapExpression);
         return new MappedQuob<TMapped>(Formatter, Executer, result.BinderExpression.Compile(), Descriptor, result);
      }

      public virtual Quob<T> Where(Expression<Func<T, bool>> whereClause)
      {
         return AddWhereClause(whereClause).CastTo<Quob<T>>();
      }
      public virtual Quob<T> Having(Expression<Func<T, bool>> havingClause)
      {
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new QuobContext(Formatter);

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
         if (!keepOrder)
         {
            Descriptor.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var d = expression.GetMemberInfo().GetColumnDescriptor();
            Descriptor.OrderColumns.Add(new QueryDescriptor.OrderColumnDescriptor(Formatter.FormatColumn(d), descending));
         }
         return this;
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
            var c = expression.GetMemberInfo().GetColumnDescriptor();
            Descriptor.GroupByColumns.Add(Formatter.FormatColumn(c));
         }
         return this;
      }

      public virtual Quob<T> Distinct()
      {
         Descriptor.Distict = true;
         return this;
      }

      private BinderExpressionCompiler.CompileResult<TMapped> PrepareMapExpression<TMapped>(Expression<Func<T, TMapped>> mapExpression)
      {
         var compiler = new BinderExpressionCompiler(Formatter);
         var result = compiler.Compile<TMapped>(mapExpression);
         Descriptor.AddJoins(result.Joins);
         Descriptor.SelectColumns.AddRange(result.Columns.Select(c => new QueryDescriptor.SelectColumnDescriptor(c.Sql, c.Alias, c.MappedMember)));
         Descriptor.EnsureSelectColumnsAreAliased();
         return result;
      }

      protected override IEnumerable<T> GetEnumerable(out int count)
      {
         EnsureColumnsSelected();
         using (var w = new StringWriter())
         {
            Descriptor.WriteTotalCount(w);
            w.Write(Formatter.StatementSeperator);
            if (Descriptor.IsPaged())
            {
               Descriptor.WritePagedQuery(w, Formatter);
            }
            else
            {
               Descriptor.WriteSelect(w);
            }
            return Executer.ExecuteQuery<T>(w.GetStringBuilder().ToString(), GetArguments(), out count);
         }
      }
      protected override IEnumerable<T> GetEnumerable()
      {
         EnsureColumnsSelected();
         return Executer.ExecuteQuery<T>(GetSql(), GetArguments());
      }

      protected override object Clone()
      {
         return new Quob<T>(Formatter, Executer) {Descriptor = Descriptor.Clone()};
      }

      protected virtual void EnsureColumnsSelected()
      {
         if (!Descriptor.SelectColumns.Any())
         {
            foreach (var c in typeof(T).GetTableDescriptor().Columns)
            {
               Descriptor.SelectColumns.Add(new QueryDescriptor.SelectColumnDescriptor(Formatter.FormatColumn(c), Formatter.FormatIdentifier(c.Member.Name), c.Member));
            }
         }
      }
   }
}
