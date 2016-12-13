using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Dialect;

namespace XAdo.Quobs.Core
{
   public abstract class BaseQuob<T> : ICloneable, ISqlBuilder
   {
      protected bool _argumentsAsLiterals;

      protected BaseQuob(ISqlFormatter formatter, ISqlConnection executer, QueryDescriptor descriptor, List<DbSchemaDescriptor.JoinPath> joins, bool argumentsAsLiterals)
      {
         if (formatter == null) throw new ArgumentNullException("formatter");
         if (executer == null) throw new ArgumentNullException("executer");
         if (descriptor == null) throw new ArgumentNullException("descriptor");
         Descriptor = descriptor;
         Formatter = formatter;
         Executer = executer;
         Joins = joins ?? new List<DbSchemaDescriptor.JoinPath>();
         _argumentsAsLiterals = argumentsAsLiterals;
      }

      protected ISqlFormatter Formatter { get; set; }
      protected ISqlConnection Executer { get; set; }
      protected QueryDescriptor Descriptor { get; set; }
      protected List<DbSchemaDescriptor.JoinPath> Joins { get; set; }

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
      public virtual IDictionary<TKey, List<TValue>> ToGroupedList<TKey, TValue>(Func<T, TKey> groupKeySelector, Func<T, TValue> listElementSelector)
      {
         var dictionary = new Dictionary<TKey, List<TValue>>();
         var current = default(TKey);
         List<TValue> list = null;
         foreach (var row in GetEnumerable())
         {
            var key = groupKeySelector(row);
            if (!Equals(current, key))
            {
               if (list != null)
               {
                  try
                  {
                     dictionary.Add(current, list);
                  }
                  catch (ArgumentException ex)
                  {
                     throw new InvalidOperationException("You need to order by key first, before calling ToGroupedList", ex);
                  }
               }
               list = new List<TValue>();
               current = key;
            }
            var v = listElementSelector(row);
            if (v != null)
            {
               list.Add(v);
            }
         }
         if (list != null)
         {
            try
            {
               dictionary.Add(current, list);
            }
            catch (ArgumentException ex)
            {
               throw new InvalidOperationException("You need to order by key first, before calling ToGroupedList", ex);
            }
         }
         return dictionary;
      }

      public virtual T SingleOrDefault()
      {
         return ToEnumerable().SingleOrDefault();
      }
      public virtual T Single()
      {
         return ToEnumerable().Single();
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
      protected abstract BaseQuob<T> CloneQuob();
      object ICloneable.Clone()
      {
         return CloneQuob();
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
               Formatter.WritePagedSelect(w, Descriptor);
            }
            else
            {
               Formatter.WriteSelect(w, Descriptor);
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