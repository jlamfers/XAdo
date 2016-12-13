using System.Threading.Tasks;

namespace XAdo.Core.Interface
{
   public partial interface ISqlBatch
   {
      Task<bool> FlushAsync(IAdoSession session);
   }
}