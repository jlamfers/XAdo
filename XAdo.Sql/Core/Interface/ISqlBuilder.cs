namespace XAdo.Quobs.Core.Interface
{
   public interface ISqlBuilder
   {
      string BuildSelect(ISqlResource sqlResource, bool throwException = true);
      string BuildUpdate(ISqlResource sqlResource, bool throwException = true);
      string BuildDelete(ISqlResource sqlResource, bool throwException = true);
      string BuildInsert(ISqlResource sqlResource, bool throwException = true);
   }
}
