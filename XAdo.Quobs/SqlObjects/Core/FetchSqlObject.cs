using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using XAdo.Quobs.Core;
using XAdo.Quobs.DbSchema;
using XAdo.Quobs.Dialects;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects.Core
{
   public abstract class FetchSqlObject<T> : ReadSqlObject, IFetchSqlObject<T>
   {
      protected FetchSqlObject(ISqlFormatter formatter, ISqlConnection connection, QueryChunks chunks, List<DbSchemaDescriptor.JoinPath> joins) 
         : base(formatter, connection, chunks, joins)
      {
      }

      public virtual List<T> FetchToList(out int count)
      {
         return FetchToEnumerable(out count).ToList();
      }

      public virtual List<T> FetchToList()
      {
         return FetchToEnumerable().Cast<T>().ToList();
      }

      public virtual T[] FetchToArray(out int count)
      {
         return FetchToEnumerable(out count).ToArray();
      }

      public virtual T[] FetchToArray()
      {
         return FetchToEnumerable().Cast<T>().ToArray();
      }

      public virtual IDictionary<TKey, TValue> FetchToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> elementSelector, out int count)
      {
         return FetchToEnumerable(out count).ToDictionary(keySelector, elementSelector);
      }

      public virtual IDictionary<TKey, TValue> FetchToDictionary<TKey, TValue>(Func<T, TKey> keySelector, Func<T, TValue> elementSelector)
      {
         return FetchToEnumerable().Cast<T>().ToDictionary(keySelector, elementSelector);
      }

      public virtual IDictionary<TKey, List<TValue>> FetchToGroupedDictionary<TKey, TValue>(Func<T, TKey> groupKeySelector, Func<T, TValue> listElementSelector)
      {
         var dictionary = new Dictionary<TKey, List<TValue>>();
         var current = default(TKey);
         List<TValue> list = null;
         foreach (var row in FetchToEnumerable().Cast<T>())
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
            if (!Equals(v,null) && list != null)
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

      public virtual T FetchSingleOrDefault()
      {
         return FetchToEnumerable().Cast<T>().SingleOrDefault();
      }

      public virtual T FetchSingle()
      {
         return FetchToEnumerable().Cast<T>().Single();
      }

      IEnumerable<T> IFetchSqlObject<T>.FetchToEnumerable()
      {
         return FetchToEnumerable().Cast<T>();
      }

      public virtual IEnumerable<T> FetchToEnumerable(out int count)
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
            return Connection.ExecuteQuery<T>(w.GetStringBuilder().ToString(), GetArguments(), out count);
         }
      }

      protected override IEnumerable FetchToEnumerable()
      {
         EnsureColumnsSelected();
         using (var sw = new StringWriter())
         {
            WriteSql(sw);
            return Connection.ExecuteQuery<T>(sw.GetStringBuilder().ToString(), GetArguments());
         }
      }


      protected virtual void EnsureColumnsSelected()
      {
         if (!Chunks.SelectColumns.Any())
         {
            foreach (var c in typeof(T).GetTableDescriptor().Columns)
            {
               Chunks.SelectColumns.Add(new QueryChunks.SelectColumn(c.Format(Formatter), Formatter.FormatIdentifier(c.Member.Name)));
            }
         }
      }

   }
}