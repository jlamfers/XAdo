using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.Dialect;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects
{
   public class SqlMapObject<T> : SqlFetchObject<T>, ISqlMapObject<T> {

      private readonly Func<IDataRecord, T> _binder;
      private readonly BinderExpressionCompiler.CompileResult<T> _binderCompileResult;

      protected internal SqlMapObject(ISqlFormatter formatter, ISqlExecuter executer, Func<IDataRecord, T> binder, QueryDescriptor descriptor, BinderExpressionCompiler.CompileResult<T> binderCompileResult, List<DbSchemaDescriptor.JoinPath> joins)
         : base(formatter, executer, descriptor, joins)
      {
         _binder = binder;
         _binderCompileResult = binderCompileResult;
      }

      protected override ISqlReadObject Where(Expression expression)
      {
         if (expression == null) return this;
         var sqlBuilder = new MappedSqlExpressionBuilder(_binderCompileResult.MemberMap.ToDictionary(m => m.Key, m => m.Value.Sql));
         var context = new SqlBuilderContext(Formatter) { ArgumentsAsLiterals = false };

         sqlBuilder.BuildSql(context, expression);
         Descriptor.WhereClausePredicates.Add(context.ToString());
         foreach (var arg in context.Arguments)
         {
            Descriptor.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }

      public virtual new ISqlMapObject<T> Union(ISqlReadObject sqlObject)
      {
         return (ISqlMapObject<T>)base.Union(sqlObject);
      }

      protected override ISqlReadObject OrderBy(bool keepOrder, bool @descending, params Expression[] expressions)
      {
         if (!keepOrder)
         {
            Descriptor.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var m = expression.GetMemberInfo();
            var mappedColumnInfo = _binderCompileResult.MemberMap[m];
            Descriptor.OrderColumns.Add(new QueryDescriptor.OrderColumnDescriptor(mappedColumnInfo.Sql, mappedColumnInfo.Alias, descending));
         }
         return this;
      }

      public virtual new ISqlMapObject<T> Distinct()
      {
         return (ISqlMapObject<T>)base.Distinct();
      }

      public virtual new ISqlMapObject<T> Skip(int skip)
      {
         return (ISqlMapObject<T>)base.Skip(skip);
      }

      public virtual new ISqlMapObject<T> Take(int take)
      {
         return (ISqlMapObject<T>)base.Take(take);
      }

      public virtual ISqlMapObject<T> OrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlMapObject<T>)OrderBy(false, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlMapObject<T> OrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlMapObject<T>)OrderBy(false, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlMapObject<T> AddOrderBy(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlMapObject<T>)OrderBy(true, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlMapObject<T> AddOrderByDescending(params Expression<Func<T, object>>[] expressions)
      {
         return (ISqlMapObject<T>)OrderBy(true, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual new ISqlMapObject<T> Attach(ISqlExecuter executer)
      {
         return (ISqlMapObject<T>)base.Attach(executer);
      }

      public virtual ISqlMapObject<T> Clone()
      {
         return (ISqlMapObject<T>)CloneSqlReadObject();
      }

      public virtual ISqlMapObject<T> Where(Expression<Func<T, bool>> whereClause)
      {
         this.CastTo<ISqlReadObject>().Where(whereClause);
         return this;
      }

      protected override SqlReadObject CloneSqlReadObject()
      {
         //TODO: do we need to clone joins??
         return new SqlMapObject<T>(Formatter, Executer, _binder, Descriptor.Clone(), _binderCompileResult, Joins.Select(j => new DbSchemaDescriptor.JoinPath(j.Joins.Select(x => new DbSchemaDescriptor.JoinDescriptor(x.JoinInfo, x.JoinType)))).ToList());
      }
   }
}