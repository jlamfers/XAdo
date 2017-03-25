using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public sealed class SelectPartial : SqlPartial, ICloneable
   {
      private SelectPartial() { }

      public SelectPartial(bool distinct, IList<ColumnPartial> columns, int? maxRows, bool writeColumnsOnly)
         : base("SELECT" + (distinct ? " DISTINCT" : "") )
      {
         Distinct = distinct;
         Columns = ConfigureMeta(columns);
         MaxRows = maxRows;
         WriteColumnsOnly = writeColumnsOnly;
      }

      public bool Distinct { get; private set; }
      public bool WriteColumnsOnly { get; internal set; }
      public int? MaxRows { get; internal set; }
      public IList<ColumnPartial> Columns { get; private set; }

      public override void Write(TextWriter w)
      {
         if (WriteColumnsOnly)
         {
            WriteColumns(w);
         }
         else
         {
            WriteComplete(w);
         }
      }

      public void WriteComplete(TextWriter w)
      {
         w.Write("SELECT ");
         if (Distinct) w.Write("DISTINCT ");
         if (MaxRows != null)
         {
            w.Write("TOP ");
            w.Write(MaxRows + 1);
         }
         w.WriteLine();
         var comma = "";
         foreach (var c in Columns)
         {
            w.WriteLine(comma);
            w.Write("   ");
            c.Write(w);
            comma = ",";
         }
      }
      public void WriteColumns(TextWriter w)
      {
         var comma = "";
         foreach (var c in Columns)
         {
            w.WriteLine(comma);
            w.Write("   ");
            c.Write(w);
            comma = ",";
         }
      }

      object ICloneable.Clone()
      {
         return Clone();
      }
      public SelectPartial Clone()
      {
         return new SelectPartial{Columns = Columns.Select(c => c.Clone()).ToList().AsReadOnly(), Distinct = Distinct, Expression = Expression, MaxRows = MaxRows, WriteColumnsOnly = WriteColumnsOnly};
      }

      private IList<ColumnPartial> ConfigureMeta(IList<ColumnPartial> columns)
      {
         var prevPath = "";
         int ordinal = 0;
         foreach (var column in columns)
         {
            column.SetIndex(ordinal++);
            if (Distinct)
            {
               column.Meta.SetReadOnly(true);
            }
            if (column.Map == null && column.Path != null)
            {
               var fullname = ResolvePath(prevPath, column.Path);
               column.SetMap(new ColumnMap(fullname));
               prevPath = column.Map.Path;
               column.Path = null;
            }
         }
         // TODO: always reset aliases??
         var count = columns.Select(m => (m.NameOrAlias).ToLower()).Distinct().Count();
         if (count != columns.Count)
         {
            int i = 0;
            foreach (var m in columns)
            {
               m.SetAlias("c"+ i++);
            }
         }
         return columns.ToList().AsReadOnly();
      }

      private string ResolvePath(string previousPath, string path)
      {
         var stack = new Stack<int>();
         var sb = new StringBuilder();
         foreach (var ch in previousPath)
         {
            sb.Append(ch);
            if (ch == Constants.Syntax.Chars.NAME_SEP)
            {
               stack.Push(sb.Length - 1);
            }
         }
         foreach (var part in path.Split(Constants.Syntax.Chars.PATH_SEP))
         {
            switch (part)
            {
               case "":
               case Constants.Syntax.CURRENT_PATH_STR:
                  break;
               case Constants.Syntax.PREV_PATH:
                  if (stack.Count == 0)
                  {
                     throw new SqlParserException("mapping error: cannot resolve path from '{0}' to '{1}".FormatWith(previousPath,path));
                  }
                  sb.Length = stack.Pop();
                  break;
               case Constants.Syntax.PREV_PREV_PATH:
                  if (stack.Count < 2)
                  {
                     throw new SqlParserException("mapping error: cannot resolve path from '{0}' to '{1}".FormatWith(previousPath, path));
                  }
                  stack.Pop();
                  sb.Length = stack.Pop();
                  break;
               default:
                  if (sb.Length > 0) sb.Append(Constants.Syntax.Chars.NAME_SEP);
                  sb.Append(part);
                  stack.Push(sb.Length - 1);
                  break;
            }
         }
         return sb.ToString();


      }

      public override string ToString()
      {
         using (var sw = new StringWriter())
         {
            WriteComplete(sw);
            return sw.GetStringBuilder().ToString();
         }
      }
   }
}
