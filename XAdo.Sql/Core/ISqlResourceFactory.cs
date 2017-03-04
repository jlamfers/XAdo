using System;

namespace XAdo.Quobs.Core
{
   public interface ISqlResourceFactory
   {
      ISqlResource Create(string sql, Type type);
      ISqlResource<T> Create<T>(string sql);
   }
}