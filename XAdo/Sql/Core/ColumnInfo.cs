using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace XAdo.Sql.Core
{
   [Flags]
   public enum PersistencyType
   {
      Default=3, // all
      Create=1,
      Update=2,
      None=0
   }

   public class ColumnInfo
   {
      private PersistencyType? _persistencyType;

      private ColumnInfo()
      {
         
      }

      public ColumnInfo(string expression, string alias, string map)
      {
         if (expression == null) throw new ArgumentNullException("expression");
         alias = string.IsNullOrEmpty(alias) ? null : alias;

         if (!string.IsNullOrEmpty(map))
         {
            map = FindTags(map);
         }
         else if (!string.IsNullOrEmpty(alias))
         {
            alias = FindTags(alias);
         }
         else
         {
            expression = FindTags(expression);
         }

         map = NormalizeMap(expression, alias, map);

         Expression = expression;
         Alias = alias;
         Map = map;

         var dotpos = map.LastIndexOf('/');
         if (dotpos != -1)
         {
            Name = map.Substring(dotpos + 1);
            Path = dotpos == 0 ? "/" : map.Substring(0, dotpos);
         }
         else
         {
            Name = map;
            Path = ""; // empty path, meaning keep same path
         }
      }

      public string Expression { get; private set; }
      public string Alias { get; private set; }
      public string Map { get; private set; }

      public string Path { get; private set; }

      internal void ResolveFullPath(string previousPath)
      {
         if (string.IsNullOrEmpty(Path) || Path == "." || Path == "./")
         {
            Path = previousPath;
         }
         else if (Path.StartsWith("/"))
         {
            Path = Path.Substring(1).Replace("/", ".");
         }
         else
         {
            Path = ResolvePath(previousPath, Path);
         }
         var dot = Path.Length > 0 ? "." : "";
         FullName = (Path + dot + Name);
      }

      public string Name { get; private set; }
      public string FullName { get; private set; }
      public int Index { get; internal set; }

      internal MemberInfo MappedMember { get; set; }

      public bool IsKey { get; private set; }
      public bool IsCalculated { get; private set; }
      public bool IsAutoIncrement { get; private set; }
      public bool IsOuterJoinColumn { get; private set; }
      public bool NotNull { get; private set; }

      public PersistencyType Persistency
      {
         get { return _persistencyType.GetValueOrDefault(PersistencyType.Default); }
         private set { _persistencyType = value; }
      }

      public override string ToString()
      {
         var dot = string.IsNullOrEmpty(Path) ? "" : ".";
         return string.Format("{3} -> {0}{1}{2}", Path, dot, Name, Expression);
      }

      public ColumnInfo Clone()
      {
         return new ColumnInfo
         {
            Path = Path,
            Name = Name,
            Index = Index,
            IsKey = IsKey,
            Expression = Expression,
            Alias = Alias,
            FullName = FullName,
            Map = Map,
            IsAutoIncrement = IsAutoIncrement,
            IsCalculated = IsCalculated,
            IsOuterJoinColumn = IsOuterJoinColumn,
            NotNull = NotNull,
            _persistencyType = _persistencyType
         };
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
               case"..":
                  sb.Length = stack.Pop();
                  break;
               case "...":
                  stack.Pop();
                  sb.Length = stack.Pop();
                  break;
               default:
                  if (sb.Length > 0) sb.Append(".");
                  sb.Append(part);
                  stack.Push(sb.Length-1);
                  break;
            }
         }
         return sb.ToString();


      }
      private string FindTags(string map)
      {
         // optional seperator so that 'normal' characters (behind the seperator) can be interpreted as tags
         var sepIndex = map.IndexOf(':');
         for (var i = map.Length - 1; i >= 0; i--)
         {
            var ch = map[i];
            switch (ch)
            {
               case '\r':
               case '\n':
               case '\t':
               case ' ':
                  continue;
               case '*':
                  IsKey = true;
                  Persistency &= ~PersistencyType.Update;
                  break;
               case '@':
                  IsCalculated = true;
                  Persistency &= ~PersistencyType.Create;
                  Persistency &= ~PersistencyType.Update;
                  break;
               case '+':
                  IsAutoIncrement = true;
                  IsKey = true;
                  Persistency &= ~PersistencyType.Create;
                  Persistency &= ~PersistencyType.Update;
                  break;
               case '?':
                  IsOuterJoinColumn = true;
                  break;
               case '!':
                  NotNull = true;
                  break;
               default:
                  if (sepIndex != -1 && i >= sepIndex)
                  {
                     switch (ch)
                     {
                        case ':' :
                           break;
                        case'c':
                        case 'C':
                           Persistency = _persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Create;
                           break;
                        case 'u':
                        case 'U':
                           Persistency = _persistencyType.GetValueOrDefault(PersistencyType.None) | PersistencyType.Update;
                           break;
                     }
                  }
                  else
                  {
                     map = i == map.Length - 1 ? map : map.Substring(0, i + 1);
                     return map;
                  }
                  break;
            }
         }
         return null;
      }
      private static string NormalizeMap(string expression, string alias, string map)
      {
         if (string.IsNullOrEmpty(map))
         {
            if (@alias != null)
            {
               map = @alias;
            }
            else
            {
               map = expression.Split('.').Last();
               switch (map[0])
               {
                  case '"':
                  case '\'':
                  case '`':
                  case '[':
                     map = map.Substring(1, map.Length - 2);
                     break;
               }
            }
         }
         return map;
      }

   }
}