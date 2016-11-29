using System;
using System.Collections.Generic;

namespace XAdo.Core.Interface
{
   public partial interface ISqlCommandQueue
   {
      ISqlCommandQueue Enqueue(string sql, IDictionary<string,object> args);
      ISqlCommandQueue Enqueue(string sql, object args = null);
      bool Flush(IAdoSession session);
      ISqlCommandQueue Clear();
      int Count { get; }
   }
}