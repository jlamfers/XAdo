using System;
using XAdo.SqlObjects.DbSchema;

namespace XAdo.SqlObjects.SqlObjects.Interface
{
   public interface ITablePersister<in TTable> where TTable : IDbTable
   {
      int? Update(TTable entity, Action<object> callback = null);
      int? Delete(TTable entity, Action<object> callback = null);
      object Insert(TTable entity, Action<object> callback = null);
   }
}