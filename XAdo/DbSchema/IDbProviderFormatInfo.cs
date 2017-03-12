using System.Collections.Generic;

namespace XAdo.DbSchema
{
   public interface IDbProviderFormatInfo
   {
      string ParameterFormat { get; }
      string QuotedIdentifierFormat { get; }
      string QuotedStringFormat { get; }
      string IdentifierQuoteLeft { get; }
      string IdentifierQuoteRight { get; }
      string IdentifierQuoteEscape { get; }
      string StringQuote { get; }
      string StringQuoteEscape { get; }
      string StatementSeparator { get; }
      string MultiPartIdentifierSeparator { get; }

      string QuoteIdentifier(string identifier);
      string QuoteStringLiteral(string literal);
      string QuoteIdentifier(IEnumerable<string> parts);

      bool IsQuotedString(string value);
      bool IsQuotedIdentifier(string value);
   }
}