using System.Threading.Tasks;

namespace XAdo.Core.Interface
{
   public partial interface ISqlCommandQueue
   {
      Task<bool> FlushAsync(IAdoSession session);
   }
}