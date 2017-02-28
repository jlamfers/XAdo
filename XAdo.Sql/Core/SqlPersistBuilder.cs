using System;
using System.IO;
using System.Linq;
using System.Text;
using XAdo.Quobs.Core.Mapper;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core
{
   public class SqlPersistBuilder : ISqlPersistBuilder
   {
      public string BuildUpdate(IQueryBuilder q, bool throwException = true)
      {
         var sb = new StringBuilder();
         var updateTables =
            q.Tables.Where(
               t => q.Select.Columns.Any(c => c.Table == t && c.Meta.Persistency.HasFlag(PersistencyType.Update))).ToArray();

         var sep = "";
         foreach (var t in updateTables)
         {
            sb.AppendLine(sep);
            if (q.Select.Columns.Any(c => c.Table==t && (c.Meta.IsKey || c.AdoMeta.PKey)))
            {
               BuildUpdate(q, t, sb, throwException);
            }
            else
            {
               BuildPartialUpdate(q,t,sb,throwException);
            }
            sep = q.Dialect.StatementSeperator;
         }
         return sb.ToString();
      }

      public string BuildDelete(IQueryBuilder q, bool throwException = true)
      {
         throw new System.NotImplementedException();
      }

      public string BuildInsert(IQueryBuilder q, bool throwException = true)
      {
         throw new System.NotImplementedException();
      }

      protected virtual void BuildUpdate(IQueryBuilder q, TablePartial t, StringBuilder sb, bool throwException)
      {
            var keys = q.Select.Columns.Where(c => c.Table==t && (c.Meta.IsKey || c.AdoMeta.PKey)).ToList();
            if (!keys.Any())
            {
               if (throwException)
               {
                  throw new InvalidOperationException("Cannot build sql update if no key columns are included");
               }
               return;
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

      protected virtual void BuildPartialUpdate(IQueryBuilder q, TablePartial t, StringBuilder sb, bool throwException)
      {
         if (t.Alias == null)
         {
            //TODO: generate alias if missing??
            throw new Exception("Alias for table must be set");
         }
         var keys = q.Select.Columns.Where(c => c.Meta.IsKey || c.AdoMeta.PKey).ToList();

         if (!keys.Any())
         {
            if (throwException)
            {
               throw new InvalidOperationException("Cannot build sql update if no key columns are included");
            }
            return;
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
         q.Table.WriteAliased(w,null);
         w.WriteLine();
         foreach (var j in q.Joins)
         {
            j.Write(w,null);
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