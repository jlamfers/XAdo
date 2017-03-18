using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;

namespace XAdo.DbSchema
{
   public class DbProviderInfo : IDbProviderFormatInfo
   {

      public virtual DbProviderInfo Initialize(DbConnection cn)
      {
         if (cn == null) throw new ArgumentNullException("cn");
         var opened = false;
         if (cn.State != ConnectionState.Open)
         {
            cn.Close();
            cn.Open();
            opened = true;
         }
         var dataTable = cn.GetSchema(DbMetaDataCollectionNames.DataSourceInformation);
         var row = dataTable.Rows[0];
         foreach (DataColumn column in dataTable.Columns)
         {
            var columnName = column.ColumnName;
            var value = row[column.Ordinal];

            if (string.IsNullOrEmpty(columnName) || value == null || value == DBNull.Value)
            {
               continue;
            }

            switch (columnName)
            {
               case "QuotedIdentifierCase":
                  QuotedIdentifierCase = (int) value;
                  break;
               case "GroupByBehavior":
                  GroupByBehavior = (GroupByBehavior) (int) value;
                  break;
               case "IdentifierCase":
                  IdentifierCase = (IdentifierCase) (int) value;
                  break;
               case "SupportedJoinOperators":
                  SupportedJoinOperators = (SupportedJoinOperators) (int) value;
                  break;
               case "DataSourceProductVersion":
                  DataSourceProductVersion = value.ToString();
                  break;
               case "IdentifierPattern":
                  IdentifierPattern = value.ToString();
                  break;
               case "OrderByColumnsInSelect":
                  OrderByColumnsInSelect = (bool) value;
                  break;
               case "ParameterMarkerFormat":
                  ParameterMarkerFormat = value.ToString();
                  break;
               case "ParameterMarkerPattern":
                  ParameterMarkerPattern = value.ToString();
                  break;
               case "CompositeIdentifierSeparatorPattern":
                  CompositeIdentifierSeparatorPattern = value.ToString();
                  break;
               case "DataSourceProductName":
                  DataSourceProductName = value.ToString();
                  break;
               case "DataSourceProductVersionNormalized":
                  DataSourceProductVersionNormalized = value.ToString();
                  break;
               case "ParameterNameMaxLength":
                  ParameterNameMaxLength = (int) value;
                  break;
               case "ParameterNamePattern":
                  ParameterNamePattern = value.ToString();
                  break;
               case "QuotedIdentifierPattern":
                  QuotedIdentifierPattern = value.ToString();
                  break;
               case "StatementSeparatorPattern":
                  StatementSeparatorPattern = value.ToString();
                  break;
               case "StringLiteralPattern":
                  StringLiteralPattern = value.ToString();
                  break;
            }
         }
         if (opened)
         {
            cn.Close();
         }
         return EnsureInitialized(ParameterFormat, QuotedIdentifierFormat, QuotedStringFormat);
      }

      private DbProviderInfo EnsureInitialized(params object[] args)
      {
         return this;
      }

      public virtual int QuotedIdentifierCase { get; private set; }
      public virtual GroupByBehavior GroupByBehavior { get; private set; }
      public virtual IdentifierCase IdentifierCase { get; private set; }
      public virtual SupportedJoinOperators SupportedJoinOperators { get; private set; }
      public virtual string DataSourceProductVersion { get; private set; }
      public virtual string IdentifierPattern { get; private set; }
      public virtual bool OrderByColumnsInSelect { get; private set; }
      public virtual string ParameterMarkerFormat { get; private set; }
      public virtual string ParameterMarkerPattern { get; private set; }
      public virtual string CompositeIdentifierSeparatorPattern { get; private set; }
      public virtual string DataSourceProductName { get; private set; }
      public virtual string DataSourceProductVersionNormalized { get; private set; }
      public virtual int ParameterNameMaxLength { get; private set; }
      public virtual string ParameterNamePattern { get; private set; }
      public virtual string QuotedIdentifierPattern { get; private set; }
      public virtual string StatementSeparatorPattern { get; private set; }
      public virtual string StringLiteralPattern { get; private set; }

      #region IDbProviderFormatInfo
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _quotedIdentifierFormat;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _quotedStringFormat;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _parameterFormat;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _compositeIdentifierSeparator;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _statementsperator;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _identifierQuoteLeft;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _identifierQuoteRight;
      [DebuggerBrowsable(DebuggerBrowsableState.Never)]
      private string _stringQuote;

      public virtual string QuoteIdentifier(string identifier)
      {
         if (identifier == null) return null;
         if (IdentifierQuoteEscape != null)
         {
            identifier = identifier.Replace(IdentifierQuoteRight, IdentifierQuoteEscape);
         }
         return string.Format(QuotedIdentifierFormat, identifier);
      }
      public virtual string QuoteStringLiteral(string literal)
      {
         if (literal == null) return null;
         if (StringQuoteEscape != null)
         {
            literal = literal.Replace(StringQuote, StringQuoteEscape);
         }
         return string.Format(QuotedStringFormat, literal);
      }
      public virtual string QuoteIdentifier(IEnumerable<string> parts)
      {
         return string.Join(MultiPartIdentifierSeparator, parts.Select(QuoteIdentifier));
      }

      public virtual bool IsQuotedString(string value)
      {
         return value != null 
            && Regex.IsMatch(value.Trim(), "^" + StringLiteralPattern + "$", RegexOptions.Compiled);
      }
      public virtual bool IsQuotedIdentifier(string value)
      {
         return value != null 
            && Regex.IsMatch(value.Trim(), "^" + IdentifierPattern + "$", RegexOptions.Compiled);
      }

      public virtual string ParameterFormat
      {
         get
         {
            if (_parameterFormat == null)
            {
               if (!string.IsNullOrEmpty(ParameterMarkerFormat) && ParameterMarkerFormat != "{0}")
               {
                  return _parameterFormat = ParameterMarkerFormat;
               }
               _parameterFormat = ParameterMarkerPattern;
               if (_parameterFormat[0] == '^')
               {
                  _parameterFormat = _parameterFormat.Substring(1);
               }
               while (_parameterFormat[0] == '\\' )
               {
                  _parameterFormat = _parameterFormat.Substring(1);
               }
               _parameterFormat = _parameterFormat.Substring(0, 1)+"{0}";
            }
            return _parameterFormat;
         }
      }
      public virtual string QuotedIdentifierFormat
      {
         get
         {
            if (_quotedIdentifierFormat == null)
            {
               var probes = new[] { "[a]", "`a`", "\"a\"", "'a'" };
               var match = probes.FirstOrDefault(p => Regex.IsMatch(p, QuotedIdentifierPattern)) ?? "a";
               _quotedIdentifierFormat = match.Replace("a", "{0}");
               if (match == "{0}") return match;

               var last = _quotedIdentifierFormat.Substring(_quotedIdentifierFormat.Length-1);
               var probe = string.Format(_quotedIdentifierFormat, "a"+last + last);
               if (Regex.IsMatch(probe, "^"+QuotedIdentifierPattern+"$"))
               {
                  IdentifierQuoteEscape = last + last;
               }
               else
               {
                  IdentifierQuoteEscape = "\\" + last;
               }
            }
            return _quotedIdentifierFormat;
         }
      }
      public virtual string QuotedStringFormat
      {
         get
         {
            if (_quotedStringFormat == null)
            {
               var probes = new[] { "\"a\"", "'a'" };
               var match = probes.FirstOrDefault(p => Regex.IsMatch(p, StringLiteralPattern)) ?? "a";
               _quotedStringFormat = match.Replace("a", "{0}");
               if (match == "{0}") return match;

               var first = _quotedStringFormat.Substring(0,1);
               var probe = string.Format(_quotedStringFormat, first + first);
               if (Regex.IsMatch(probe, "^" + StringLiteralPattern + "$"))
               {
                  StringQuoteEscape = first + first;
               }
               else
               {
                  StringQuoteEscape = "\\" + first;
               }
            }
            return _quotedStringFormat;
         }
      }

      public virtual string IdentifierQuoteLeft
      {
         get
         {
            if (_identifierQuoteLeft != null) return _identifierQuoteLeft;
            var left = QuotedIdentifierFormat.First();
            return _identifierQuoteLeft = (left == '{' ? "" : QuotedIdentifierFormat.Substring(0, 1));
         }
      }
      public virtual string IdentifierQuoteRight
      {
         get
         {
            if (_identifierQuoteRight != null) return _identifierQuoteRight;
            var right = QuotedIdentifierFormat.Last();
            return _identifierQuoteRight = (right == '}' ? "" : QuotedIdentifierFormat.Substring(QuotedIdentifierFormat.Length - 1));
         }
      }
      public virtual string StringQuote
      {
         get
         {
            if (_stringQuote != null) return _stringQuote;
            var left = QuotedStringFormat.First();
            return _stringQuote = (left == '{' ? "" : QuotedStringFormat.Substring(0, 1));
         }
      }
     
      public virtual string StatementSeparator
      {
         get
         {
            if (_statementsperator != null) 
               return _statementsperator;

            if (string.IsNullOrEmpty(StatementSeparatorPattern)) 
               return _statementsperator=Environment.NewLine;

            var statementsperator = StatementSeparatorPattern;
            if (statementsperator.First() == '\\')
            {
               statementsperator = statementsperator.Substring(1);
            }
            statementsperator = statementsperator.Replace("\\\\", "\\");
            statementsperator = statementsperator.Replace("\\n", "\n");
            statementsperator = statementsperator.Replace("\\r", "\r");
            return _statementsperator = statementsperator;
         }
      }
      public virtual string MultiPartIdentifierSeparator
      {
         get
         {
            if (_compositeIdentifierSeparator == null)
            {
               _compositeIdentifierSeparator = CompositeIdentifierSeparatorPattern ?? ".";
               if (_compositeIdentifierSeparator.First() == '\\')
               {
                  _compositeIdentifierSeparator = _compositeIdentifierSeparator.Substring(1);
               }
            }
            return _compositeIdentifierSeparator;
         }
      }

      public virtual string IdentifierQuoteEscape { get; protected set; }
      public virtual string StringQuoteEscape { get; protected set; }
      #endregion

   }
}
