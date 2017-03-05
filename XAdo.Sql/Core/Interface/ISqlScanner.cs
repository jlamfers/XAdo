using System.Collections.Generic;
using System.Text;

namespace XAdo.Quobs.Core.Interface
{
   public interface ISqlScanner
   {
      ISqlScanner Initialize(string sql);
      string ReadAll();
      string ReadIdentifier(ICollection<char> includes = null );
      string ReadParenthesed();
      string ReadQuoted();
      int ReadInt();
      string ReadAnyUntilSpace();
      void SkipSpaces();
      void Expect(string identifier);
      bool NextIs(string identifier, bool proceed = true);
      bool ReadAnyOf(params string[] identifiers);
      bool PeekAnyOf(params string[] identifiers);
      bool PeekAnyOf(params char[] chrs);
      bool NextIsAnyOf(bool proceed, params string[] identifiers);
      bool IsLetter(char ch);
      bool IsLetterOrDigit(char ch);
      bool IsRParen(char ch);
      bool IsLParen(char ch);
      bool IsSpace(char ch);
      bool IsStartQuote(char ch);
      void Take(StringBuilder sb);
      char NextChar();
      char Peek(int forward = 0);
      bool Eof(int forward = 0);
      int Position { get; }
      string Source { get; }
      string ClearBlockComments();
      string ToString();
   }

   public static class SqlScannerExtensions
   {
      public static bool IsLetter(this ISqlScanner self)
      {
         return self.IsLetter(self.Peek());
      }

      public static bool IsLetterOrDigit(this ISqlScanner self)
      {
         return self.IsLetterOrDigit(self.Peek());
      }

      public static bool IsRParen(this ISqlScanner self)
      {
         return self.IsRParen(self.Peek());
      }

      public static bool IsLParen(this ISqlScanner self)
      {
         return self.IsLParen(self.Peek());
      }

      public static bool IsSpace(this ISqlScanner self)
      {
         return self.IsSpace(self.Peek());
      }

      public static bool IsStartQuote(this ISqlScanner self)
      {
         return self.IsStartQuote(self.Peek());
      }

   }
}