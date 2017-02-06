using System;
using System.Threading.Tasks;

namespace XAdo.SqlObjects.SqlObjects.Interface
{

   public interface IWriteSqlObject : ISqlObject
   {
      void Apply(bool literals = false, Action<object> callback = null);
      Task ApplyAsync(bool literals = false, Action<object> callback = null);
   }
}