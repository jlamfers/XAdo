using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public partial class SqlCommandImpl
   {
      public virtual async Task<bool> FlushAsync(IAdoSession session)
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
            await session.ExecuteAsync(list[0].Item1.ToString(), list[0].Item2);
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
                     await session.ExecuteAsync(items.Item1.ToString(), items.Item2);
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
                  await session.ExecuteAsync(items.Item1.ToString(), items.Item2);
               }
            }
            
         }
         commands.Clear();
         return true;
      }

   }
}
