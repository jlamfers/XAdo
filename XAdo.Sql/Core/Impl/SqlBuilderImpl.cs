using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using XAdo.Quobs.Core.Interface;
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

      public virtual string BuildCount(ISqlResource r)
      {
         if (r.Select.Distinct && r.With != null)
         {
            throw new QuobException(
               "Cannot build count query. DISTINCT must be moved to the CTE (must be moved inside the WITH part)");
         }

         var partials = r.Partials.Where(t => !SkipExpressionOnCount(t)).ToList();

         //if (!r.Select.Distinct)
         //{
         //   var selectIndex = partials.IndexOf(r.Select);
         //   var countColumn = new ColumnPartial(new[] { "COUNT(*)" }, "c1", null, new ColumnMap("c1"), 0);
         //   partials[selectIndex] = new SelectPartial(false, new[] { countColumn });
         //}
         //else
         //{
            partials.Insert(0, new SqlPartial("SELECT COUNT(*) AS c1 FROM ("));
            partials.Add(new SqlPartial(") AS __inner"));
         //}
         using (var w = new StringWriter())
         {
            foreach (var partial in partials)
            {
               partial.WriteAsTemplate(w);
            }
            return w.GetStringBuilder().ToString();
         }
      }

      protected virtual bool SkipExpressionOnCount(SqlPartial partial)
      {
         // skip all order by
         if (partial is OrderByPartial) return true;
         var template = partial as TemplatePartial;
         return template != null && _skipCountRegexes.Any(r => r.IsMatch(template.Expression));
      }

      public virtual string BuildUpdate(ISqlResource sqlResource)
      {
         throw new NotImplementedException();
      }

      public virtual string BuildDelete(ISqlResource sqlResource)
      {
         throw new NotImplementedException();
      }

      public virtual string BuildInsert(ISqlResource sqlResource)
      {
         throw new NotImplementedException();
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
