using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core.SqlExpression;
using XAdo.Quobs.Core.SqlExpression.Sql;

namespace XAdo.Quobs.Core
{
   public abstract class BaseQuob<T> : ICloneable, ISqlBuilder
   {

      protected BaseQuob(ISqlFormatter formatter, ISqlExecuter executer, QueryDescriptor descriptor)
      {
         if (formatter == null) throw new ArgumentNullException("formatter");
         if (executer == null) throw new ArgumentNullException("executer");
         if (descriptor == null) throw new ArgumentNullException("descriptor");
         Descriptor = descriptor;
         Formatter = formatter;
         Executer = executer;
      }

      protected ISqlFormatter Formatter { get; set; }
      protected ISqlExecuter Executer { get; set; }
      protected QueryDescriptor Descriptor { get; set; }

      protected virtual BaseQuob<T> AddWhereClause(Expression whereClause)
      {
         if (whereClause == null) return this;
         var sqlBuilder = new SqlExpressionBuilder();
         var context = new QuobContext(Formatter);

         sqlBuilder.BuildSql(context, whereClause);
         Descriptor.AddJoins(context.GetJoins(whereClause.CastTo<LambdaExpression>().Parameters[0].Type));
         Descriptor.WhereClausePredicates.Add(context.ToString());
         foreach (var arg in context.Arguments)
         {
            Descriptor.Arguments[arg.Key] = arg.Value;
         }
         return this;
      }

      public virtual bool Any(Expression<Func<T, bool>> whereClause = null)
      {
         var descriptor = Descriptor;
         if (whereClause != null)
         {
            Descriptor = Descriptor.Clone();
            AddWhereClause(whereClause);
         }

         try
         {
            using (var sw = new StringWriter())
            {
               Formatter.WriteExists(sw, w => Descriptor.WriteSelect(w, true));
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
            Descriptor.WriteActualCount(sw, Formatter);
            var sql = sw.GetStringBuilder().ToString();
            return Executer.ExecuteScalar<int>(sql, Descriptor.GetArguments());
         }
      }
      public virtual List<T> ToList(out int count)
      {
         return GetEnumerable(out count).ToList();
      }
      public virtual List<T> ToList()
      {
         return GetEnumerable().ToList();
      }
      public virtual T[] ToArray(out int count)
      {
         return GetEnumerable(out count).ToArray();
      }
      public virtual T[] ToArray()
      {
         return GetEnumerable().ToArray();
      }
      public virtual IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> elementSelector,out int count)
      {
         return GetEnumerable(out count).ToDictionary(keySelector, elementSelector);
      }
      public virtual IDictionary<TKey, TValue> ToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> elementSelector)
      {
         return GetEnumerable().ToDictionary(keySelector, elementSelector);
      } 

      protected abstract IEnumerable<T> GetEnumerable(out int count);
      protected abstract IEnumerable<T> GetEnumerable();

      public IEnumerable<T> ToEnumerable()
      {
         return GetEnumerable();
      }
      public IEnumerable<T> ToEnumerable(out int count)
      {
         return GetEnumerable(out count);
      }

      #region ICloneable
      protected abstract object Clone();
      object ICloneable.Clone()
      {
         return Clone();
      }
      #endregion

      #region ISqlBuilder
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
            if (Descriptor.IsPaged())
            {
               Descriptor.WritePagedQuery(w, Formatter);
            }
            else
            {
               Descriptor.WriteSelect(w);
            }
            return w.GetStringBuilder().ToString();
         }
      }
      protected virtual IDictionary<string, object> GetArguments()
      {
         return Descriptor.GetArguments();
      }
      #endregion

   }
}