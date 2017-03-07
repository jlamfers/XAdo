using System;
using System.Collections.Generic;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Impl;
using XAdo.Quobs.Core.Interface;

namespace XAdo.Quobs.Services
{
   public class RestfulApi : IRestfulApi
   {
      private readonly ISqlResource _resource;
      private IXAdoDbSession _session;

      public RestfulApi(ISqlResource resource)
      {
         _resource = resource;
      }

      public object Put(object instance)
      {
         throw new NotImplementedException();
      }

      public IList<object> Get(string selectExpression, string filterExpression, string orderExpression, int? skip, int? take)
      {
         // /api/persons/?q=select(concat(firstname,' ',lastname)|name).where(firstname~ct~J).order(firstname~lastname).page(1~100)
         // /api/persons/respond(select(concat(firstname,' ',lastname)|name).where(firstname~ct~J).order(firstname~lastname).page(1~100))
         // /api/persons/?q=select(concat(firstname,' ',lastname)|name).where(firstname~ct~J).order(firstname~lastname).page(1-100)
         var quob = new QuobImpl(_resource)
            .Attach(_session);

         if (selectExpression != null)
         {
            quob = quob.Select(selectExpression);
         }
         if (filterExpression != null)
         {
            quob = quob.Where(filterExpression);
         }
         if (orderExpression != null)
         {
            quob = quob.OrderBy(orderExpression);
         }
         if (skip != null || take != null)
         {
            quob.Skip(skip);
            quob.Take(take);
         }
         return quob.Fetch();
      }

      public bool Delete(string filterExpression)
      {
         throw new NotImplementedException();
      }

      public object Post(object instance)
      {
         throw new NotImplementedException();
      }

      public object Patch(object instance)
      {
         throw new NotImplementedException();
      }

      public void Attach(IXAdoDbSession session)
      {
         _session = session;
      }

   }
}
