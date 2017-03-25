using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Core.Parser.Partials;

namespace XAdo.Quobs.Core.Impl
{
   public class SqlBuilderImpl : ISqlBuilder
   {
      private static Regex[] _skipCountRegexes =
         {
            new Regex(@"\{\??skip}", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"\{\??order}", RegexOptions.IgnoreCase | RegexOptions.Compiled),
            new Regex(@"\{\??take}", RegexOptions.IgnoreCase | RegexOptions.Compiled)
         };

      public virtual string BuildSelect(ISqlResource sqlResource)
      {
         using (var w = new StringWriter())
         {
            foreach (var partial in sqlResource.Partials)
            {
               partial.WriteAsTemplate(w);
            }
            return w.GetStringBuilder().ToString();
         }
      }

      public virtual string BuildTotalCount(ISqlResource r)
      {
         if (r.Select.Distinct && r.With != null)
         {
            throw new QuobException(
               "Cannot build count query. DISTINCT must be moved to the CTE (must be moved inside the WITH part)");
         }
         var partials = r.Partials.Where(t => !SkipExpressionOnTotalCount(t)).Select(t => t.CloneOrElseSelf()).ToList();
         partials.Insert(0, new SqlPartial("SELECT COUNT(*) AS c1 FROM ("));
         partials.Add(new SqlPartial(") AS __inner"));
         using (var w = new StringWriter())
         {
            foreach (var partial in partials)
            {
               var selectPartial = partial as SelectPartial;
               if (selectPartial != null)
               {
                  // always turn threshold off when counting the total number of records
                  selectPartial.MaxRows = null;
               }
               partial.WriteAsTemplate(w);
            }
            return w.GetStringBuilder().ToString();
         }
      }

      public virtual string BuildUpdate(ISqlResource sqlResource)
      {
         var sb = new StringBuilder();
         var r = sqlResource;
         if (r.Joins == null)
         {
            if (!r.Table.CanUpdate())
            {
               return null;
            }
            BuildUpdate(r, r.Table, sb);
            return sb.ToString();
         }

         var updateTables = r.Tables.Where(t => t.CanUpdate() && t.Columns.Any(c => c.CanUpdate() && !c.Meta.IsPKey && r.Select.Columns.Contains(c))).ToArray();

         var sep = "";
         foreach (var t in updateTables)
         {
            sb.AppendLine(sep);
            BuildPartialUpdate(r, t, sb);
            sep = r.Dialect.StatementSeperator;
         }
         return sb.ToString();
      }

      public virtual string BuildDelete(ISqlResource sqlResource)
      {
         throw new NotImplementedException();
      }

      public virtual string BuildInsert(ISqlResource sqlResource)
      {
         throw new NotImplementedException();
      }

      protected virtual void BuildUpdate(ISqlResource q, TablePartial t, StringBuilder sb)
      {
         var keys = q.Select.Columns.Where(c => c.Table == t && (c.Meta.IsPKey)).ToList();
         if (!keys.Any())
         {
            throw new QuobException("Cannot build sql update if no key columns are included");
         }
         var w = new StringWriter(sb);
         w.Write("UPDATE ");
         w.Write(t.Expression);
         w.WriteLine(" SET");
         var comma = "";
         foreach (var c in q.Select.Columns.Where(c => c.Table == t && c.CanUpdate() && !c.Meta.IsPKey))
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

         #region Any output on update?
         if (q.Select.Columns.Any(c => c.Meta.OutputOnUpdate))
         {
            sb.AppendLine().AppendLine(q.Dialect.StatementSeperator);
            comma = "";
            sb.Append("SELECT ");
            foreach (var c in q.Select.Columns.Where(c => c.Meta.OutputOnUpdate))
            {
               sb
                  .Append(comma)
                  .Append(c.RawParts.Last())
                  .Append(" AS ")
                  .Append(q.Dialect.IdentifierDelimiterLeft)
                  .Append(c.Map.FullName)
                  .Append(q.Dialect.IdentifierDelimiterRight);
                  
               comma = ", ";
            }
            sb.AppendLine().AppendFormat("FROM {0}", t.Expression);
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
         #endregion

      }
      protected virtual void BuildPartialUpdate(ISqlResource q, TablePartial t, StringBuilder sb)
      {
         if (t.Alias == null)
         {
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
         foreach (var c in q.Select.Columns.Where(c => c.Table == t && !c.Meta.IsPKey && c.CanUpdate()))
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
         #region Any output on update?
         if (q.Select.Columns.Any(c => c.Table==t && c.Meta.OutputOnUpdate))
         {
            sb.AppendLine().AppendLine(q.Dialect.StatementSeperator);
            comma = "";
            sb.Append("SELECT ");
            foreach (var c in q.Select.Columns.Where(c => c.Table == t && c.Meta.OutputOnUpdate))
            {
               sb
                  .Append(comma)
                  .Append(c.Expression)
                  .Append(" AS ")
                  .Append(q.Dialect.IdentifierDelimiterLeft)
                  .Append(c.Map.FullName)
                  .Append(q.Dialect.IdentifierDelimiterRight);

               comma = ", ";
            }
            
            sb.AppendLine().AppendFormat("FROM ");
            t.WriteAliased(w);
            sb.AppendLine();
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
         #endregion
      }

      protected virtual bool SkipExpressionOnTotalCount(SqlPartial partial)
      {
         // skip all order by
         if (partial is OrderByPartial) return true;
         var template = partial as TemplatePartial;
         return template != null && _skipCountRegexes.Any(r => r.IsMatch(template.Expression));
      }


   }

   internal static class HelperExtensions
   {
      public static void WriteAsTemplate(this SqlPartial self, TextWriter w)
      {
         if (self != null)
         {
            self.Write(w);
            w.WriteLine();
         }
      }
   }
}
