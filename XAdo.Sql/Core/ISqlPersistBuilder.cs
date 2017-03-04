namespace XAdo.Quobs.Core
{
   public interface ISqlPersistBuilder
   {
      string BuildUpdate(ISqlResource q, bool throwException = true);
      string BuildDelete(ISqlResource q, bool throwException = true);
      string BuildInsert(ISqlResource q, bool throwException = true);
   }
}
