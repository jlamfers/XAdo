using System.IO;

namespace XAdo.Quobs.SqlObjects.Interface
{

   public interface ISqlObject
   {
      void WriteSql(TextWriter writer);
      object GetArguments();
   }

   public static class SqlObjectExtension
   {
      public static string GetSql(this ISqlObject self)
      {
         if (self == null) return null;
         using (var sw = new StringWriter())
         {
            self.WriteSql(sw);
            return sw.GetStringBuilder().ToString();
         }
      }
      
   }

}
