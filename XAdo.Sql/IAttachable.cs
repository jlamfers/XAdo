using XAdo.Core.Interface;
using XAdo.Sql.Core;

namespace XAdo.Sql
{
   internal interface IAttachable
   {
      IQuob Attach(IAdoSession session);
   }
}