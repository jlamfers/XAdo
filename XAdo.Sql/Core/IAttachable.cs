using XAdo.Core.Interface;

namespace XAdo.Quobs.Core
{
   internal interface IAttachable
   {
      IQuob Attach(IAdoSession session);
   }
}