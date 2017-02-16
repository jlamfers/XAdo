using XAdo.Core.Interface;
using XAdo.Sql.Core;

namespace XAdo.Sql
{
   internal interface IAttachable
   {
      void Attach(IAdoSession session);
      void SetContext(QueryContext context);

   }
}