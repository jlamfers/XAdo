﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlExpression.Visitors;
using XAdo.SqlObjects.SqlObjects.Core;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects
{
   public class UpdateSqlObject<TTable> : WriteSqlObject<TTable>, IWriteWhereSqlObject<TTable>
      where TTable : IDbTable
   {

      protected static readonly HashSet<string>
         KeyColumns =
            new HashSet<string>(typeof (TTable).GetTableDescriptor().Columns.Where(c => c.IsPKey).Select(c => c.Name));


      protected Expression<Func<TTable, bool>> WhereExpression { get; private set; }
      protected SqlBuilderContext SqlBuilderContext { get; private set; }

      public UpdateSqlObject(ISqlConnection connection)
         : base(connection)
      {
      }


      public virtual new IWriteWhereSqlObject<TTable> From(Expression<Func<TTable>> expression)
      {
         return (IWriteWhereSqlObject<TTable>)base.From(expression);
      }

      public virtual IWriteWhereSqlObject<TTable> Where(Expression<Func<TTable, bool>> whereExpression)
      {
         WhereExpression = whereExpression;
         return this;
      }

      public override void Apply(bool literals = false, Action<object> callback = null)
      {
         if (WhereExpression != null)
         {
            var sqlBuilder = new SqlExpressionVisitor();
            var context = new SqlBuilderContext(Formatter,null)
            {
               ArgumentsAsLiterals = literals
            };
            SqlBuilderContext = sqlBuilder.BuildSql(context, WhereExpression);
            WhereExpression = null;
         }

         base.Apply(literals, callback);
      }

      public async override Task ApplyAsync(bool literals = false, Action<object> callback = null)
      {
         if (WhereExpression != null)
         {
            var sqlBuilder = new SqlExpressionVisitor();
            var context = new SqlBuilderContext(Formatter, null)
            {
               ArgumentsAsLiterals = literals
            };
            SqlBuilderContext = sqlBuilder.BuildSql(context, WhereExpression);
            WhereExpression = null;
         }

         await base.ApplyAsync(literals, callback);
      }

      protected override void WriteSql(TextWriter sw)
      {
         if (CompileResult == null)
         {
            return;
         }
         if (SqlBuilderContext == null)
         {
            var cols = CompileResult.KeyConstraint.Select(k => k.Item1.Name).ToArray();
            if (cols.Length != KeyColumns.Count || cols.Any(c => !KeyColumns.Contains(c)))
            {
               throw new SqlObjectsException(
                  string.Format("Missing pkey columns in update: {0}. Add pkey columns or else use where-clause.",
                     Expression));
            }
         }
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
            Formatter.FormatIdentifier(sw, c.Item1.Name);
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
      }
   }
}
