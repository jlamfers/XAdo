using System.IO;
using System.Linq;
using XAdo.Quobs.Core;
using XAdo.Quobs.DbSchema;
using XAdo.Quobs.DbSchema.Attributes;
using XAdo.Quobs.SqlObjects.Interface;

namespace XAdo.Quobs.SqlObjects
{
   public class DeleteSqlObject<TTable> : UpdateSqlObject<TTable>
      where TTable : IDbTable
   {

      public DeleteSqlObject(ISqlConnection connection)
         : base(connection)
      {
      }


      protected override void WriteSql(TextWriter sw)
      {
         if (CompileResult == null && SqlBuilderContext == null)
         {
            return;
         }

         if (SqlBuilderContext == null)
         {
            var cols = CompileResult.KeyConstraint.Select(k => k.Item1.Name).ToArray();
            if (cols.Length != KeyColumns.Count || cols.Any(c => !KeyColumns.Contains(c)))
            {
               throw new SqlObjectsException(
                  string.Format("Missing pkey columns in delete: {0}. Add pkey columns or else use where-clause.",
                     Expression));
            }
         }
         sw.Write("DELETE ");
         sw.WriteLine(typeof(TTable).GetTableDescriptor().Format(Formatter));
         sw.Write("WHERE ");
         if (SqlBuilderContext != null)
         {
            sw.Write(SqlBuilderContext.ToString());
         }
         else
         {

            var and = "";
            foreach (var c in CompileResult.KeyConstraint)
            {
               sw.Write(and);
               Formatter.FormatIdentifier(sw, c.Item1.Name);
               sw.Write(" = ");
               sw.Write(c.Item2);
               and = " AND ";
            }
         }
      }
   }
}