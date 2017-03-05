using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public partial class XAdoSqlBatchImpl
   {
      public virtual async Task<bool> FlushAsync(IXAdoDbSession session)
      {
         if (session == null) throw new ArgumentNullException("session");

         List<XAdoSqlBatchItem> commands;
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
               var result = await session.ExecuteScalarAsync<object>(item.Sql, item.Args);
               c(result);
            }
            else
            {
               await session.ExecuteAsync(item.Sql, item.Args);
            }
         }
         commands.Clear();
         return true;
      }

   }
}
