namespace XAdo.Quobs.Core
{
   public interface ISqlPersistBuilder
   {
      string BuildUpdate(IQueryBuilder q, bool throwException = true);
      string BuildDelete(IQueryBuilder q, bool throwException = true);
      string BuildInsert(IQueryBuilder q, bool throwException = true);
   }
}
