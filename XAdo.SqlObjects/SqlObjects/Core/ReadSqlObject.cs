﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.Dialects;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects.Core
{
   public abstract class ReadSqlObject : IReadSqlObject
   {
   
      protected ReadSqlObject(ISqlFormatter formatter, ISqlConnection connection, QueryChunks chunks, List<DbSchemaDescriptor.JoinPath> joins)
      {
         if (formatter == null) throw new ArgumentNullException("formatter");
         //if (connection == null) throw new ArgumentNullException("connection"); can be null
         if (chunks == null) throw new ArgumentNullException("chunks");
         Chunks = chunks;
         Formatter = formatter;
         Connection = connection;
         Joins = joins ?? new List<DbSchemaDescriptor.JoinPath>();
      }


      protected ISqlFormatter Formatter { get; set; }
      protected ISqlConnection Connection { get; set; }
      protected QueryChunks Chunks { get; set; }
      protected List<DbSchemaDescriptor.JoinPath> Joins { get; set; }

      #region ISqlObject
      void ISqlObject.WriteSql(TextWriter writer)
      {
         WriteSql(writer);
      }
      protected virtual void WriteSql(TextWriter writer)
      {
         if (Chunks.IsPaged())
         {
            Formatter.WritePagedSelect(writer, Chunks);
         }
         else
         {
            Formatter.WriteSelect(writer, Chunks);
         }
      }

      object ISqlObject.GetArguments()
      {
         return GetArguments();
      }
      protected virtual object GetArguments()
      {
         return Chunks.GetArguments();
      }
      #endregion


      public virtual bool Any()
      {
         if (Chunks.IsPaged())
         {
            return Count() > 0;
         }
         var descriptor = Chunks;
         try
         {
            using (var sw = new StringWriter())
            {
               Formatter.WriteExists(sw, w => Formatter.WriteSelect(w, Chunks, true));
               var sql = sw.GetStringBuilder().ToString();
               return Connection.ExecuteScalar<bool>(sql, Chunks.GetArguments());
            }
         }
         finally
         {
            Chunks = descriptor;
         }
      }
      public virtual int Count()
      {
         using (var sw = new StringWriter())
         {
            Formatter.WritePagedCount(sw, Chunks);
            var sql = sw.GetStringBuilder().ToString();
            return Connection.ExecuteScalar<int>(sql, Chunks.GetArguments());
         }
      }

      IReadSqlObject IReadSqlObject.Where(Expression expression)
      {
         return Where(expression);
      }

      IReadSqlObject IReadSqlObject.Union(IReadSqlObject sqlReadObject)
      {
         return Union(sqlReadObject);
      }
      protected virtual IReadSqlObject Union(IReadSqlObject sqlReadObject)
      {
         Chunks.Unions.Add(sqlReadObject);
         return this;
      }

      protected abstract IReadSqlObject Where(Expression expression);

      IReadSqlObject IReadSqlObject.OrderBy(bool keepOrder, bool @descending, params Expression[] expressions)
      {
         return OrderBy(keepOrder, @descending, expressions);
      }
      protected abstract IReadSqlObject OrderBy(bool keepOrder, bool @descending, params Expression[] expressions);

      IReadSqlObject IReadSqlObject.Distinct()
      {
         return Distinct();
      }
      protected virtual IReadSqlObject Distinct()
      {
         Chunks.Distict = true;
         return this;
      }

      IReadSqlObject IReadSqlObject.Skip(int skip)
      {
         return Skip(skip);
      }
      protected virtual IReadSqlObject Skip(int skip)
      {
         Chunks.Skip = skip;
         return this;
      }

      IReadSqlObject IReadSqlObject.Take(int take)
      {
         return Take(take);
      }
      protected virtual IReadSqlObject Take(int take)
      {
         Chunks.Take = take;
         return this;
      }

      IReadSqlObject IReadSqlObject.Attach(ISqlConnection connection)
      {
         return Attach(connection);
      }

      IAliases IReadSqlObject.Aliases
      {
         get { return Chunks.Aliases; }
         set { Chunks.Aliases = value; }
      }

      protected virtual IReadSqlObject Attach(ISqlConnection executer)
      {
         if (executer == null) throw new ArgumentNullException("executer");
         var clone = CloneSqlReadObject();
         clone.Connection = executer;
         return clone;
      }

      #region ICloneable
      protected abstract ReadSqlObject CloneSqlReadObject();
      object ICloneable.Clone()
      {
         return CloneSqlReadObject();
      }
      #endregion
   }
}
