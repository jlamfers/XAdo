using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Sql.Parser.Parser
{
   public class Scanner
   {
      private static readonly HashSet<char> 
         Spaces = new HashSet<char>(new[] { ' ', '\t', '\n', '\r' });

      public static readonly IDictionary<char, char> 
         Quotes =
         new ReadOnlyDictionary<char, char>(new Dictionary<char, char>
         {
            {'\'','\''},
            {'"','"'},
            {'`','`'},
            {'[',']'}
         });


      private int _pos;
      private readonly string _source;


      public Scanner(string sql)
      {
         _source = sql;
      }

      public string ReadAll()
      {
         var pos = _pos;
         _pos = _source.Length;
         return _source.Substring(pos);
      }
      public string ReadIdentifier()
      {
         var sb = new StringBuilder();
         if (!IsLetter())
         {
            throw new SqlParserException(_source, _pos, "Invalid identifier start char: " + Peek());
         }
         Take(sb);
         while (IsLetterOrDigit())
         {
            Take(sb);
         }
         return sb.ToString();
      }
      public string ReadParenthesed()
      {
         var sb = new StringBuilder();
         if (!IsLParen())
         {
            throw new SqlParserException(_source,_pos, "Expected LParen");
         }
         Take(sb);
         var count = 1;
         while (!Eof())
         {
            if (IsStartQuote())
            {
               sb.Append(ReadQuoted());
               continue;
            }
            char ch;
            sb.Append(ch = NextChar());
            if (IsLParen(ch))
            {
               count++;
            }
            else if (IsRParen(ch))
            {
               count--;
            }
            if (count == 0)
            {
               return sb.ToString();
            }
         }
         throw new SqlParserException(_source, _pos, "Unexpected EOF: Missing RParen");
      }
      public string ReadQuoted()
      {
         var left = Peek();
         char right;
         if (!Quotes.TryGetValue(left, out right))
         {
            throw new SqlParserException(_source, _pos, "Quote character expected");
         }
         var sb = new StringBuilder();
         Take(sb);
         while (!Eof())
         {
            char ch;
            sb.Append(ch = NextChar());
            if (ch == right)
            {
               if (Peek() != right)
               {
                  return sb.ToString();
               }
               Take(sb);
            }
         }
         throw new SqlParserException(_source, _pos, "Unexpected EOF: unterminated quoted, expected: " + right);
      }
      public int ReadInt()
      {
         var sb = new StringBuilder();
         while (char.IsDigit(Peek()))
         {
            Take(sb);
         }
         try
         {
            return int.Parse(sb.ToString());
         }
         catch (Exception ex)
         {
            throw new SqlParserException(_source, _pos, "Invalid int value",ex);
         }
      }
      public string ReadAnyUntilSpace()
      {
         var sb = new StringBuilder();
         while (!Eof() && !IsSpace())
         {
            Take(sb);
         }
         return sb.ToString();
      }
      public void SkipSpaces()
      {
         while (IsSpace())
         {
            NextChar();
         }
      }
      public void Expect(string identifier)
      {
         if (ReadIdentifier().ToUpper() != identifier)
         {
            throw new SqlParserException(_source, _pos, "Expected identifier: " + identifier.ToUpper());
         }
      }
      public bool NextIs(string identifier, bool proceed = true)
      {
         var forward = 0;
         if (identifier.All(ch => char.ToUpper(Peek(forward++)) == ch))
         {
            if (proceed)
               _pos += forward;
            return true;
         }
         return false;
      }
      public bool ReadAnyOf(params string[] identifiers)
      {
         return NextIsAnyOf(true, identifiers);
      }
      public bool PeekAnyOf(params string[] identifiers)
      {
         return NextIsAnyOf(false, identifiers);
      }
      public bool PeekAnyOf(params char[] chrs)
      {
         return chrs.Contains(Peek());
      }

      public bool NextIsAnyOf(bool proceed, params string[] identifiers)
      {
         if (identifiers.Any(i => NextIs(i, proceed)))
         {
            return true;
         }
         return false;
      }
      public bool IsLetter()
      {
         char ch;
         return char.IsLetter(ch = Peek()) || ch == '_';
      }
      public bool IsLetterOrDigit()
      {
         char ch;
         return char.IsLetterOrDigit(ch = Peek()) || ch == '_';
      }
      public bool IsRParen(char ch)
      {
         return ch == ')';
      }
      public bool IsRParen()
      {
         return IsRParen(Peek());
      }
      public bool IsLParen(char ch)
      {
         return ch == '(';
      }
      public bool IsLParen()
      {
         return IsLParen(Peek());
      }
      public bool IsSpace()
      {
         return IsSpace(Peek());
      }
      public bool IsSpace(char ch)
      {
         return Spaces.Contains(ch);
      }
      public bool IsStartQuote()
      {
         return IsStartQuote(Peek());
      }
      public bool IsStartQuote(char ch)
      {
         return Quotes.ContainsKey(ch);
      }
      public void Take(StringBuilder sb)
      {
         sb.Append(NextChar());
      }
      public char NextChar()
      {
         return Eof() ? '\0' : _source[_pos++];
      }
      public char Peek(int forward = 0)
      {
         return Eof(forward) ? '\0' : _source[_pos + forward];
      }
      public bool Eof(int forward = 0)
      {
         return (_pos + forward) >= _source.Length;
      }

      public int Position { get { return _pos; } }
      public string Source { get { return _source; } }

      public string ClearBlockComments()
      {
         if (_source == null || !_source.Contains("/*")) return _source;
         var sb = new StringBuilder();
         var level = 0;
         while (!Eof())
         {
            var ch = NextChar();
            if (ch == '/' && Peek() == '*')
            {
               _pos++;
               level++;
               continue;
            }
            if (ch == '*' && Peek() == '/')
            {
               _pos++;
               level--;
               continue;
            }
            if (level > 0)
            {
               continue;
            }
            sb.Append(ch);
         }
         return sb.ToString();
      }

      public override string ToString()
      {
         return "..."+_source.Substring(_pos);
      }
   }

}
