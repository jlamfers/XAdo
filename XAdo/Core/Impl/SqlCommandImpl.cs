using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public class SqlCommandImpl : ISqlCommand
   {
      private IAdoSession _session;

      private readonly List<Tuple<string,IDictionary<string,object>>>
         _commands = new List<Tuple<string, IDictionary<string, object>>>();

      public ISqlCommand Attach(IAdoSession session)
      {
         if (session == null) throw new ArgumentNullException("session");
         _session = session;
         return this;
      }


      public virtual ISqlCommand Register(string sql,  IDictionary<string, object> args = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         _commands.Add(Tuple.Create(sql,args));
         return this;
      }

      public ISqlCommand Register(string sql, object args)
      {
         return Register(sql, ToDictionary(args));
      }

      private static IDictionary<string, object> ToDictionary(object args)
      {
         if (args == null) return null;
         var dict = args as IDictionary<string, object>;
         if (dict != null) return dict;
         return args.GetType().GetProperties().ToDictionary(p => p.Name, p => p.GetValue(args));
      }

      internal protected virtual string Seperator { get; set; }

      public virtual ISqlCommand Flush()
      {
         if (!_commands.Any())
         {
            return this;
         }
         var list = new List<Tuple<StringBuilder, Dictionary<string, object>>>();
         var sb = new StringBuilder();
         var args = new Dictionary<string, object>();
         list.Add(Tuple.Create(sb,args));
         foreach (var cmd in _commands)
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
         if (list.Count == 1)
         {
            _session.Execute(list[0].Item1.ToString(), list[0].Item2);
         }
         else
         {
            if (!_session.HasTransaction)
            {
               _session.BeginTransaction();
               try
               {
                  foreach (var items in list)
                  {
                     _session.Execute(items.Item1.ToString(), items.Item2);
                  }
                  _session.Commit();
               }
               catch
               {
                  _session.Rollback();
                  throw;
               }
            }
            
         }
         _commands.Clear();
         return this;
      }

      public virtual ISqlCommand Clear()
      {
         _commands.Clear();
         return this;
      }

      public virtual bool HasWork
      {
         get { return _commands.Count > 0; }
      }

      public IEnumerator<Tuple<string, IDictionary<string, object>>> GetEnumerator()
      {
         return _commands.GetEnumerator();
      }

      IEnumerator IEnumerable.GetEnumerator()
      {
         return GetEnumerator();
      }
   }
}
