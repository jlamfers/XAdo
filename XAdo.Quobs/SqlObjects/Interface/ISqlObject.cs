using System.IO;

namespace XAdo.Quobs.SqlObjects.Interface
{

   public interface ISqlObject
   {
      void WriteSql(TextWriter writer);
      object GetArguments();
   }
}
