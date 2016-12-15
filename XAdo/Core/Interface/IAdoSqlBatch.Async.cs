using System.Threading.Tasks;

namespace XAdo.Core.Interface
{
   public partial interface IAdoSqlBatch
   {
      Task<bool> FlushAsync(IAdoSession session);
   }
}