// ReSharper disable InconsistentNaming
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XAdo.Parser
{
   public class SqlParser
   {
      public class Sql
      {
         public const string
            SELECT = "SELECT",
            DISTINCT = "DISTINCT",
            TOP = "TOP",
            AS = "AS",
            FROM = "FROM",
            INNER = "INNER",
            OUTER = "OUTER",
            LEFT = "LEFT",
            RIGHT = "RIGHT",
            JOIN = "JOIN",
            ON = "ON",
            WHERE = "WHERE",
            AND = "AND",
            OR = "OR",
            GROUP = "GROUP",
            BY = "BY",
            HAVING = "HAVING",
            ORDER = "ORDER",
            EQUAL = "=",
            NOTEQUAL = "<>",
            LESSTHAN = "<",
            LESSTHANOREQUAL = "<=",
            GREATERTHAN = ">",
            GREATERTHANOREQUAL = ">=",
            NOT = "NOT",
            IS = "IS",
            NULL = "NULL";
      }


      public void ParseSelect()
      {
         //var selectOptions = ReadSelectOptions();
         //var columns = ReadColums();
         //var from = ReadFromClause();
         //var whereClause = ReadWhereClause();
         //var groupClause = ReadGroupClause();
         //var havingClause = ReadHavingClause();
         //var order = ReadOrderClause();
      }

      public void ParseSelectoptions()
      {
         
      }
   }

   public class SqlTokenizer
   {
      private static HashSet<char> Spaces = new HashSet<char>(new[] { ' ', '\t', '\n', '\r' });

      private int _pos;
      private string _source;


      public SqlTokenizer(string sql)
      {
         _source = sql;
      }

      public string ReadIdentifier()
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
      public string ReadParenthesed()
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
            sb.Append(ch = NextChar());
            if (ch == '(')
            {
               count++;
            }
            else if (ch == ')')
            {
               count--;
            }
            if (count == 0)
            {
               return sb.ToString();
            }
         }
         throw new Exception("Unterminated parenthese");
      }
      public string ReadQuoted()
      {
         var left = Peek();
         var right = left == '[' ? ']' : left;
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
         throw new Exception("Unterminated quoted");
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
            throw new Exception("Expected identifier: " + identifier);
         }
      }
      public bool NextIs(string identifier, bool proceed = true)
      {
         var forward = 0;
         if (identifier.All(ch => char.ToUpper(Peek(forward++)) == ch))
         {
            if(proceed)
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

      private bool NextIsAnyOf(bool proceed, params string[] identifiers)
      {
         SkipSpaces();
         if (identifiers.Any(i => NextIs(i, proceed)))
         {
            if (proceed)
               SkipSpaces();
            return true;
         }
         return false;
      }

      private bool IsLetter()
      {
         char ch;
         return char.IsLetter(ch = Peek()) || ch == '_';
      }
      private bool IsLetterOrDigit()
      {
         char ch;
         return char.IsLetterOrDigit(ch = Peek()) || ch == '_';
      }
      private bool IsLParen(char ch)
      {
         return ch == '(';
      }
      private bool IsLParen()
      {
         return IsLParen(Peek());
      }
      private bool IsSpace()
      {
         return IsSpace(Peek());
      }
      private bool IsSpace(char ch)
      {
         return Spaces.Contains(ch);
      }
      private bool IsQuote()
      {
         return IsQuote(Peek());
      }
      private bool IsQuote(char ch)
      {
         return "'\"`[]".Contains(ch);
      }
      private void Take(StringBuilder sb)
      {
         sb.Append(NextChar());
      }
      private char NextChar()
      {
         return Eof() ? '\0' : _source[_pos++];
      }
      private char Peek(int forward = 0)
      {
         return Eof(forward) ? '\0' : _source[_pos + forward];
      }
      private bool Eof(int forward = 0)
      {
         return (_pos + forward) >= _source.Length;
      }

   }
}
