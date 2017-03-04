using System;
using System.Data;

namespace XAdo.Core.Interface
{
   public interface IXAdoGraphBinderCompiler
   {
      Func<IDataReader, object> CompileGraphReader(IDataReader reader, Type[] binderTypes, Type resultType, bool allowUnbindableFetchResults, bool allowUnbindableMembers, Delegate handler_BinderTypes_ResultType);
   }
}