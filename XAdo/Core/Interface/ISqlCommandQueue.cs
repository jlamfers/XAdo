namespace XAdo.Core.Interface
{
   public partial interface ISqlCommandQueue
   {
      ISqlCommandQueue Enqueue(string sql, object args = null);
      bool Flush(IAdoSession session);
      ISqlCommandQueue Clear();
      int Count { get; }
   }
}