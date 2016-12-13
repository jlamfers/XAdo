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
   public class SqlReadMappedObject<TMapped> : SqlFetchObject<TMapped>, ISqlReadMappedObject<TMapped> {

      private readonly Func<IDataRecord, TMapped> _binder;
      private readonly BinderExpressionCompiler.CompileResult<TMapped> _binderCompileResult;

      protected internal SqlReadMappedObject(ISqlFormatter formatter, ISqlConnection connection, Func<IDataRecord, TMapped> binder, QueryDescriptor descriptor, BinderExpressionCompiler.CompileResult<TMapped> binderCompileResult, List<DbSchemaDescriptor.JoinPath> joins)
         : base(formatter, connection, descriptor, joins)
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

      public virtual new ISqlReadMappedObject<TMapped> Union(ISqlReadObject sqlObject)
      {
         return (ISqlReadMappedObject<TMapped>)base.Union(sqlObject);
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

      public virtual new ISqlReadMappedObject<TMapped> Distinct()
      {
         return (ISqlReadMappedObject<TMapped>)base.Distinct();
      }

      public virtual new ISqlReadMappedObject<TMapped> Skip(int skip)
      {
         return (ISqlReadMappedObject<TMapped>)base.Skip(skip);
      }

      public virtual new ISqlReadMappedObject<TMapped> Take(int take)
      {
         return (ISqlReadMappedObject<TMapped>)base.Take(take);
      }

      public virtual ISqlReadMappedObject<TMapped> OrderBy(params Expression<Func<TMapped, object>>[] expressions)
      {
         return (ISqlReadMappedObject<TMapped>)OrderBy(false, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlReadMappedObject<TMapped> OrderByDescending(params Expression<Func<TMapped, object>>[] expressions)
      {
         return (ISqlReadMappedObject<TMapped>)OrderBy(false, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlReadMappedObject<TMapped> AddOrderBy(params Expression<Func<TMapped, object>>[] expressions)
      {
         return (ISqlReadMappedObject<TMapped>)OrderBy(true, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual ISqlReadMappedObject<TMapped> AddOrderByDescending(params Expression<Func<TMapped, object>>[] expressions)
      {
         return (ISqlReadMappedObject<TMapped>)OrderBy(true, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual new ISqlReadMappedObject<TMapped> Attach(ISqlConnection executer)
      {
         return (ISqlReadMappedObject<TMapped>)base.Attach(executer);
      }

      public virtual ISqlReadMappedObject<TMapped> Clone()
      {
         return (ISqlReadMappedObject<TMapped>)CloneSqlReadObject();
      }

      public virtual ISqlReadMappedObject<TMapped> Where(Expression<Func<TMapped, bool>> whereClause)
      {
         this.CastTo<ISqlReadObject>().Where(whereClause);
         return this;
      }

      protected override SqlReadObject CloneSqlReadObject()
      {
         //TODO: do we need to clone joins??
         return new SqlReadMappedObject<TMapped>(Formatter, Connection, _binder, Descriptor.Clone(), _binderCompileResult, Joins.Select(j => new DbSchemaDescriptor.JoinPath(j.Joins.Select(x => new DbSchemaDescriptor.JoinDescriptor(x.JoinInfo, x.JoinType)))).ToList());
      }
   }
}