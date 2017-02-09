using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XAdo.UnitTest
{
   /// <summary>
   /// Rough SELECT parser. It only finds th select columns and corresponding aliases.
   /// </summary>
   public class SelectParser
   {
      private static HashSet<char> Spaces = new HashSet<char>(new []{' ','\t','\n','\r'});
      private string _source;
      private int _pos;

      public List<Tuple<string, string>> ParseSelectColumns(string source)
      {
         _pos = 0;
         _source = source;
         var result = new List<Tuple<string, string>>();
         StringBuilder 
            expr=new StringBuilder(), 
            alias = new StringBuilder();
         var readingExpr = true;
         _pos = _source.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
         Expect("SELECT");
         SkipAnyOff("ALL", "DISTINCT", "DISTINCTROW");
         SkipTop();
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
                  result.Add(Tuple.Create(expr.ToString().Trim(),alias.ToString().Trim()));
                  expr.Length = 0;
                  alias.Length = 0;
                  readingExpr = true;
                  break;
               default:
                  if (NextIs("--"))
                  {
                     // skip the comments
                     ReadUntilEndOfLine();
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
                           if (!result.Any() && expr[0] == '*')
                           {
                              // probe next SELECT
                              return ParseSelectColumns(_source.Substring(_pos));
                           }
                           result.Add(Tuple.Create(expr.ToString().Trim(), alias.ToString().Trim()));
                        }
                        return result;
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
      private void ReadUntilEndOfLine()
      {
         while (!Eof() && Peek() != '\n')
         {
            Next();
         }
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
      private void SkipAnyOff(params string[] identifiers)
      {
         SkipSpaces();
         if (identifiers.Any(NextIs))
         {
            SkipSpaces();
         }
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
