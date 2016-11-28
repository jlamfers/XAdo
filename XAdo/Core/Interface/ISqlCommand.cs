using System;
using System.Collections.Generic;

namespace XAdo.Core.Interface
{
   public interface ISqlCommand : IEnumerable<Tuple<string,IDictionary<string,object>>>
   {
      ISqlCommand Register(string sql, IDictionary<string,object> args);
      ISqlCommand Register(string sql, object args = null);
      ISqlCommand Flush();
      ISqlCommand Clear();
      ISqlCommand Attach(IAdoSession session);
      bool HasWork { get; }
   }
}