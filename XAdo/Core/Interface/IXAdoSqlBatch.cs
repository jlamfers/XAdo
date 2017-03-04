namespace XAdo.Core.Interface
{
   public partial interface IXAdoSqlBatch
   {
      IXAdoSqlBatch Add(XAdoSqlBatchItem item);
      bool Flush(IXAdoSession session);
      int Count { get; }
      bool Clear();
   }
}