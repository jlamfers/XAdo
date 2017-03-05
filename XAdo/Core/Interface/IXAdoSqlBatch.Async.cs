using System.Threading.Tasks;

namespace XAdo.Core.Interface
{
   public partial interface IXAdoSqlBatch
   {
      Task<bool> FlushAsync(IXAdoDbSession session);
   }
}