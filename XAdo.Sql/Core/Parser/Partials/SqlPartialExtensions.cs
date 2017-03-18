using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public static class SqlPartialExtensions
   {
      internal static List<SqlPartial> Clone(this IList<SqlPartial> self)
      {
         return self.Select(c => c.CloneOrElseSelf()).ToList();
      }
      internal static IList<SqlPartial> EnsureLinked(this IList<SqlPartial> self)
      {
         var selectColumns = new List<ColumnPartial>();
         var otherColumns = new List<ColumnPartial>();
         var tables = new List<TablePartial>();
         var result = new List<SqlPartial>();
         foreach (var p in self)
         {
            if (p is SelectPartial)
            {
               result.Add(p);
               selectColumns.AddRange(p.CastTo<SelectPartial>().Columns);
               continue;
            }
            if (p is FromTablePartial)
            {
               result.Add(p);
               tables.Add(p.CastTo<FromTablePartial>().Table);
               continue;
            }
            if (p is JoinPartial)
            {
               var jp = p.CastTo<JoinPartial>();
               tables.Add(jp.RighTable);
               var cols1 = jp.EquiJoinColumns.Select(c => selectColumns.SingleOrDefault(c2 => c2.SameColumn(c.Item1)) ?? otherColumns.SingleOrDefault(c2 => c2.SameColumn(c.Item1)) ?? c.Item1).ToList();
               var cols2 = jp.EquiJoinColumns.Select(c => selectColumns.SingleOrDefault(c2 => c2.SameColumn(c.Item2)) ?? otherColumns.SingleOrDefault(c2 => c2.SameColumn(c.Item2)) ?? c.Item2).ToList();
               otherColumns.AddRange(cols1.Where(c => !selectColumns.Contains(c) && !otherColumns.Contains(c)));
               otherColumns.AddRange(cols2.Where(c => !selectColumns.Contains(c) && !otherColumns.Contains(c)));
               var tuples = new List<Tuple<ColumnPartial, ColumnPartial>>();
               for(var n = 0; n < cols1.Count; n++)
               {
                  tuples.Add(Tuple.Create(cols1[n], cols2[n]));
               }
               result.Add(new JoinPartial(jp.Expression,jp.JoinType,jp.RighTable,tuples));
               continue;
            }
            if (p is OrderByPartial)
            {
               var order = p.CastTo<OrderByPartial>();
               var cols = order.Columns.Select(c => new OrderColumnPartial(selectColumns.SingleOrDefault(c2 => c2.SameColumn(c.Column)) ?? otherColumns.SingleOrDefault(c2 => c2.SameColumn(c.Column)) ?? c.Column, c.Descending)).ToList();
               otherColumns.AddRange(cols.Select(c => c.Column).Where(c => !selectColumns.Contains(c) && !otherColumns.Contains(c)));
               result.Add(new OrderByPartial(cols,order.Expression));
               continue;
            }
            if (p is GroupByPartial)
            {
               var group = p.CastTo<GroupByPartial>();
               var cols = group.Columns.Select(c => selectColumns.SingleOrDefault(c2 => c2.SameColumn(c)) ?? otherColumns.SingleOrDefault(c2 => c2.SameColumn(c)) ?? c).ToList();
               otherColumns.AddRange(cols.Where(c => !selectColumns.Contains(c) && !otherColumns.Contains(c)));
               result.Add(new GroupByPartial(cols, group.Expression));
               continue;
            }
            result.Add(p);
         }


         selectColumns.AddRange(otherColumns);
         if (tables.Count == 1)
         {
            tables[0].AttachColumns(selectColumns.Where(c => !c.IsCalculated));
         }
         else foreach (var t in tables)
         {
            // do not try to attach columns from additional order expressions
            t.AttachOwnedColumns(selectColumns);
         }

         //foreach (var c in otherColumns)
         //{
         //   if (tables.Count == 1)
         //   {
         //      if (!c.IsCalculated)
         //      {
         //         c.SetTable(tables[0]);
         //      }
         //   }
         //   else foreach (var t in tables)
         //   {
         //      if (t.IsColumnOwnerOf(c))
         //      {
         //         // table now is set
         //         break;
         //      }
         //   }
         //}

         // ensure all tables are aliased
         string[] tableAliases = null;
         var i = 1;
         const string aliasPrefix = "t";
         foreach (var table in tables)
         {
            // ensure that all tables are aliased
            if (string.IsNullOrEmpty(table.Alias))
            {
               tableAliases = tableAliases ?? tables.Select(t => t.Alias).Where(a => a != null).ToArray();
               var alias = aliasPrefix + (i++);
               while (tableAliases.Contains(alias))
               {
                  alias = aliasPrefix + (i++);
               }
               table.SetAlias(alias);
            }
         }
         return result;
      }
      internal static IList<ColumnPartial> Clone(this IList<ColumnPartial> self)
      {
         return self.Select(c => c.Clone()).ToList();
      }
      public static void ToTemplate(this IList<SqlPartial> self, TextWriter w)
      {
         var sw = w as StringWriter;
         var sb = sw != null ? sw.GetStringBuilder() : null;
         var pos = sb != null ? sb.Length : -1;
         foreach (var t in self)
         {
            t.Write(w);
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
      public static string ToTemplate(this IList<SqlPartial> self)
      {
         using (var sw = new StringWriter())
         {
            self.ToTemplate(sw);
            return sw.GetStringBuilder().ToString();
         }
      }



/*
{?take}{!order}raiserror ('no order specified', 16, 10);      // only if paging is specified (indicated by take), and no order is specified, then throw error
{?take}SELECT * FROM (                                        // only if paging is specified
@SELECT
{?take},ROW_NUMBER() OVER (ORDER BY {order}) AS __rownum      // only if both paging and order are specified
@FROM
@WHERE
WHERE {where}  
@GROUP_BY
@HAVING
HAVING {having}
//any @ORDER_BY from original sql is ignored
{!take}ORDER BY {order}                                     // only if NO paging is specified
{?take}) AS __paged                                         // only if paging is specified
{?order}WHERE __rowNum > {skip} AND __rowNum <= {skip}+{take} ORDER BY __rowNum"; // only if both order and paging are specified
*/


      private static class Markers
      {
         public const string
            SELECT = "@SELECT",
            SELECT_COLUMNS = "@SELECT_COLUMNS",
            FROM = "@FROM",
            WHERE = "@WHERE",
            GROUP_BY = "@GROUP_BY",
            HAVING = "@HAVING",
            ORDER_BY = "@ORDER_BY";
      }

      private static readonly HashSet<string> _markers = new HashSet<string>(new[] { Markers.SELECT, Markers.SELECT_COLUMNS, Markers.FROM, Markers.WHERE, Markers.GROUP_BY, Markers.HAVING, Markers.ORDER_BY });
      private static readonly Dictionary<Type, string> _typeMarkers = new Dictionary<Type, string>
      {
         {typeof(SelectPartial),Markers.SELECT},
         {typeof(FromTablePartial), Markers.FROM},
         {typeof(JoinPartial),Markers.FROM},
         {typeof(WherePartial),Markers.WHERE},
         {typeof(GroupByPartial),Markers.GROUP_BY},
         {typeof(HavingPartial),Markers.HAVING},
         {typeof(OrderByPartial),Markers.ORDER_BY},
      };

      public static List<SqlPartial> MergeTemplate(this IList<SqlPartial> self, string template)
      {
         var parts = template
            .Split('\n')
            .Select(s => s.Split(new[] { "//" }, StringSplitOptions.None).First().Trim())
            .Where(s => s.Length > 0)
            .ToArray();

         var partials = self.ToList();//.Clone();
         SqlPartial partial = null;
         var includeOrderBy = false;
         var markerLookup = new Dictionary<string, int>();
         var idx = 0;
         foreach (var p in partials)
         {
            idx++; // inc index before, so that index is right after marker position
            string marker;
            if (_typeMarkers.TryGetValue(p.GetType(), out marker))
            {
               markerLookup[marker] = idx;
               if (marker == Markers.SELECT)
               {
                  markerLookup[Markers.SELECT_COLUMNS] = idx;
               }
            }
         }
         var index = 0;
         var addedCount = 0;
         var columnsOnly = false;
         foreach (var p in parts)
         {
            var upperP = p.ToUpper();
            if (_markers.Contains(upperP))
            {
               // it is a marker

               includeOrderBy = includeOrderBy || upperP == Markers.ORDER_BY;
               // is it an ORDER_BY marker?

               if (upperP == Markers.SELECT_COLUMNS)
               {
                  columnsOnly = true;
               }

               if (markerLookup.TryGetValue(upperP, out idx))
               {
                  index = idx + addedCount;
                  partial = index < partials.Count ? partials[index] : null;
               }
               else
               {
                  index++;
                  partial = null;
               }
            }
            else
            {
               // probe where clause
               var wp = partial as WherePartial;
               if (wp != null)
               {
                  if (wp.Expression.Length == 0)
                  {
                     var wc = p;
                     if (upperP.StartsWith("WHERE "))
                     {
                        wc = wc.Substring(6).Trim();
                     }
                     partials[index] = new WherePartial(wp.WhereClause, " AND (" + wc + ")");
                  }
                  partial = null;
                  continue;
               }

               // probe group clause
               var gp = partial as GroupByPartial;
               if (gp != null)
               {
                  if (gp.Expression.Length == 0)
                  {
                     var gc = p;
                     if (upperP.StartsWith("GROUP "))
                     {
                        gc = gc.Substring(6).TrimStart();
                        if (gc.StartsWith("BY ", StringComparison.OrdinalIgnoreCase))
                        {
                           gc = gc.Substring(3).TrimStart();
                        }
                     }
                     partials[index] = new GroupByPartial(gp.Columns, ", " + gc);
                  }
                  partial = null;
                  continue;
               }

               //probe having clause
               var hp = partial as HavingPartial;
               if (hp != null)
               {
                  if (hp.Expression.Length == 0)
                  {
                     var hc = p;
                     if (upperP.StartsWith("HAVING "))
                     {
                        hc = hc.Substring(7).Trim();
                     }
                     partials[index] = new HavingPartial(hp.HavingClause, " AND (" + hc + ")");
                  }
                  partial = null;
                  continue;
               }

               //probe order by clause
               var op = partial as OrderByPartial;
               if (op != null)
               {
                  if (op.Expression.Length == 0)
                  {
                     var oc = p;
                     if (upperP.StartsWith("ORDER "))
                     {
                        oc = oc.Substring(6).TrimStart();
                        if (oc.StartsWith("BY ",StringComparison.OrdinalIgnoreCase))
                        {
                           oc = oc.Substring(3).TrimStart();
                        }
                     }
                     partials[index] = new OrderByPartial(op.Columns, ", " + oc);
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
               addedCount++;
            }
         }
         if (!includeOrderBy)
         {
            //@ORDER_BY was not in template
            // it may be moved out in case of a legacy paging template
            partial = partials.OfType<OrderByPartial>().SingleOrDefault();
            if (partial != null)
            {
               partials.Remove(partial);
            }
         }
         if (columnsOnly)
         {
            var sp = partials.OfType<SelectPartial>().SingleOrDefault();
            if (sp != null)
            {
               sp.WriteColumnsOnly = true;
            }
         }

         return partials;
      }

      public static IList<SqlPartial> AddGroupBy(this IList<SqlPartial> self, GroupByPartial groupBy)
      {
         var i = 0;
         foreach (var p in self)
         {
            var t = p as TemplatePartial;
            if (t != null && t.Expression.StartsWith("GROUP ",StringComparison.OrdinalIgnoreCase))
            {
               var exp = t.Expression.Substring(6).TrimStart();
               if (exp.StartsWith("BY ", StringComparison.OrdinalIgnoreCase))
               {
                  exp = ", "+exp.Substring(3).TrimStart();
                  self[i] = new GroupByPartial(groupBy.Columns, exp);
                  return self;
               }
            }
            if (p is HavingPartial)
            {
               self.Insert(i, groupBy);
               return self;
            }
            if (t != null && t.Expression.ToUpper().StartsWith("HAVING "))
            {
               self.Insert(i, groupBy);
               return self;
            }
            if (p is OrderByPartial)
            {
               self.Insert(i,groupBy);
               return self;
            }
            if (t != null && t.Expression.ToUpper().StartsWith("ORDER "))
            {
               self.Insert(i, groupBy);
               return self;
            }
            i++;
         }
         self.Add(groupBy);
         return self;
      }


   }
}