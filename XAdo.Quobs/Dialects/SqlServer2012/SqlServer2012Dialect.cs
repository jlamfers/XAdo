using XAdo.Quobs.Dialects.SqlServer;

namespace XAdo.Quobs.Dialects.SqlServer2012
{
   public class SqlServer2012Dialect : SqlServerDialect
   {
      public override string Concat
      {
         get { return "CONCAT({0,...})"; }
      }
   }
}