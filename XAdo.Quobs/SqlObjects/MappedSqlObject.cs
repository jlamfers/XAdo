using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.DbSchema;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.SqlObjects.Core;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects
{
   public class MappedSqlObject<TMapped> : FetchSqlObject<TMapped>, IMappedSqlObject<TMapped> {

      private readonly Func<IDataRecord, TMapped> _binder;
      private readonly BinderExpressionVisitor.CompileResult<TMapped> _binderCompileResult;

      protected internal MappedSqlObject(ISqlFormatter formatter, ISqlConnection connection, Func<IDataRecord, TMapped> binder, QueryChunks descriptor, BinderExpressionVisitor.CompileResult<TMapped> binderCompileResult, List<DbSchemaDescriptor.JoinPath> joins)
         : base(formatter, connection, descriptor, joins)
      {
         _binder = binder;
         _binderCompileResult = binderCompileResult;
      }

      protected override IReadSqlObject Where(Expression expression)
      {
         if (expression == null) return this;
         var sqlBuilder = new MappedSqlExpressionVisitor(_binderCompileResult.MemberMap.ToDictionary(m => m.Key, m => m.Value.Sql));
         var context = new SqlBuilderContext(Formatter) { ArgumentsAsLiterals = false };

         sqlBuilder.BuildSql(context, expression);
         Chunks.WhereClausePredicates.Add(context.ToString());
         foreach (var arg in context.Arguments)
         {
            Chunks.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }

      public virtual new IMappedSqlObject<TMapped> Union(IReadSqlObject sqlObject)
      {
         return (IMappedSqlObject<TMapped>)base.Union(sqlObject);
      }

      protected override IReadSqlObject OrderBy(bool keepOrder, bool @descending, params Expression[] expressions)
      {
         if (!keepOrder)
         {
            Chunks.OrderColumns.Clear();
         }
         foreach (var expression in expressions)
         {
            var m = expression.GetMemberInfo();
            var mappedColumnInfo = _binderCompileResult.MemberMap[m];
            Chunks.OrderColumns.Add(new QueryChunks.OrderColumn(mappedColumnInfo.Sql, mappedColumnInfo.Alias, descending));
         }
         return this;
      }

      public virtual new IMappedSqlObject<TMapped> Distinct()
      {
         return (IMappedSqlObject<TMapped>)base.Distinct();
      }

      public virtual new IMappedSqlObject<TMapped> Skip(int skip)
      {
         return (IMappedSqlObject<TMapped>)base.Skip(skip);
      }

      public virtual new IMappedSqlObject<TMapped> Take(int take)
      {
         return (IMappedSqlObject<TMapped>)base.Take(take);
      }

      public virtual IMappedSqlObject<TMapped> OrderBy(params Expression<Func<TMapped, object>>[] expressions)
      {
         return (IMappedSqlObject<TMapped>)OrderBy(false, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual IMappedSqlObject<TMapped> OrderByDescending(params Expression<Func<TMapped, object>>[] expressions)
      {
         return (IMappedSqlObject<TMapped>)OrderBy(false, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual IMappedSqlObject<TMapped> AddOrderBy(params Expression<Func<TMapped, object>>[] expressions)
      {
         return (IMappedSqlObject<TMapped>)OrderBy(true, false, expressions.Cast<Expression>().ToArray());
      }

      public virtual IMappedSqlObject<TMapped> AddOrderByDescending(params Expression<Func<TMapped, object>>[] expressions)
      {
         return (IMappedSqlObject<TMapped>)OrderBy(true, true, expressions.Cast<Expression>().ToArray());
      }

      public virtual new IMappedSqlObject<TMapped> Attach(ISqlConnection executer)
      {
         return (IMappedSqlObject<TMapped>)base.Attach(executer);
      }

      public virtual IMappedSqlObject<TMapped> Clone()
      {
         return (IMappedSqlObject<TMapped>)CloneSqlReadObject();
      }

      public virtual IMappedSqlObject<TMapped> Where(Expression<Func<TMapped, bool>> whereClause)
      {
         this.CastTo<IReadSqlObject>().Where(whereClause);
         return this;
      }

      protected override ReadSqlObject CloneSqlReadObject()
      {
         return new MappedSqlObject<TMapped>(Formatter, Connection, _binder, Chunks.Clone(), _binderCompileResult, Joins);
      }

      protected override IEnumerable FetchToEnumerable()
      {
         EnsureColumnsSelected();
         using (var sw = new StringWriter())
         {
            WriteSql(sw);
            return Connection.ExecuteQuery(sw.GetStringBuilder().ToString(), _binder, GetArguments());
         }
      }

      public override IEnumerable<TMapped> FetchToEnumerable(out int count)
      {
         EnsureColumnsSelected();
         using (var w = new StringWriter())
         {
            Formatter.WriteCount(w, Chunks);
            w.Write(Formatter.SqlDialect.StatementSeperator);
            if (Chunks.IsPaged())
            {
               Formatter.WritePagedSelect(w, Chunks);
            }
            else
            {
               Formatter.WriteSelect(w, Chunks);
            }
            return Connection.ExecuteQuery(w.GetStringBuilder().ToString(), _binder, GetArguments(), out count);
         }
      }
   }
}