using System;

namespace XAdo.SqlObjects.SqlObjects.Interface
{

   public interface IWriteSqlObject : ISqlObject
   {
      void Apply(bool literals = false, Action<object> callback = null);
   }
}