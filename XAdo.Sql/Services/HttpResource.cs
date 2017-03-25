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
      private IXAdoDbSession _session;

      public HttpResource(IXAdoDbSession session, string sql, Type entityType = null)
      {
         _session = session;
         _sqlResource = session.GetSqlResource(sql,entityType);
      }

      public virtual dynamic Put(object key, object instance)
      {
         var sqlUpdate = _sqlResource.BuildSqlUpdate(null);
         _session.Execute(sqlUpdate, instance);
         return instance;
      }

      public virtual IList<dynamic> Get(UrlQuery query)
      {
         // /api/persons/?q=select(concat(firstname,' ',lastname)|name).where(firstname~ct~J).order(firstname~lastname).page(1-100)
         var quob = new QuobImpl(_sqlResource).Attach(_session);
         if (query != null)
         {
            query.Apply(ref quob);
         }
         var list = quob.FetchToList();
         if (list.Count > quob.SqlResource.Select.MaxRows.GetValueOrDefault(int.MaxValue))
         {
            throw new QuobException("Too many result rows (> {0}). You need to add a filter of page constraint".FormatWith(quob.SqlResource.Select.MaxRows));
         }
         return list;
      }

      public virtual bool Delete(UrlQuery query)
      {
         throw new NotImplementedException();
      }

      public virtual bool Delete(object key)
      {
         throw new NotImplementedException();
      }

      public virtual object Post(object instance)
      {
         throw new NotImplementedException();
      }

      public virtual object Patch(object key, object instance)
      {
         throw new NotImplementedException();
      }

      public Type GetEntityType()
      {
         return _sqlResource.GetEntityType(_session);
      }
   }
}
