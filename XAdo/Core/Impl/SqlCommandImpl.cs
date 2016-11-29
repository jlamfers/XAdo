using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public partial class SqlCommandImpl : ISqlCommandQueue
   {

      private readonly List<Tuple<string,IDictionary<string,object>>>
         _commands = new List<Tuple<string, IDictionary<string, object>>>();


      public virtual ISqlCommandQueue Enqueue(string sql,  IDictionary<string, object> args = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         _commands.Add(Tuple.Create(sql, args));
         return this;
      }

      public virtual ISqlCommandQueue Enqueue(string sql, object args)
      {
         return Enqueue(sql, ToDictionary(args));
      }

      private static IDictionary<string, object> ToDictionary(object args)
      {
         if (args == null) return null;
         var dict = args as IDictionary<string, object>;
         if (dict != null) return dict;
         return args.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(args));
      }

      internal protected virtual string Seperator { get; set; }

      public virtual bool Flush(IAdoSession session)
      {
         if (session == null) throw new ArgumentNullException("session");

         List<Tuple<string, IDictionary<string, object>>> commands;
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
            session.Execute(list[0].Item1.ToString(), list[0].Item2);
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
                     session.Execute(items.Item1.ToString(), items.Item2);
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
                  session.Execute(items.Item1.ToString(), items.Item2);
               }
            }
            
         }
         commands.Clear();
         return true;
      }

      private List<Tuple<StringBuilder, Dictionary<string, object>>> CollectSqlExecuteFragments(IEnumerable<Tuple<string, IDictionary<string, object>>> commands)
      {
         var list = new List<Tuple<StringBuilder, Dictionary<string, object>>>();
         var sb = new StringBuilder();
         var args = new Dictionary<string, object>();
         list.Add(Tuple.Create(sb, args));
         foreach (var cmd in commands)
         {
            if (cmd.Item2 == null || cmd.Item2.Count == 0)
            {
               sb.AppendLine(cmd.Item1);
               sb.AppendLine(Seperator);
               continue;
            }
            if (cmd.Item2.Keys.Any(args.ContainsKey))
            {
               sb = new StringBuilder();
               args = new Dictionary<string, object>();
               list.Add(Tuple.Create(sb, args));
            }
            foreach (var kv in cmd.Item2)
            {
               args.Add(kv.Key, kv.Value);
            }
            sb.AppendLine(cmd.Item1);
            sb.AppendLine(Seperator);
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
