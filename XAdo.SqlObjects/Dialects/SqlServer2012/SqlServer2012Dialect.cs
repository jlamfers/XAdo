using XAdo.SqlObjects.Dialects.SqlServer;

namespace XAdo.SqlObjects.Dialects.SqlServer2012
{
   public class SqlServer2012Dialect : SqlServerDialect
   {
      public override string Concat
      {
         get { return "CONCAT({0,...})"; }
      }
   }
}