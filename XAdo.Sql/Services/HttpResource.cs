using System;
using System.Collections.Generic;
using XAdo.Core.Interface;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.Impl;
using XAdo.Quobs.Core.Interface;

namespace XAdo.Quobs.Services
{
   public class HttpResource : IHttpResource
   {
      private readonly ISqlResource _sqlResource;
      private IXAdoDbSession _dbSession;

      public HttpResource(ISqlResource resource)
      {
         _sqlResource = resource;
      }

      public object Put(object key, object instance)
      {
         var sqlUpdate = _sqlResource.BuildSqlUpdate(null);
         _dbSession.Execute(sqlUpdate, instance);
         return instance;
      }

      public IList<object> Get(UrlQuery query)
      {
         // /api/persons/?q=select(concat(firstname,' ',lastname)|name).where(firstname~ct~J).order(firstname~lastname).page(1-100)
         var quob = new QuobImpl(_sqlResource).Attach(_dbSession);
         if (query != null)
         {
            query.Apply(ref quob);
         }
         var list = quob.Fetch();
         if (list.Count > quob.SqlResource.Select.MaxRows.GetValueOrDefault(int.MaxValue))
         {
            throw new QuobException("Too many result rows (> {0}). You need to add a filter of page constraint".FormatWith(quob.SqlResource.Select.MaxRows));
         }
         return list;
      }

      public bool Delete(UrlQuery query)
      {
         throw new NotImplementedException();
      }

      public bool Delete(object key)
      {
         throw new NotImplementedException();
      }

      public object Post(object instance)
      {
         throw new NotImplementedException();
      }

      public object Patch(object key, object instance)
      {
         throw new NotImplementedException();
      }

      public void AttachDbSession(IXAdoDbSession dbSession)
      {
         _dbSession = dbSession;
      }

   }
}
