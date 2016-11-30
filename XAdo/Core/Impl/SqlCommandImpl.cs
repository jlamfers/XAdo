using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public partial class SqlCommandImpl : ISqlCommandQueue
   {

      private readonly List<Tuple<string,object>>
         _commands = new List<Tuple<string, object>>();


      public virtual ISqlCommandQueue Enqueue(string sql,  object args = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         _commands.Add(Tuple.Create(sql, args));
         return this;
      }

      internal protected virtual string Seperator { get; set; }

      public virtual bool Flush(IAdoSession session)
      {
         if (session == null) throw new ArgumentNullException("session");

         List<Tuple<string, object>> commands;
         lock (_commands)
         {
            if (!_commands.Any())
            {
               return false;
            }
            commands = _commands.ToList();
            _commands.Clear();
         }

         var list = CollectSqlExecuteFragments(commands);

         if (list.Count == 1)
         {
            session.Execute(list[0].Item1, list[0].Item2);
         }
         else
         {
            if (!session.HasTransaction)
            {
               var tr = session.BeginTransaction();
               try
               {
                  foreach (var items in list)
                  {
                     session.Execute(items.Item1, items.Item2);
                  }
                  tr.Commit();
               }
               catch
               {
                  tr.Rollback();
                  throw;
               }
            }
            else
            {
               foreach (var items in list)
               {
                  session.Execute(items.Item1, items.Item2);
               }
            }
            
         }
         commands.Clear();
         return true;
      }

      private List<Tuple<string,object>> CollectSqlExecuteFragments(IEnumerable<Tuple<string, object>> commands)
      {
         var list = new List<Tuple<string,object>>();
         var sb = new StringBuilder();
         var args = new Dictionary<string, object>();
         foreach (var cmd in commands)
         {
            var item2 = cmd.Item2 as IDictionary<string, object>;
            if (item2 == null)
            {
               if (sb.Length > 0)
               {
                  list.Add(Tuple.Create(sb.ToString(), (object)args));
                  sb = new StringBuilder();
                  args = new Dictionary<string, object>();
               }
               list.Add(Tuple.Create(cmd.Item1,cmd.Item2));
               continue;
            }
            if (item2.Count == 0)
            {
               sb.AppendLine(cmd.Item1);
               sb.AppendLine(Seperator);
               continue;
            }
            if (item2.Keys.Any(args.ContainsKey))
            {
               list.Add(Tuple.Create(sb.ToString(), (object)args));
               sb = new StringBuilder();
               args = new Dictionary<string, object>();
            }
            sb.AppendLine(cmd.Item1);
            sb.AppendLine(Seperator);
            foreach (var kv in item2)
            {
               args.Add(kv.Key, kv.Value);
            }
         }
         if (sb.Length > 0)
         {
            list.Add(Tuple.Create(sb.ToString(), (object)args));
         }
         return list;
      }


      public virtual ISqlCommandQueue Clear()
      {
         lock (_commands)
         {
            _commands.Clear();
         }
         return this;
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
