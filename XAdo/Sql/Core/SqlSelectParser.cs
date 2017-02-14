using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XAdo.Sql.Core
{
   /// <summary>
   /// Rough SELECT parser. It only finds th select columns and corresponding aliases.
   /// </summary>
   public class SqlSelectParser
   {
      private static HashSet<char> Spaces = new HashSet<char>(new []{' ','\t','\n','\r'});
      private string _source;
      private int _pos;

      public SelectInfo Parse(string source, int startpos=0)
      {
         _pos = startpos;
         _source = source;
         var columns = new List<ColumnInfo>();
         StringBuilder 
            expr=new StringBuilder(), 
            alias = new StringBuilder(),
            map = new StringBuilder();
         var readingExpr = true;
         _pos = _source.IndexOf("SELECT", startpos, StringComparison.OrdinalIgnoreCase);
         Expect("SELECT");
         SkipAnyOff("ALL", "DISTINCTROW");
         SkipSpaces();
         var distinct = NextIs("DISTINCT");
         SkipTop();
         var selectColumnsPos = _pos;
         while (!Eof())
         {
            // now we are ready to read our columns
            switch (Peek())
            {
               case '\'':
               case '"':
               case '`':
               case '[':
                  var right = Peek() == '[' ? ']' : Peek();
                  if (readingExpr)
                  {
                     expr.Append(ReadQuoted(Peek(), right));
                  }
                  else
                  {
                     var quoted = ReadQuoted(Peek(), right);
                     alias.Append(quoted.Substring(1,quoted.Length-2));
                  }
                  break;
               case '(':
                  if (readingExpr)
                  {
                     expr.Append(ReadParenthesed());
                  }
                  else
                  {
                     throw new Exception("parenthese is not allowed in alias");
                  }
                  break;
               case ',':
                  Next();
                  SkipSpaces();
                  if (NextIs("-->") || NextIs("-- >"))
                  {
                     // read the mapping
                     ReadUntilEndOfLine(map);
                  }
                  columns.Add(new ColumnInfo(expr.ToString().Trim(),alias.ToString().Trim(),map.ToString().Trim()));
                  expr.Length = 0;
                  alias.Length = 0;
                  map.Length = 0;
                  readingExpr = true;
                  break;
               default:
                  if (NextIs("-->") || NextIs("-- >"))
                  {
                     // read the mapping
                     ReadUntilEndOfLine(map);
                  }
                  if (NextIs("--"))
                  {
                     // skip the comments
                     ReadUntilEndOfLine(null);
                  }
                  else if (IsLetter())
                  {
                     if (NextIs("AS"))
                     {
                        readingExpr = false;
                     }
                     else if (NextIs("FROM"))
                     {
                        if (expr.Length > 0)
                        {
                           if (!columns.Any() && expr[0] == '*')
                           {
                              // probe next SELECT
                              return Parse(_source,_pos);
                           }
                           columns.Add(new ColumnInfo(expr.ToString().Trim(), alias.ToString().Trim(),map.ToString().Trim()));
                        }
                        var fromPos = _pos - 4;
                        SkipSpaces();
                        var tables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        var tablename = ReadUntilSpace();
                        string tablealias = null;
                        SkipSpaces();
                        if (NextIs("AS"))
                        {
                           SkipSpaces();
                           tablealias = ReadUntilSpace();
                           SkipSpaces();
                        }
                        tables.Add(tablealias ?? tablename, tablename);
                        while (SkipAnyOff("INNER", "OUTER", "LEFT", "RIGHT", "JOIN", "FULL") && !Eof())
                        {
                           SkipSpaces();
                           while (SkipAnyOff("INNER", "OUTER", "JOIN"))
                           {
                              SkipSpaces();
                           }
                           tablename = ReadUntilSpace();
                           tablealias = null;
                           SkipSpaces();
                           if (NextIs("AS"))
                           {
                              SkipSpaces();
                              tablealias = ReadUntilSpace();
                              SkipSpaces();
                           }
                           tables.Add(tablealias ?? tablename, tablename);
                           Expect("ON");
                           SkipSpaces();
                           while (!SkipAnyOff("INNER", "OUTER", "LEFT", "RIGHT", "JOIN", "FULL") && !Eof())
                           {
                              if (SkipAnyOff("WHERE", "HAVING", "ORDER", "UNION"))
                              {
                                 return new SelectInfo(_source,columns, tables,distinct,selectColumnsPos, fromPos);
                              }
                              _pos++;
                           }

                        }

                        return new SelectInfo(_source,columns, tables, distinct, selectColumnsPos, fromPos);
                     }
                     else
                     {
                        var identifier = ReadIdentifier();
                        if (readingExpr)
                        {
                           expr.Append(identifier);
                        }
                        else
                        {
                           alias.Append(identifier);
                        }
                     }
                  }
                  else
                  {
                     Take(readingExpr ? expr : alias);
                  }
                  break;
            }
         }
         throw new Exception("Unexpected eof");
      }

      private string ReadParenthesed()
      {
         var sb = new StringBuilder();
         if (Peek() != '(')
         {
            throw new Exception("Invalid parenthesed start char");
         }
         Take(sb);
         var count = 1;
         while (!Eof())
         {
            char ch;
            sb.Append(ch = Next());
            if (ch == '(')
            {
               count++;
            }
            else if (ch == ')')
            {
               count--;
            }
            if (count==0)
            {
               return sb.ToString();
            }
         }
         throw new Exception("Unterminated quoted");
      }
      private string ReadQuoted(char left, char right)
      {
         var sb = new StringBuilder();
         if (Peek() != left)
         {
            throw new Exception("Invalid quoted start char");
         }
         Take(sb);
         while (!Eof())
         {
            char ch;
            sb.Append(ch = Next());
            if (ch == right)
            {
               return sb.ToString();
            }
         }
         throw new Exception("Unterminated quoted");
      }
      private string ReadIdentifier()
      {
         var sb = new StringBuilder();
         if (!IsLetter())
         {
            throw new Exception("Invalid identifier start char");
         }
         Take(sb);
         while (IsLetterOrDigit())
         {
            Take(sb);
         }
         return sb.ToString();
      }
      private void ReadUntilEndOfLine(StringBuilder sb)
      {
         if (sb != null)
         {
            while (!Eof() && Peek() != '\n' && Peek() != '\r')
            {
               Take(sb);
            }
            return;
         }
         while (!Eof() && Peek() != '\n')
         {
            Next();
         }
      }
      private string ReadUntilSpace()
      {
         var sb = new StringBuilder();
         while (!Eof() && !IsSpace(Peek()))
         {
            switch (Peek())
            {
               case '\'':
               case '"':
               case '`':
               case '[':
                  var right = Peek() == '[' ? ']' : Peek();
                  sb.Append(ReadQuoted(Peek(), right));
                  continue;
            }
            Take(sb);
         }
         return sb.ToString();
      }
      private void Expect(string identifier)
      {
         if (ReadIdentifier().ToUpper() != identifier)
         {
            throw new Exception("Expected identifier: " + identifier);
         }
      }
      private bool NextIs(string identifier)
      {
         var forward = 0;
         if (identifier.All(ch => char.ToUpper(Peek(forward++)) == ch))
         {
            _pos += forward;
            return true;
         }
         return false;
      }
      private bool SkipAnyOff(params string[] identifiers)
      {
         SkipSpaces();
         if (identifiers.Any(NextIs))
         {
            SkipSpaces();
            return true;
         }
         return false;
      }
      private void SkipTop()
      {
         SkipSpaces();
         if (NextIs("TOP"))
         {
            // read any argument
            SkipSpaces();
            while (!Eof() && !IsSpace())
            {
               Next();
            }
            SkipSpaces();
            if (NextIs("PERCENT"))
            {
               SkipSpaces();
            }
            if (NextIs("WITH"))
            {
               SkipSpaces();
               Expect("WITH");
               SkipSpaces();
            }
         }
      }
      private void SkipSpaces()
      {
         while (IsSpace())
         {
            Next();
         }
      }
      private bool IsLetter()
      {
         char ch;
         return char.IsLetter(ch=Peek()) || ch == '_';
      }
      private bool IsLetterOrDigit()
      {
         return char.IsLetterOrDigit(Peek());
      }
      private bool IsSpace()
      {
         return IsSpace(Peek());
      }
      private bool IsSpace(char ch)
      {
         return Spaces.Contains(ch);
      }
      private void Take(StringBuilder sb)
      {
         sb.Append(Next());
      }
      private char Next()
      {
         return Eof() ? '\0' : _source[_pos++];
      }
      private char Peek(int forward = 0)
      {
         return Eof(forward) ? '\0' : _source[_pos + forward];
      }
      private bool Eof(int forward = 0)
      {
         return (_pos+forward) >= _source.Length;
      }
   }
}
