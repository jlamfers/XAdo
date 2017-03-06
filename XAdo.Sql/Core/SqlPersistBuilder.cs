using System;
using System.IO;
using System.Linq;
using System.Text;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core
{
   public class SqlPersistBuilder : ISqlBuilder
   {
      public string BuildSelect(ISqlResource sqlResource)
      {
         throw new NotImplementedException();
      }

      public string BuildCount(ISqlResource sqlResource)
      {
         throw new NotImplementedException();
      }

      public string BuildUpdate(ISqlResource q)
      {
         var sb = new StringBuilder();
         var updateTables =
            q.Tables.Where(
               t => q.Select.Columns.Any(c => c.Table == t && c.Meta.Persistency.HasFlag(PersistencyType.Update))).ToArray();

         var sep = "";
         foreach (var t in updateTables)
         {
            sb.AppendLine(sep);
            if (q.Select.Columns.Any(c => c.Table==t && (c.Meta.IsPKey)))
            {
               BuildUpdate(q, t, sb);
            }
            else
            {
               BuildPartialUpdate(q,t,sb);
            }
            sep = q.Dialect.StatementSeperator;
         }
         return sb.ToString();
      }

      public string BuildDelete(ISqlResource q)
      {
         throw new System.NotImplementedException();
      }

      public string BuildInsert(ISqlResource q)
      {
         throw new System.NotImplementedException();
      }

      protected virtual void BuildUpdate(ISqlResource q, TablePartial t, StringBuilder sb)
      {
            var keys = q.Select.Columns.Where(c => c.Table==t && (c.Meta.IsPKey)).ToList();
            if (!keys.Any())
            {
               throw new QuobException("Cannot build sql update if no key columns are included");
            }
            var w = new StringWriter(sb);
            w.Write("UPDATE ");
            w.Write(t.Expression);
            w.WriteLine(" SET");
            var comma = "";
            foreach (var c in q.Select.Columns.Where(c => c.Table == t && !keys.Contains(c) && c.Meta.Persistency.HasFlag(PersistencyType.Update)))
            {
               sb
                  .Append(comma)
                  .Append(c.RawParts.Last())
                  .Append(" = ")
                  .AppendFormat(q.Dialect.ParameterFormat, c.Map.FullName.Replace('.', '_'));
               comma = ", ";
            }
            sb.AppendLine();
            comma = "";
            sb.Append("WHERE ");
            foreach (var c in keys)
            {
               sb
                  .Append(comma)
                  .Append(c.RawParts.Last())
                  .Append(" = ")
                  .AppendFormat(q.Dialect.ParameterFormat, c.Map.FullName.Replace('.', '_'));
               comma = " AND ";
            }
         }

      protected virtual void BuildPartialUpdate(ISqlResource q, TablePartial t, StringBuilder sb)
      {
         if (t.Alias == null)
         {
            //TODO: generate alias if missing??
            throw new QuobException("Alias for table must be set");
         }
         var keys = q.Select.Columns.Where(c => c.Meta.IsPKey).ToList();

         if (!keys.Any())
         {
            throw new QuobException("Cannot build sql update if no key columns are included");
         }
         var w = new StringWriter(sb);
         w.Write("UPDATE ");
         w.Write(t.Alias);
         w.WriteLine(" SET");
         var comma = "";
         foreach (var c in q.Select.Columns.Where(c => c.Table == t && !keys.Contains(c) && c.Meta.Persistency.HasFlag(PersistencyType.Update)))
         {
            sb
               .Append(comma)
               .Append(c.Expression)
               .Append(" = ")
               .AppendFormat(q.Dialect.ParameterFormat, c.Map.FullName.Replace('.', '_'));
            comma = ", ";
         }
         sb.AppendLine();
         comma = "";
         w.Write("FROM ");
         q.Table.WriteAliased(w);
         w.WriteLine();
         foreach (var j in q.Joins)
         {
            j.Write(w);
            w.WriteLine();
         }
         sb.Append("WHERE ");
         foreach (var c in keys)
         {
            sb
               .Append(comma)
               .Append(c.Expression)
               .Append(" = ")
               .AppendFormat(q.Dialect.ParameterFormat, c.Map.FullName.Replace('.', '_'));
            comma = " AND ";
         }
      }
         
   }
}