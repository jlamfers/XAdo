using System;
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


      public IUnitOfWork Register(string sql,  IDictionary<string, object> args = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         _commands.Add(Tuple.Create(sql,args));
         return this;
      }

      public IUnitOfWork Flush(IAdoSession session)
      {
         const string sep = ";";
         var sb = new StringBuilder();
         var args = new Dictionary<string, object>();
         foreach (var cmd in _commands)
         {
            if (cmd.Item2 == null || cmd.Item2.Count == 0)
            {
               sb.AppendLine(cmd.Item1);
               sb.AppendLine(sep);
               continue;
            }
            if (cmd.Item2.Keys.Any(args.ContainsKey))
            {
               session.Execute(sb.ToString(), args, CommandType.Text);
               sb.Clear();
               args.Clear();
            }
            foreach (var kv in cmd.Item2)
            {
               args.Add(kv.Key, kv.Value);
            }
            sb.AppendLine(cmd.Item1);
            sb.AppendLine(sep);
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

      public IUnitOfWork Clear()
      {
         _commands.Clear();
         return this;
      }

      public bool HasWork
      {
         get { return _commands.Count > 0; }
      }
   }
}
