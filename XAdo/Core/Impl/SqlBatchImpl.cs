using System;
using System.Collections.Generic;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public partial class SqlBatchImpl : ISqlBatch
   {

      private readonly List<BatchItem>
         _commands = new List<BatchItem>();


      public virtual ISqlBatch Add(BatchItem batchItem)
      {
         if (batchItem == null) throw new ArgumentNullException("batchItem");
         var dict = batchItem.Args as IDictionary<string, object>;
         if (dict != null && dict.Count == 0)
         {
            batchItem.Args = null;
         }
         if (_commands.Count > 0 && batchItem.Args == null && batchItem.Callback != null)
         {
            var c = _commands.Last();
            c.Append(Seperator);
            c.Append(batchItem.Sql);
         }
         else
         {
            _commands.Add(batchItem);
         }
         return this;
      }

      internal protected virtual string Seperator { get; set; }

      public virtual bool Flush(IAdoSession session)
      {
         if (session == null) throw new ArgumentNullException("session");

         List<BatchItem> commands;
         lock (_commands)
         {
            if (!_commands.Any())
            {
               return false;
            }
            commands = _commands.ToList();
            _commands.Clear();
         }

         foreach (var item in commands)
         {
            var c = item.Callback;
            if (c != null)
            {
               c(session.ExecuteScalar<object>(item.Sql, item.Args));
            }
            else
            {
               session.Execute(item.Sql, item.Args);
            }
         }
         commands.Clear();
         return true;
      }

      public virtual bool Clear()
      {
         lock (_commands)
         {
            var result = _commands.Any();
            _commands.Clear();
            return result;
         }
      }

      public virtual int Count
      {
         get
         {
            lock (_commands)
            {
               return _commands.Count;
            }
         }
      }

   }
}
