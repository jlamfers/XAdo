namespace XAdo.Core.Interface
{
   public partial interface IAdoSqlBatch
   {
      IAdoSqlBatch Add(AdoSqlBatchItem item);
      bool Flush(IAdoSession session);
      int Count { get; }
      bool Clear();
   }
}