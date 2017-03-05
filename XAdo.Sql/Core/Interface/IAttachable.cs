using XAdo.Core.Interface;

namespace XAdo.Quobs.Core.Interface
{
   internal interface IAttachable
   {
      IQuob Attach(IXAdoDbSession session);
   }
}