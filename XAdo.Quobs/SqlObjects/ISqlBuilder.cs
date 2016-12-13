using System.IO;

namespace XAdo.Quobs.SqlObjects
{
   public interface ISqlBuilder
   {
      void WriteSql(TextWriter writer);
      object GetArguments();
   }
}