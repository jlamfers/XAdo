using System;

namespace XAdo.Quobs.SqlObjects.Interface
{

   public interface ISqlWriteObject : ISqlObject
   {
      void Apply(bool literals = false, Action<object> callback = null);
   }
}