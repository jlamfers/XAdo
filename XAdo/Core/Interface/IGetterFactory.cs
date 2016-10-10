using System;
using System.Data;

namespace XAdo.Core.Interface
{
   public interface IGetterFactory
   {
      Func<IDataRecord, int, object> CreateGetter();
   }

   public interface IGetterFactory<TSetter, TGetter> : IGetterFactory
   {
      new Func<IDataRecord, int, TSetter> CreateGetter();
   }
}