using XAdo.Core.Interface;

namespace XAdo.Quobs.Interface
{
   internal interface IAttachable
   {
      IQuob Attach(IXAdoSession session);
   }
}