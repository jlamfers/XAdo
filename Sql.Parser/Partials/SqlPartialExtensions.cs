using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sql.Parser.Partials
{
   public static class SqlPartialExtensions
   {
      public static void Format(this IList<SqlPartial> self, TextWriter w, object args)
      {
         var sw = w as StringWriter;
         var sb = sw != null ? sw.GetStringBuilder() : null;
         var pos = sb != null ? sb.Length : -1;
         foreach (var t in self)
         {
            t.Write(w,args);
            if (sb != null)
            {
               if (sb.Length > pos)
               {
                  w.WriteLine();
               }
               pos = sb.Length;
            }
            else
            {
               w.WriteLine();
            }
         }
      }

      public static string Format(this IList<SqlPartial> self, object args)
      {
         using (var sw = new StringWriter())
         {
            self.Format(sw, args);
            return sw.GetStringBuilder().ToString();
         }
      }

      public static string ToTemplateString(this IList<SqlPartial> self)
      {
         var w = new StringWriter();
         var sb = w.GetStringBuilder();
         var pos = 0;
         foreach (var t in self)
         {
            w.Write(t);
            if (sb.Length > pos)
            {
               w.WriteLine();
            }
            pos = sb.Length;
         }
         return sb.ToString();
      }


      /*
   {?take}SELECT * FROM (
   $SELECT
   {?take},ROW_NUMBER() OVER (ORDER BY {order}) AS __rownum
   {?take}{!order},ROW_NUMBER() OVER (ORDER BY 1) AS __rownum
   $FROM
   $WHERE          // this is comment
   WHERE {where}  
   $GROUP_BY
   $HAVING
   {having}
   {!take}ORDER BY {order}
   {!take}{!order}ORDER BY 1
   {?take}) 
   WHERE __rowNum > {skip} AND __rowNum <= {skip}+{take} ORDER BY __rowNum

       * */

      public static List<SqlPartial> ApplyTemplate(this IList<SqlPartial> self, string template)
      {
         var parts = template.Split('\n').Select(s => s.Trim()).Where(s => s.Length > 0).ToArray();
         var index = 0;
         var partials = self.ToList();
         SqlPartial partial = null;
         var includeOrderBy = false;
         foreach (var p in parts.Select(x => x.Split(new []{"//"},StringSplitOptions.None).First().Trim()))
         {
            switch (p.ToUpper())
            {
               case "$SELECT":
                  index = partials.IndexOf(partials.OfType<SelectPartial>().Single()) + 1;
                  break;
               case "$FROM":
                  index = partials.IndexOf((SqlPartial)partials.OfType<JoinPartial>().LastOrDefault() ?? partials.OfType<TablePartial>().Single()) + 1;
                  break;
               case "$WHERE":
                  partial = partials.OfType<WherePartial>().SingleOrDefault();
                  index = partial != null ? partials.IndexOf(partial)+1 : index;
                  break;
               case "$GROUP_BY":
                  partial = partials.OfType<GroupByPartial>().SingleOrDefault();
                  index = partial != null ? partials.IndexOf(partial)+1 : index;
                  break;
               case "$HAVING":
                  partial = partials.OfType<HavingPartial>().SingleOrDefault();
                  index = partial != null ? partials.IndexOf(partial)+1 : index;
                  break;
               case "$ORDER_BY":
                  includeOrderBy = true;
                  partial = partials.OfType<OrderByPartial>().SingleOrDefault();
                  index = partial != null ? partials.IndexOf(partial)+1 : index;
                  break;
               default:

                  // probe where clause
                  var wt = partial as WherePartial;
                  if (wt != null)
                  {
                     if (wt.Expression.Length == 0)
                     {
                        var wc = p;
                        if (wc.ToUpper().StartsWith("WHERE "))
                        {
                           wc = wc.Substring(6).Trim();
                        }
                        var i = partials.IndexOf(partial);
                        partials[i] = new WherePartial(wt.WhereClause, " AND (" + wc + ")");
                     }
                     partial = null;
                     continue;
                  }

                  //probe having clause
                  var ht = partial as HavingPartial;
                  if (ht != null)
                  {
                     if (ht.Expression.Length == 0)
                     {
                        var hc = p;
                        if (hc.ToUpper().StartsWith("HAVING "))
                        {
                           hc = hc.Substring(7).Trim();
                        }
                        var i = partials.IndexOf(partial);
                        partials[i] = new HavingPartial(ht.HavingClause, " AND (" + hc + ")");
                     }
                     partial = null;
                     continue;
                  }

                  //probe order by clause
                  var ot = partial as OrderByPartial;
                  if (ot != null)
                  {
                     if (ot.Expression.Length == 0)
                     {
                        var oc = p;
                        if (oc.ToUpper().StartsWith("ORDER BY "))
                        {
                           oc = oc.Substring(9).Trim();
                        }
                        var i = partials.IndexOf(partial);
                        partials[i] = new OrderByPartial(ot.Columns, ", " + oc);
                     }
                     partial = null;
                     continue;
                  }
                  if (index <= partials.Count - 1)
                  {
                     partials.Insert(index, new TemplatePartial(p));
                  }
                  else
                  {
                     partials.Add(new TemplatePartial(p));
                  }
                  index++;
                  break;
            }
         }
         if (!includeOrderBy)
         {
            //$ORDER_BY was not in template
            partial = partials.OfType<OrderByPartial>().SingleOrDefault();
            if (partial != null)
            {
               partials.Remove(partial);
            }
         }
         return partials;
      }


   }
}