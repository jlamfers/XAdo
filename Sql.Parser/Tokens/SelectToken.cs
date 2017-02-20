using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sql.Parser.Tokens
{
   public class SelectToken : SqlToken
   {
      public SelectToken(bool distinct, IList<SqlToken> childs)
         : base(ToExpression(distinct,childs) )
      {
         Distinct = distinct;
         Childs = ConfigureMeta(childs);
      }

      public bool Distinct { get; private set; }
      public IList<MetaColumnToken> Childs { get; private set; }

      private static string ToExpression(bool distinct, IEnumerable<SqlToken> childs)
      {
         var sb = new StringBuilder().Append("SELECT " + (distinct ? "DISTINCT " : ""));
         var comma = "";
         foreach (var c in childs.OfType<ColumnToken>())
         {
            sb.AppendFormat("{0}{1}   {2}",comma,Environment.NewLine, c);
            comma = ",";
         }
         return sb.ToString();
      }

      private IList<MetaColumnToken> ConfigureMeta(IList<SqlToken> childs)
      {
         var metaChilds = new List<MetaColumnToken>();

         var prevPath = "";
         for (var i = 0; i < childs.Count; i++)
         {
            var column = (ColumnToken)childs[i];
            TagToken tag = null;
            ColumnMeta meta = null;
            string relativeName = null;

            if (i < childs.Count - 1 && childs[i + 1] is TagToken)
            {
               i++;
               tag = (TagToken)childs[i];
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
            metaChilds.Add(new MetaColumnToken(column,map, meta ?? new ColumnMeta()));
            prevPath = map.Path;
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
