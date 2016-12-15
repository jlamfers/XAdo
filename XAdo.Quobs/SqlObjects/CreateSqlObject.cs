using System.IO;
using XAdo.SqlObjects.DbSchema;
using XAdo.SqlObjects.SqlExpression;
using XAdo.SqlObjects.SqlObjects.Core;
using XAdo.SqlObjects.SqlObjects.Interface;

namespace XAdo.SqlObjects.SqlObjects
{
   public class CreateSqlObject<TTable> : WriteSqlObject<TTable>
      where TTable : IDbTable
   {

      public CreateSqlObject(ISqlConnection connection)
         : base(connection)
      {
      }


      protected override void WriteSql(TextWriter writer)
      {
         if (CompileResult == null)
         {
            return;
         }
         writer.Write("INSERT INTO ");
         writer.Write(CompileResult.TableName);
         writer.Write(" (");
         var comma = "";
         foreach (var c in CompileResult.Assignments)
         {
            writer.Write(comma);
            Formatter.FormatIdentifier(writer, c.Item1.Name);
            comma = ", ";
         }
         writer.WriteLine(")");

         writer.Write("VALUES (");
         comma = "";
         foreach (var c in CompileResult.Assignments)
         {
            writer.Write(comma);
            writer.Write(c.Item2);
            comma = ",";
         }
         writer.WriteLine(")");
         if (HasIdentityReturn)
         {
            writer.WriteLine(Formatter.SqlDialect.StatementSeperator);
            Formatter.WriteSelectLastIdentity(writer, IdentityColumn.Member.GetMemberType());
            writer.WriteLine(Formatter.SqlDialect.SelectLastIdentity);
         }
      }

   }
}
