using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using XAdo.Quobs.Core.Interface;
using XAdo.Quobs.Core.Parser;
using XAdo.Quobs.Core.Parser.Partials;

// ReSharper disable InconsistentNaming


namespace XAdo.Quobs.Core.Impl
{
   public class SqlSelectParserImpl : ISqlSelectParser
   {
      private readonly ISqlScanner _scanner;

      public static class SQL
      {
         #region Keywords
         public const string

            WITH = "WITH",
            AS = "AS",
            SELECT = "SELECT",
            DISTINCT = "DISTINCT",
            TOP = "TOP",
            INTO = "INTO",
            FROM = "FROM",
            LINECOMMENT = "--",

            INNER = "INNER",
            OUTER = "OUTER",
            LEFT = "LEFT",
            RIGHT = "RIGHT",
            FULL = "RIGHT",
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
            TEMPLATE1 = "--$",
            TEMPLATE2 = "-- $",
            TAG1 = "-->",
            TAG2 = "-- >",
            ASC = "ASC",
            DESC = "DESC";
         #endregion

      }

      public SqlSelectParserImpl(ISqlScanner scanner)
      {
         _scanner = scanner;
      }

      public IList<SqlPartial> Parse(string sql)
      {
         if (sql == null) throw new ArgumentNullException("sql");
        var scanner = _scanner.Initialize(sql);
         scanner.ClearBlockComments();
         var partials = new List<SqlPartial>();
         while (!scanner.Eof())
         {
            scanner.SkipSpaces();

            if (scanner.NextIs(SQL.TEMPLATE1) || scanner.NextIs(SQL.TEMPLATE2))
            {
               partials.Add(new TemplatePartial(ReadLineComment(scanner)));
               continue;
            }

            if(scanner.NextIs(SQL.LINECOMMENT))
            {
               ReadLineComment(scanner);
               continue;
            }

            if (scanner.NextIs(SQL.WITH))
            {
               partials.Add(ReadWith(scanner));
               continue;
            }

            if (scanner.NextIs(SQL.SELECT))
            {
               partials.Add(ReadSelect(scanner));
               continue;
            }

            if (scanner.NextIs(SQL.INTO))
            {
               // no need to handle
               throw new SqlParserException(scanner.Source,scanner.Position,"INTO is not supported in select parser");
            }

            if (scanner.NextIs(SQL.FROM))
            {
               partials.Add(new FromTablePartial(ReadTable(scanner)));
               continue;
            }

            if (scanner.PeekAnyOf(SQL.INNER, SQL.LEFT, SQL.RIGHT, SQL.FULL, SQL.JOIN))
            {
               partials.Add(ReadJoin(scanner));
               continue;
            }
            if (scanner.NextIs(SQL.WHERE))
            {
               partials.Add(ReadWhere(scanner));
               continue;
            }
            if (scanner.NextIs(SQL.GROUP))
            {
               partials.Add(ReadGroupBy(scanner));
               continue;
            }
            if (scanner.NextIs(SQL.HAVING))
            {
               partials.Add(ReadHaving(scanner));
               continue;
            }
            if (scanner.NextIs(SQL.ORDER))
            {
               partials.Add(ReadOrderBy(scanner));
               continue;
            }
            var partial = new SqlPartial(scanner.ReadAll());
            if (partial.Expression.Length > 0)
            {
               partials.Add(partial);
            }

         }

         return partials.Where(p => !string.IsNullOrWhiteSpace(p.Expression)).ToList();
      }

      // where clause only can have one template placolder, at the end
      protected virtual WithPartial ReadWith(ISqlScanner scanner)
      {
         SkipComments(scanner);
         var alias = ReadAlias(scanner,null);
         if (string.IsNullOrEmpty(alias))
         {
            throw new SqlParserException(scanner.Source, scanner.Position, "alias expected");
         }
         SkipComments(scanner);
         scanner.Expect(SQL.AS);
         SkipComments(scanner);
         var expression = scanner.ReadParenthesed();
         return new WithPartial(expression, alias);

      }
      protected virtual SelectPartial ReadSelect(ISqlScanner scanner)
      {
         var distinct = false;
         var selectChilds = new List<ColumnPartial>();
         int? maxRows = null;

         SkipComments(scanner);
         if (scanner.NextIs(SQL.DISTINCT))
         {
            distinct = true;
            SkipComments(scanner);
         }

         if (scanner.NextIs(SQL.TOP))
         {
            SkipComments(scanner);
            maxRows = scanner.ReadInt();
         }

         while (true)
         {
            SkipComments(scanner);

            if (scanner.Eof())
            {
               throw new SqlParserException(scanner.Source,scanner.Position,"Unexpected eof");
            }

            if (scanner.Peek() == '*')
            {
               selectChilds.Add(new ColumnPartial(new[] {"*"}, null, null));
               scanner.NextChar();
               //throw new SqlParserException(scanner.Source, scanner.Position, "wildcards in select are not allowed, these cannot be mapped");
            }
            else
            {
               selectChilds.Add(ReadColumn(scanner));
            }
            SkipComments(scanner);

            if (scanner.Peek() == ',')
            {
               scanner.NextChar();
               continue;
            }

            if (!scanner.PeekAnyOf(SQL.FROM))
            {
               throw new SqlParserException(scanner.Source, scanner.Position, "'{0}' expected".FormatWith(SQL.FROM));
            }

            return new SelectPartial(distinct, selectChilds,maxRows,false);

         }

      }
      protected virtual TablePartial ReadTable(ISqlScanner scanner)
      {
         SkipComments(scanner);
         var parts = new List<string>();
         while (true)
         {
            parts.Add(scanner.IsLParen() ?
                  scanner.ReadParenthesed()
                  : (scanner.IsStartQuote() ?
                     scanner.ReadQuoted()
                     : scanner.ReadIdentifier(null)));

            if (scanner.Peek() == Constants.Syntax.Chars.COLUMN_SEP)
            {
               scanner.NextChar();
               continue;
            }
            scanner.SkipSpaces();

            string alias = null;
            if (scanner.NextIs(SQL.AS))
            {
               alias = ReadAlias(scanner,null);
               scanner.SkipSpaces();
            }
            string tag = null;
            if (scanner.NextIs(SQL.TAG1) || scanner.NextIs(SQL.TAG2))
            {
               tag = ReadLineComment(scanner);
               scanner.SkipSpaces();
            }
            return new TablePartial(parts, alias,tag);
         }
      }
      protected virtual JoinPartial ReadJoin(ISqlScanner scanner)
      {
         var joinType = JoinType.Inner;

         SkipComments(scanner);
         if (scanner.NextIs(SQL.INNER) || scanner.NextIs(SQL.JOIN))
         {
            joinType = JoinType.Inner;
         }
         else if (scanner.NextIs(SQL.LEFT))
         {
            joinType = JoinType.Left;
         }
         else if (scanner.NextIs(SQL.RIGHT))
         {
            joinType = JoinType.Right;
         }
         else if (scanner.NextIs(SQL.FULL))
         {
            joinType = JoinType.Full;
         }
         else
         {
            throw new SqlParserException(scanner.Source,scanner.Position,"Unexpected token: " + scanner.NextChar());
         }
         SkipComments(scanner);
         while (scanner.NextIsAnyOf(true, SQL.OUTER, SQL.JOIN))
         {
            SkipComments(scanner);
         }
         var table = ReadTable(scanner);
         SkipComments(scanner);

         scanner.Expect(SQL.ON);
         SkipComments(scanner);

         var expression = new StringBuilder();
         while (!scanner.Eof() && !scanner.PeekAnyOf(SQL.INNER, SQL.LEFT, SQL.RIGHT, SQL.FULL, SQL.JOIN, SQL.WHERE, SQL.GROUP, SQL.ORDER, SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            if (scanner.NextIs(SQL.TAG1) || scanner.NextIs(SQL.TAG2))
            {
               var tag = ReadLineComment(scanner);
               table.Tag = tag;
               scanner.NextChar();
               scanner.NextChar();
               expression.Append(" ");
               continue;
            }

            if (scanner.NextIs(SQL.LINECOMMENT))
            {
               ReadLineComment(scanner);
               scanner.NextChar();
               scanner.NextChar();
               expression.Append(" ");
               continue;
            }
            ReadAnyPartial(scanner,expression);
         }

         var expr = expression.ToString();
         return new JoinPartial(expr, joinType, table, ReadEquiJoinColumns(expr));
      }
      protected virtual List<Tuple<ColumnPartial, ColumnPartial>> ReadEquiJoinColumns(string expression)
      {
         var result = new List<Tuple<ColumnPartial, ColumnPartial>>();
         if (expression.IndexOfAny(new []{ '(',')'}) != -1)
         {
            // parentheses are not accepted (since these are not needed with conjunct equi joins)
            return result;
         }
         var scanner = _scanner.Initialize(expression);
         while (!scanner.Eof())
         {
            scanner.SkipSpaces();
            var c1 = ReadColumn(scanner, false, false);
            scanner.SkipSpaces();
            if (!scanner.PeekAnyOf(SQL.EQUAL))
            {
               // only accepting equi joins 
               result.Clear();
               break;
            }
            scanner.NextChar();
            scanner.SkipSpaces();
            var c2 = ReadColumn(scanner, false, false);
            result.Add(Tuple.Create(c1, c2));
            if (scanner.PeekAnyOf(SQL.OR))
            {
               // only accepting conjunctions
               result.Clear();
               break;
            }
            if (!scanner.Eof())
            {
               scanner.Expect(SQL.AND);
            }
         }
         return result;
      }
      protected virtual WherePartial ReadWhere(ISqlScanner scanner)
      {
         SkipComments(scanner);
         var whereClause = new StringBuilder();
         var template = default(string);
         while (!scanner.Eof() && !scanner.PeekAnyOf(SQL.GROUP, SQL.ORDER, SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            ReadAnyPartial(scanner,whereClause);
         }
         if (scanner.NextIs(SQL.TEMPLATE1) || scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment(scanner);
         }

         return new WherePartial(whereClause.ToString(), template);
      }
      protected virtual GroupByPartial ReadGroupBy(ISqlScanner scanner)
      {
         SkipComments(scanner);
         scanner.Expect(SQL.BY);
         SkipComments(scanner);
         var columns = new List<ColumnPartial>();
         while (!scanner.Eof() && !scanner.PeekAnyOf(SQL.TEMPLATE1, SQL.TEMPLATE2, SQL.HAVING, SQL.ORDER))
         {
            columns.Add(ReadColumn(scanner,false));
            if (scanner.Peek() == ',')
            {
               scanner.NextChar();
            }
            SkipComments(scanner);
         }
         string template = null;
         if (scanner.NextIs(SQL.TEMPLATE1) || scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment(scanner);
         }
         return new GroupByPartial(columns, template);
      }
      protected virtual HavingPartial ReadHaving(ISqlScanner scanner)
      {
         SkipComments(scanner);
         var havingClause = new StringBuilder();
         var template = default(string);
         while (!scanner.Eof() && !scanner.PeekAnyOf(SQL.ORDER, SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            ReadAnyPartial(scanner,havingClause);
         }
         if (scanner.NextIs(SQL.TEMPLATE1) || scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment(scanner);
         }

         return new HavingPartial(havingClause.ToString(), template);
      }
      protected virtual OrderByPartial ReadOrderBy(ISqlScanner scanner)
      {
         SkipComments(scanner);
         scanner.Expect(SQL.BY);
         SkipComments(scanner);
         var columns = new List<OrderColumnPartial>();
         while (!scanner.Eof() && !scanner.PeekAnyOf(SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            var col = ReadColumn(scanner,false);
            var order = SQL.ASC;
            if (scanner.NextIs(SQL.ASC))
            {
               order = SQL.ASC;
               SkipComments(scanner);
            }
            else if (scanner.NextIs(SQL.DESC))
            {
               order = SQL.DESC;
               SkipComments(scanner);
            }
            columns.Add(new OrderColumnPartial(col, order == SQL.DESC));
            if (scanner.Peek() == ',')
            {
               scanner.NextChar();
            }
            SkipComments(scanner);
         }
         string template = null;
         if (scanner.NextIs(SQL.TEMPLATE1) || scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment(scanner);
         }
         return new OrderByPartial(columns, template);
      }


      protected virtual string ReadAlias(ISqlScanner scanner, ICollection<char> specialColumnChars)
      {
         scanner.SkipSpaces();
         return scanner.IsStartQuote() ? scanner.ReadQuoted() : scanner.ReadIdentifier(specialColumnChars);
      }
      protected virtual ColumnPartial ReadColumn(ISqlScanner scanner, bool aliased = true, bool tagged = true)
      {
         var specialColumnChars = aliased ? Constants.Syntax.Chars.TagCharsSet : null;
         SkipComments(scanner);
         var parts = new List<string>();
         while (true)
         {
            parts.Add(scanner.IsLParen() ?
                  scanner.ReadParenthesed()
                  : (scanner.IsStartQuote() ?
                     scanner.ReadQuoted()
                     : scanner.ReadIdentifier(specialColumnChars)));

            if (scanner.Peek() == Constants.Syntax.Chars.COLUMN_SEP)
            {
               scanner.NextChar();
               continue;
            }
            scanner.SkipSpaces();

            string alias = null;
            if (aliased)
            {
               if (scanner.NextIs(SQL.AS))
               {
                  alias = ReadAlias(scanner,specialColumnChars);
                  scanner.SkipSpaces();
               }
            }
            string tag = null;
            if (tagged)
            {
               if (scanner.NextIs(SQL.TAG1) || scanner.NextIs(SQL.TAG2))
               {
                  tag = ReadLineComment(scanner);
               }
            }
            return new ColumnPartial(parts, alias,tag);
         }
      }
      protected virtual string ReadLineComment(ISqlScanner scanner)
      {
         var sb = new StringBuilder();
         while (!scanner.Eof() && !scanner.PeekAnyOf('\r', '\n'))
         {
            scanner.Take(sb);
         }
         return sb.ToString();
      }
      protected virtual void SkipComments(ISqlScanner scanner)
      {
         scanner.SkipSpaces();
         while (true)
         {
            if (scanner.NextIs(SQL.LINECOMMENT))
            {
               ReadLineComment(scanner);
               scanner.SkipSpaces();
               continue;
            }
            return;
         }
      }
      protected virtual void ReadAnyPartial(ISqlScanner scanner, StringBuilder sb)
      {
         if (scanner.IsLParen())
         {
            sb.Append(scanner.ReadParenthesed());
         }
         else if (scanner.IsStartQuote())
         {
            sb.Append(scanner.ReadQuoted());
         }
         else if (scanner.IsLetter())
         {
            sb.Append(scanner.ReadIdentifier());
         }
         else
         {
            sb.Append(scanner.NextChar());
         }
      }
   }
}
