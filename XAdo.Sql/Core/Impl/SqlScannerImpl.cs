using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser;

namespace XAdo.Quobs.Core.Impl
{
   public class SqlScannerImpl : ISqlScanner
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
      private string _source;

      public virtual ISqlScanner Initialize(string sql)
      {
         return new SqlScannerImpl
         {
            _source = sql
         };
      }

      public virtual string ReadAll()
      {
         var pos = _pos;
         _pos = _source.Length;
         return _source.Substring(pos);
      }
      public virtual string ReadIdentifier(ICollection<char> includes = null)
      {
         var sb = new StringBuilder();
         if (!this.IsLetter())
         {
            throw new SqlParserException(_source, _pos, "Invalid identifier start char: " + Peek());
         }
         Take(sb);
         while (this.IsLetterOrDigit() || (includes != null && includes.Contains(Peek())))
         {
            Take(sb);
         }
         return sb.ToString();
      }
      public virtual string ReadParenthesed()
      {
         var sb = new StringBuilder();
         if (!this.IsLParen())
         {
            throw new SqlParserException(_source,_pos, "Expected LParen");
         }
         Take(sb);
         var count = 1;
         while (!Eof())
         {
            if (this.IsStartQuote())
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
      public virtual string ReadQuoted()
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
      public virtual int ReadInt()
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
      public virtual string ReadAnyUntilSpace()
      {
         var sb = new StringBuilder();
         while (!Eof() && !this.IsSpace())
         {
            Take(sb);
         }
         return sb.ToString();
      }
      public virtual void SkipSpaces()
      {
         while (this.IsSpace())
         {
            NextChar();
         }
      }
      public virtual void Expect(string identifier)
      {
         if (ReadIdentifier().ToUpper() != identifier)
         {
            throw new SqlParserException(_source, _pos, "Expected identifier: " + identifier.ToUpper());
         }
      }
      public virtual bool NextIs(string identifier, bool proceed = true)
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
      public virtual bool ReadAnyOf(params string[] identifiers)
      {
         return NextIsAnyOf(true, identifiers);
      }
      public virtual bool PeekAnyOf(params string[] identifiers)
      {
         return NextIsAnyOf(false, identifiers);
      }
      public virtual bool PeekAnyOf(params char[] chrs)
      {
         return chrs.Contains(Peek());
      }

      public virtual bool NextIsAnyOf(bool proceed, params string[] identifiers)
      {
         if (identifiers.Any(i => NextIs(i, proceed)))
         {
            return true;
         }
         return false;
      }

      public virtual bool IsLetter(char ch)
      {
         return char.IsLetter(ch) || ch == '_';
      }

      public virtual bool IsLetterOrDigit(char ch)
      {
         return char.IsLetterOrDigit(ch) || ch == '_';
      }

      public virtual bool IsRParen(char ch)
      {
         return ch == ')';
      }
      public virtual bool IsLParen(char ch)
      {
         return ch == '(';
      }
      public virtual bool IsSpace(char ch)
      {
         return Spaces.Contains(ch);
      }
      public virtual bool IsStartQuote(char ch)
      {
         return Quotes.ContainsKey(ch);
      }
      public virtual void Take(StringBuilder sb)
      {
         sb.Append(NextChar());
      }
      public virtual char NextChar()
      {
         return Eof() ? '\0' : _source[_pos++];
      }
      public virtual char Peek(int forward = 0)
      {
         return Eof(forward) ? '\0' : _source[_pos + forward];
      }
      public virtual bool Eof(int forward = 0)
      {
         return (_pos + forward) >= _source.Length;
      }

      public virtual int Position { get { return _pos; } }
      public virtual string Source { get { return _source; } }

      public virtual string ClearBlockComments()
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