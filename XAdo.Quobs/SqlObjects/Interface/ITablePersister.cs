using System;
using XAdo.Quobs.Core.DbSchema.Attributes;

namespace XAdo.Quobs.SqlObjects.Interface
{
   public interface ITablePersister<in TTable> where TTable : IDbTable
   {
      int? Update(TTable entity, Action<object> callback = null);
      int? Delete(TTable entity, Action<object> callback = null);
      object Insert(TTable entity, Action<object> callback = null);
   }
}