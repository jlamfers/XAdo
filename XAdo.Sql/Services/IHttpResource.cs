using System;
using System.Collections.Generic;
using XAdo.Quobs.Core;

namespace XAdo.Quobs.Services
{
   public interface IHttpResource<T>
   {
      T Put(object key, T instance);
      IList<T> Get(UrlQuery query);
      bool Delete(UrlQuery query);
      bool Delete(object key);
      T Post(T instance);
      T Patch(object key, object instance);
   }

   public interface IHttpResource
   {
      dynamic Put(object key, object instance);
      IList<dynamic> Get(UrlQuery query);
      bool Delete(UrlQuery query);
      bool Delete(object key);
      dynamic Post(object instance);
      dynamic Patch(object key, object instance);
      Type GetEntityType();
   }


}
