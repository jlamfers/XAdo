using System;
using System.Collections.Generic;

namespace XAdo.Core.Interface
{
   public interface ISqlCommandQueue : IEnumerable<Tuple<string,IDictionary<string,object>>>
   {
      ISqlCommandQueue Enqueue(string sql, IDictionary<string,object> args);
      ISqlCommandQueue Enqueue(string sql, object args = null);
      ISqlCommandQueue Flush(IAdoSession session);
      ISqlCommandQueue Clear();
      int Count { get; }
   }
}