using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs
{
   public class MappedQuob<T> : BaseQuob<T>
   {
      private readonly Func<IDataRecord, T> _binder;
      private readonly BinderExpressionCompiler.CompileResult<T> _binderCompileResult;

      protected internal MappedQuob(ISqlFormatter formatter, ISqlExecuter executer, Func<IDataRecord,T> binder, QueryDescriptor descriptor, BinderExpressionCompiler.CompileResult<T> binderCompileResult )
         : base(formatter, executer, descriptor)
      {
         _binder = binder;
         _binderCompileResult = binderCompileResult;
      }
      public virtual MappedQuob<T> Where(Expression<Func<T, bool>> whereClause)
      {
         var substituter = new ExpressionSubstituter();
         var unmapped = substituter.Substitute(whereClause, _binderCompileResult.MemberToExpressionMap,_binderCompileResult.OrigParameter);
         AddWhereClause(unmapped);
         return this;
      }
      public virtual MappedQuob<T> Skip(int skip)
      {
         Descriptor.Skip = skip;
         return this;
      }
      public virtual MappedQuob<T> Take(int take)
      {
         Descriptor.Take = take;
         return this;
      }

      public virtual MappedQuob<T> Union(ISqlBuilder sqlBuilder)
      {
         Descriptor.Unions.Add(sqlBuilder);
         return this;
      } 

      public virtual MappedQuob<T> OrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(false, false, expressions);
      }
      public virtual MappedQuob<T> OrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(false, true, expressions);
      }
      public virtual MappedQuob<T> AddOrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(true, false, expressions);
      }
      public virtual MappedQuob<T> AddOrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return OrderBy(true, true, expressions);
      }

      private MappedQuob<T> OrderBy(bool keepOrder, bool descending, params Expression<Func<T, object>>[] expressions)
      {
         if (!keepOrder)
         {
            Descriptor.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var m = expression.GetMemberInfo();
            var mappedColumnInfo = _binderCompileResult.Columns.Single(c => c.MappedMember == m);
            Descriptor.OrderColumns.Add(new QueryDescriptor.OrderColumnDescriptor(mappedColumnInfo.Sql, mappedColumnInfo.Alias, descending));
         }
         return this;
      }

      public virtual MappedQuob<T> Distinct()
      {
         Descriptor.Distict = true;
         return this;
      }

      protected override IEnumerable<T> GetEnumerable(out int count)
      {
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
            return Executer.ExecuteQuery(w.GetStringBuilder().ToString(), _binder, GetArguments(), out count);
         }
      }
      protected override IEnumerable<T> GetEnumerable()
      {
         return Executer.ExecuteQuery(GetSql(), _binder, GetArguments());
      }

      protected override object Clone()
      {
         return new MappedQuob<T>(Formatter, Executer, _binder, Descriptor.Clone(), _binderCompileResult);
      }
   }
}