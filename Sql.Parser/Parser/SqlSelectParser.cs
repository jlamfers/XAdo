using System;
using System.Collections.Generic;
using System.Text;
using Sql.Parser.Common;
using Sql.Parser.Partials;

// ReSharper disable InconsistentNaming


namespace Sql.Parser.Parser
{
   public class SqlSelectParser
   {
      private Scanner _scanner;

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

      public IList<SqlPartial> Parse(string sql)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         _scanner = new Scanner(new Scanner(sql).ClearBlockComments());
         var partials = new List<SqlPartial>();
         while (!_scanner.Eof())
         {
            _scanner.SkipSpaces();

            if (_scanner.NextIs(SQL.TEMPLATE1) || _scanner.NextIs(SQL.TEMPLATE2))
            {
               partials.Add(new TemplatePartial(ReadLineComment().Expression));
               continue;
            }

            if(_scanner.NextIs(SQL.LINECOMMENT))
            {
               ReadLineComment();
               continue;
            }

            if (_scanner.NextIs(SQL.WITH))
            {
               partials.Add(ReadWith());
               continue;
            }

            if (_scanner.NextIs(SQL.SELECT))
            {
               partials.Add(ReadSelect());
               continue;
            }

            if (_scanner.NextIs(SQL.INTO))
            {
               // no need to handle
               throw new SqlParserException(_scanner.Source,_scanner.Position,"INTO is not supported in select parser");
            }

            if (_scanner.NextIs(SQL.FROM))
            {
               partials.Add(new FromTablePartial(ReadTable()));
               continue;
            }

            if (_scanner.PeekAnyOf(SQL.INNER, SQL.LEFT, SQL.RIGHT, SQL.FULL, SQL.JOIN))
            {
               partials.Add(ReadJoin());
               continue;
            }
            if (_scanner.NextIs(SQL.WHERE))
            {
               partials.Add(ReadWhere());
               continue;
            }
            if (_scanner.NextIs(SQL.GROUP))
            {
               partials.Add(ReadGroupBy());
               continue;
            }
            if (_scanner.NextIs(SQL.HAVING))
            {
               partials.Add(ReadHaving());
               continue;
            }
            if (_scanner.NextIs(SQL.ORDER))
            {
               partials.Add(ReadOrderBy());
               continue;
            }
            var partial = new SqlPartial(_scanner.ReadAll());
            if (partial.Expression.Length > 0)
            {
               partials.Add(partial);
            }

         }

         return partials;
      }

      // where clause only can have one template placolder, at the end
      private WithPartial ReadWith()
      {
         SkipComments();
         var alias = ReadAlias();
         if (string.IsNullOrEmpty(alias))
         {
            throw new SqlParserException(_scanner.Source, _scanner.Position, "alias expected");
         }
         SkipComments();
         _scanner.Expect(SQL.AS);
         SkipComments();
         var expression = _scanner.ReadParenthesed();
         return new WithPartial(expression, alias);

      }
      private SelectPartial ReadSelect()
      {
         var distinct = false;
         var selectChilds = new List<SqlPartial>();

         SkipComments();
         if (_scanner.NextIs(SQL.DISTINCT))
         {
            distinct = true;
            SkipComments();
         }

         if (_scanner.NextIs(SQL.TOP))
         {
            throw new SqlParserException(_scanner.Source, _scanner.Position, "TOP in select is not allowed, use paging instead");
         }

         while (true)
         {
            SkipComments();

            if (_scanner.Eof())
            {
               throw new SqlParserException(_scanner.Source,_scanner.Position,"Unexpected eof");
            }

            if (_scanner.Peek() == '*')
            {
               throw new SqlParserException(_scanner.Source, _scanner.Position, "wildcards in select are not allowed, these cannot be mapped");
            }

            selectChilds.Add(ReadColumn());

            _scanner.SkipSpaces();

            if (!_scanner.PeekAnyOf(",", SQL.TAG1,SQL.TAG2, SQL.FROM))
            {
               if (_scanner.PeekAnyOf(SQL.LINECOMMENT))
               {
                  // allow any comments
                  SkipComments();
               }
               else
               {
                  throw new SqlParserException(_scanner.Source, _scanner.Position,"',',  '{0}' or '{1}' expected".FormatWith(SQL.TAG1, SQL.FROM));
               }
            }

            if (_scanner.Peek() == ',')
            {
               _scanner.NextChar();
               _scanner.SkipSpaces();
               if (_scanner.NextIs(SQL.TAG1) || _scanner.NextIs(SQL.TAG2))
               {
                  selectChilds.Add(new TagPartial(ReadLineComment().Expression));
               }
               continue;
            }

            if (_scanner.NextIs(SQL.TAG1) || _scanner.NextIs(SQL.TAG2))
            {
               selectChilds.Add(new TagPartial(ReadLineComment().Expression));
            }
            SkipComments();
            if (!_scanner.PeekAnyOf(SQL.FROM))
            {
               throw new SqlParserException(_scanner.Source, _scanner.Position, "'{0}' expected".FormatWith(SQL.FROM));
            }

            return new SelectPartial(distinct, selectChilds);

         }

      }
      private TablePartial ReadTable()
      {
         return new TablePartial(ReadMultiPart(true));
      }
      private JoinPartial ReadJoin()
      {
         var joinType = JoinType.Inner;

         SkipComments();
         if (_scanner.NextIs(SQL.INNER) || _scanner.NextIs(SQL.JOIN))
         {
            joinType = JoinType.Inner;
         }
         else if (_scanner.NextIs(SQL.LEFT))
         {
            joinType = JoinType.Left;
         }
         else if (_scanner.NextIs(SQL.RIGHT))
         {
            joinType = JoinType.Right;
         }
         else if (_scanner.NextIs(SQL.FULL))
         {
            joinType = JoinType.Full;
         }
         else
         {
            throw new SqlParserException(_scanner.Source,_scanner.Position,"Unexpected token: " + _scanner.NextChar());
         }
         SkipComments();
         while (_scanner.NextIsAnyOf(true, SQL.OUTER, SQL.JOIN))
         {
            SkipComments();
         }
         var table = ReadTable();
         SkipComments();

         _scanner.Expect(SQL.ON);
         SkipComments();

         var expression = new StringBuilder();
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.INNER, SQL.LEFT, SQL.RIGHT, SQL.FULL, SQL.JOIN, SQL.WHERE, SQL.GROUP, SQL.ORDER, SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            if (_scanner.NextIs(SQL.LINECOMMENT))
            {
               ReadLineComment();
               _scanner.NextChar();
               _scanner.NextChar();
               expression.Append(" ");
               continue;
            }
            ReadAnyPartial(expression);
         }

         return new JoinPartial(expression.ToString(), joinType, table);
      }
      private WherePartial ReadWhere()
      {
         SkipComments();
         var whereClause = new StringBuilder();
         var template = default(string);
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.GROUP, SQL.ORDER, SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            ReadAnyPartial(whereClause);
         }
         if (_scanner.NextIs(SQL.TEMPLATE1) || _scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment().Expression;
         }

         return new WherePartial(whereClause.ToString(), template);
      }
      private GroupByPartial ReadGroupBy()
      {
         SkipComments();
         _scanner.Expect(SQL.BY);
         SkipComments();
         var columns = new List<ColumnPartial>();
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.ORDER, SQL.HAVING))
         {
            columns.Add(ReadColumn(false));
            SkipComments();
            if (_scanner.Peek() == ',')
            {
               _scanner.NextChar();
            }
         }
         return new GroupByPartial(columns);
      }
      private HavingPartial ReadHaving()
      {
         SkipComments();
         var havingClause = new StringBuilder();
         var template = default(string);
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.ORDER, SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            ReadAnyPartial(havingClause);
         }
         if (_scanner.NextIs(SQL.TEMPLATE1) || _scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment().Expression;
         }

         return new HavingPartial(havingClause.ToString(), template);
      }
      private OrderByPartial ReadOrderBy()
      {
         SkipComments();
         _scanner.Expect(SQL.BY);
         SkipComments();
         var columns = new List<OrderColumnPartial>();
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            var col = ReadColumn(false);
            var order = default(string);
            if (_scanner.NextIs(SQL.ASC))
            {
               order = SQL.ASC;
               SkipComments();
            }
            else if (_scanner.NextIs(SQL.DESC))
            {
               order = SQL.ASC;
               SkipComments();
            }
            columns.Add(new OrderColumnPartial(col.RawParts, order));
            if (_scanner.Peek() == ',')
            {
               _scanner.NextChar();
            }
            SkipComments();
         }
         string template = null;
         if (_scanner.NextIs(SQL.TEMPLATE1) || _scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment().Expression;
         }
         return new OrderByPartial(columns, template);
      }


      private string ReadAlias()
      {
         _scanner.SkipSpaces();
         return _scanner.IsStartQuote() ? _scanner.ReadQuoted() : _scanner.ReadIdentifier();
      }
      private ColumnPartial ReadColumn(bool aliased = true)
      {
         return new ColumnPartial(ReadMultiPart(aliased));
      }
      private MultiPartAliasedPartial ReadMultiPart(bool aliased)
      {
         SkipComments();
         var parts = new List<string>();
         while(true)
         {
            parts.Add(_scanner.IsLParen() ? 
                  _scanner.ReadParenthesed() 
                  : (_scanner.IsStartQuote() ? 
                     _scanner.ReadQuoted() 
                     : _scanner.ReadIdentifier()));

            if (_scanner.Peek() == '.')
            {
               _scanner.NextChar();
               continue;
            }

            if (aliased)
            {
               _scanner.SkipSpaces();
               if (_scanner.NextIs(SQL.AS))
               {
                  return new MultiPartAliasedPartial(parts, ReadAlias());
               }
            }
            return new MultiPartAliasedPartial(parts, null);
         }

      }
      private LineCommentPartial ReadLineComment()
      {
         var sb = new StringBuilder();
         while (!_scanner.Eof() && !_scanner.PeekAnyOf('\r', '\n'))
         {
            _scanner.Take(sb);
         }
         return new LineCommentPartial(sb.ToString());
      }
      private void SkipComments()
      {
         _scanner.SkipSpaces();
         while (true)
         {
            if (_scanner.NextIs(SQL.LINECOMMENT))
            {
               ReadLineComment();
               _scanner.SkipSpaces();
               continue;
            }
            return;
         }
      }
      private void ReadAnyPartial(StringBuilder sb)
      {
         if (_scanner.IsLParen())
         {
            sb.Append(_scanner.ReadParenthesed());
         }
         else if (_scanner.IsStartQuote())
         {
            sb.Append(_scanner.ReadQuoted());
         }
         else if (_scanner.IsLetter())
         {
            sb.Append(_scanner.ReadIdentifier());
         }
         else
         {
            sb.Append(_scanner.NextChar());
         }
      }
   }
}
