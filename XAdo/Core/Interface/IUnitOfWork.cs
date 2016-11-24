using System;
using System.Collections.Generic;

namespace XAdo.Core.Interface
{
   public interface IUnitOfWork : IEnumerable<Tuple<string,IDictionary<string,object>>>
   {
      IUnitOfWork Register(string sql, IDictionary<string,object> args);
      IUnitOfWork Register(string sql, object args);
      IUnitOfWork Flush(IAdoSession session);
      IUnitOfWork Clear();
      bool HasWork { get; }
   }
}