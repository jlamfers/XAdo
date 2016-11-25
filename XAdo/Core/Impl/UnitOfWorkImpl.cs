using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public class UnitOfWorkImpl : IUnitOfWork
   {
      private readonly List<Tuple<string,IDictionary<string,object>>>
         _commands = new List<Tuple<string, IDictionary<string, object>>>();


      public virtual IUnitOfWork Register(string sql,  IDictionary<string, object> args = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         _commands.Add(Tuple.Create(sql,args));
         return this;
      }

      public IUnitOfWork Register(string sql, object args)
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

      public virtual IUnitOfWork Flush(IAdoSession session)
      {
         
         var sb = new StringBuilder();
         var args = new Dictionary<string, object>();
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
               session.Execute(sb.ToString(), args);
               sb.Clear();
               args.Clear();
            }
            foreach (var kv in cmd.Item2)
            {
               args.Add(kv.Key, kv.Value);
            }
            sb.AppendLine(cmd.Item1);
            sb.AppendLine(Seperator);
         }
         if (sb.Length > 0)
         {
            session.Execute(sb.ToString(), args, CommandType.Text);
         }
         sb.Clear();
         args.Clear();
         _commands.Clear();
         return this;
      }

      public virtual IUnitOfWork Clear()
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
