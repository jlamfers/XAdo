namespace XAdo.Core.Interface
{
   public interface IAtomic
   {
      bool Commit();
      bool Rollback();
   }
}