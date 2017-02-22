using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Sql.Parser.Mapper;

namespace Sql.Parser.Partials
{
   public class SelectPartial : SqlPartial
   {
      public SelectPartial(bool distinct, IList<SqlPartial> childs)
         : base(ToExpression(distinct,childs) )
      {
         Distinct = distinct;
         Columns = ConfigureMeta(childs);
      }

      public bool Distinct { get; private set; }
      public IList<MetaColumnPartial> Columns { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("SELECT ");
         if(Distinct) w.Write("DISTINCT ");
         w.WriteLine();
         var comma = "";
         foreach (var c in Columns)
         {
            w.WriteLine(comma);
            w.Write("   ");
            c.Write(w,args);
            comma = ",";
         }
      }

      private static string ToExpression(bool distinct, IEnumerable<SqlPartial> childs)
      {
         var sb = new StringBuilder().Append("SELECT " + (distinct ? "DISTINCT " : ""));
         var comma = "";
         foreach (var c in childs.OfType<ColumnPartial>())
         {
            sb.AppendFormat("{0}{1}   {2}",comma,Environment.NewLine, c);
            comma = ",";
         }
         return sb.ToString();
      }

      private IList<MetaColumnPartial> ConfigureMeta(IList<SqlPartial> childs)
      {
         var metaChilds = new List<MetaColumnPartial>();

         var prevPath = "";
         for (var i = 0; i < childs.Count; i++)
         {
            var column = (ColumnPartial)childs[i];
            var metaColumnPartial = column as MetaColumnPartial;
            if (metaColumnPartial != null)
            {
               metaChilds.Add(metaColumnPartial);
               continue;
            }
            TagPartial tag = null;
            ColumnMeta meta = null;
            string relativeName = null;

            if (i < childs.Count - 1 && childs[i + 1] is TagPartial)
            {
               i++;
               tag = (TagPartial)childs[i];
               if (string.IsNullOrEmpty(tag.Expression))
               {
                  tag = null;
               }
            }
            if (tag != null)
            {
               meta = ColumnMeta.FindMeta(tag, Distinct, out relativeName);
               if (meta != null && relativeName == null)
               {
                  ColumnMeta.FindMeta(column, Distinct, out relativeName);
               }
            }
            meta = meta ?? ColumnMeta.FindMeta(column, Distinct, out relativeName);
            var map = new ColumnMap(ResolvePath(prevPath, relativeName));
            metaChilds.Add(new MetaColumnPartial(column, map, meta ?? new ColumnMeta(), metaChilds.Count));
            prevPath = map.Path;
         }
         var count = metaChilds.Select(m => (m.Alias ?? m.Parts.LastOrDefault() ?? "").ToLower()).Distinct().Count();
         if (count != metaChilds.Count)
         {
            int i = 0;
            foreach (var m in metaChilds)
            {
               m.SetRawAlias("c"+ i++);
            }
         }
         return metaChilds.AsReadOnly();
      }

      private string ResolvePath(string previousPath, string path)
      {
         var stack = new Stack<int>();
         var sb = new StringBuilder();
         foreach (var ch in previousPath)
         {
            sb.Append(ch);
            if (ch == '.')
            {
               stack.Push(sb.Length - 1);
            }
         }
         foreach (var part in path.Split('/'))
         {
            switch (part)
            {
               case "":
               case ".":
                  break;
               case "..":
                  sb.Length = stack.Pop();
                  break;
               case "...":
                  stack.Pop();
                  sb.Length = stack.Pop();
                  break;
               default:
                  if (sb.Length > 0) sb.Append(".");
                  sb.Append(part);
                  stack.Push(sb.Length - 1);
                  break;
            }
         }
         return sb.ToString();


      }


   }

}
