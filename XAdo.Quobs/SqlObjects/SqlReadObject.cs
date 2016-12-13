using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Dialect;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects
{
   public abstract class SqlReadObject : ISqlReadObject
   {
   
      protected SqlReadObject(ISqlFormatter formatter, ISqlExecuter executer, QueryDescriptor descriptor, List<DbSchemaDescriptor.JoinPath> joins)
      {
         if (formatter == null) throw new ArgumentNullException("formatter");
         if (executer == null) throw new ArgumentNullException("executer");
         if (descriptor == null) throw new ArgumentNullException("descriptor");
         Descriptor = descriptor;
         Formatter = formatter;
         Executer = executer;
         Joins = joins ?? new List<DbSchemaDescriptor.JoinPath>();
      }


      protected ISqlFormatter Formatter { get; set; }
      protected ISqlExecuter Executer { get; set; }
      protected QueryDescriptor Descriptor { get; set; }
      protected List<DbSchemaDescriptor.JoinPath> Joins { get; set; }

      #region ISqlObject
      void ISqlObject.WriteSql(TextWriter writer)
      {
         WriteSql(writer);
      }
      protected virtual void WriteSql(TextWriter w)
      {
         if (Descriptor.IsPaged())
         {
            Formatter.WritePagedSelect(w, Descriptor);
         }
         else
         {
            Formatter.WriteSelect(w, Descriptor);
         }
      }

      object ISqlObject.GetArguments()
      {
         return GetArguments();
      }
      protected virtual object GetArguments()
      {
         return Descriptor.GetArguments();
      }
      #endregion


      public virtual bool Any()
      {
         if (Descriptor.IsPaged())
         {
            return Count() > 0;
         }
         var descriptor = Descriptor;
         try
         {
            using (var sw = new StringWriter())
            {
               Formatter.WriteExists(sw, w => Formatter.WriteSelect(w, Descriptor, true));
               var sql = sw.GetStringBuilder().ToString();
               return Executer.ExecuteScalar<bool>(sql, Descriptor.GetArguments());
            }
         }
         finally
         {
            Descriptor = descriptor;
         }
      }
      public virtual int Count()
      {
         using (var sw = new StringWriter())
         {
            Formatter.WritePagedCount(sw, Descriptor);
            var sql = sw.GetStringBuilder().ToString();
            return Executer.ExecuteScalar<int>(sql, Descriptor.GetArguments());
         }
      }

      ISqlReadObject ISqlReadObject.Where(Expression expression)
      {
         return Where(expression);
      }

      ISqlReadObject ISqlReadObject.Union(ISqlReadObject sqlReadObject)
      {
         return Union(sqlReadObject);
      }
      protected virtual ISqlReadObject Union(ISqlReadObject sqlReadObject)
      {
         //TODO
         throw new NotImplementedException();
         //Descriptor.Unions.Add(sqlReadObject);
         return this;
      }

      protected abstract ISqlReadObject Where(Expression expression);

      ISqlReadObject ISqlReadObject.OrderBy(bool keepOrder, bool @descending, params Expression[] expressions)
      {
         return OrderBy(keepOrder, @descending, expressions);
      }
      protected abstract ISqlReadObject OrderBy(bool keepOrder, bool @descending, params Expression[] expressions);

      ISqlReadObject ISqlReadObject.Distinct()
      {
         return Distinct();
      }
      protected virtual ISqlReadObject Distinct()
      {
         Descriptor.Distict = true;
         return this;
      }

      ISqlReadObject ISqlReadObject.Skip(int skip)
      {
         return Skip(skip);
      }
      protected virtual ISqlReadObject Skip(int skip)
      {
         Descriptor.Skip = skip;
         return this;
      }

      ISqlReadObject ISqlReadObject.Take(int take)
      {
         return Take(take);
      }
      protected virtual ISqlReadObject Take(int take)
      {
         Descriptor.Take = take;
         return this;
      }

      IEnumerable ISqlReadObject.FetchToEnumerable()
      {
         return FetchToEnumerable();
      }
      protected abstract IEnumerable FetchToEnumerable();

      ISqlReadObject ISqlReadObject.Attach(ISqlExecuter executer)
      {
         return Attach(executer);
      }
      protected virtual ISqlReadObject Attach(ISqlExecuter executer)
      {
         if (executer == null) throw new ArgumentNullException("executer");
         var clone = CloneSqlReadObject();
         clone.Executer = executer;
         return clone;
      }

      #region ICloneable
      protected abstract SqlReadObject CloneSqlReadObject();
      object ICloneable.Clone()
      {
         return CloneSqlReadObject();
      }
      #endregion
   }
}
