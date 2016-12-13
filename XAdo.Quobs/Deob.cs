using System.IO;
using System.Linq;
using XAdo.Quobs.Core;
using XAdo.Quobs.Core.DbSchema;

namespace XAdo.Quobs
{
   public class Deob<T> : Upob<T>
   {
      public Deob(ISqlConnection executer)
         : base(executer)
      {
      }

      protected override bool HasSql()
      {
         return base.HasSql() || SqlBuilderContext != null;
      }

      protected override string GetSql()
      {
         if (!HasSql())
         {
            return null;
         }
         if (SqlBuilderContext == null)
         {
            var cols = CompileResult.KeyConstraint.Select(k => k.Item1.Name).ToArray();
            if (cols.Length != KeyColumns.Count || cols.Any(c => !KeyColumns.Contains(c)))
            {
               throw new QuobException(string.Format("Missing pkey columns in delete: {0}. Add pkey columns or else use where-clause.",Expression));
            }
         }
         using (var sw = new StringWriter())
         {
            sw.Write("DELETE ");
            sw.WriteLine(typeof(T).GetTableDescriptor().Format(Formatter));
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
            return sw.GetStringBuilder().ToString();
         }
      }

   }
}
