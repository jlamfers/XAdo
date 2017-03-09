namespace XAdo.Quobs.Core.Interface
{
   public interface ISqlBuilder
   {
      string BuildSelect(ISqlResource sqlResource);
      string BuildTotalCount(ISqlResource sqlResource);
      string BuildUpdate(ISqlResource sqlResource);
      string BuildDelete(ISqlResource sqlResource);
      string BuildInsert(ISqlResource sqlResource);
   }
}
