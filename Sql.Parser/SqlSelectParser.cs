using System.Collections.Generic;
using System.Text;
using Sql.Parser.Tokens;
// ReSharper disable InconsistentNaming


namespace Sql.Parser
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

      public IList<SqlToken> Tokenize(string sql)
      {
         _scanner = new Scanner(new Scanner(sql).ClearBlockComments());
         var tokens = new List<SqlToken>();
         while (!_scanner.Eof())
         {
            _scanner.SkipSpaces();

            if (_scanner.NextIs(SQL.TEMPLATE1) || _scanner.NextIs(SQL.TEMPLATE2))
            {
               tokens.Add(new TemplateToken(ReadLineComment().Expression));
               continue;
            }

            if(_scanner.NextIs(SQL.LINECOMMENT))
            {
               ReadLineComment();
               continue;
            }

            if (_scanner.NextIs(SQL.WITH))
            {
               tokens.Add(ReadWith());
               continue;
            }

            if (_scanner.NextIs(SQL.SELECT))
            {
               tokens.Add(ReadSelect());
               continue;
            }

            if (_scanner.NextIs(SQL.INTO))
            {
               // no need to handle
               throw new SqlParserException(_scanner.Source,_scanner.Position,"INTO is not supported in select parser");
            }

            if (_scanner.NextIs(SQL.FROM))
            {
               tokens.Add(new FromTableToken(ReadTable()));
               continue;
            }

            if (_scanner.PeekAnyOf(SQL.INNER, SQL.LEFT, SQL.RIGHT, SQL.FULL, SQL.JOIN))
            {
               tokens.Add(ReadJoin());
               continue;
            }
            if (_scanner.NextIs(SQL.WHERE))
            {
               tokens.Add(ReadWhere());
               continue;
            }
            if (_scanner.NextIs(SQL.GROUP))
            {
               tokens.Add(ReadGroupBy());
               continue;
            }
            if (_scanner.NextIs(SQL.HAVING))
            {
               tokens.Add(ReadHaving());
               continue;
            }
            if (_scanner.NextIs(SQL.ORDER))
            {
               tokens.Add(ReadOrderBy());
               continue;
            }
            var token = new SqlToken(_scanner.ReadAll());
            if (token.Expression.Length > 0)
            {
               tokens.Add(token);
            }

         }

         return tokens;
      }



      // where clause only can have one template placolder, at the end
      private WithToken ReadWith()
      {
         string expression = null;
         while (true)
         {
            SkipComments();

            if (_scanner.PeekAnyOf('('))
            {
               expression = _scanner.ReadParenthesed();
               continue;
            }

            if (_scanner.NextIs(SQL.AS))
            {
               return new WithToken(expression, ReadAlias());
            }
            throw new SqlParserException(_scanner.Source, _scanner.Position, "invalid WITH expression");
         }

      }
      private SelectToken ReadSelect()
      {
         var distinct = false;
         var selectChilds = new List<SqlToken>();
         while (true)
         {
            _scanner.SkipSpaces();

            if (_scanner.PeekAnyOf(SQL.FROM) || _scanner.Eof())
            {
               // done
               return new SelectToken(distinct, selectChilds);
            }

            if (_scanner.NextIs(SQL.TOP))
            {
               throw new SqlParserException(_scanner.Source, _scanner.Position, "TOP in select is not allowed, use paging instead");
            }
            if (_scanner.Peek() == '*')
            {
               throw new SqlParserException(_scanner.Source, _scanner.Position, "wildcards in select are not allowed, these cannot be mapped");
            }
            if (_scanner.NextIs(SQL.TAG1) || _scanner.NextIs(SQL.TAG2))
            {
               selectChilds.Add(new TagToken(ReadLineComment().Expression));
               continue;
            }
            if (_scanner.NextIs(SQL.LINECOMMENT))
            {
               ReadLineComment();
               continue;
            }
            if (_scanner.NextIs(SQL.DISTINCT))
            {
               distinct = true;
               continue;
            }
            if (_scanner.Peek() == ',')
            {
               _scanner.NextChar();
               continue;
            }
            selectChilds.Add(ReadColumn());
         }

      }
      private TableToken ReadTable()
      {
         return new TableToken(ReadMultiPart(true));
      }
      private JoinToken ReadJoin()
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
            ReadAnyToken(expression);
         }

         return new JoinToken(expression.ToString(), joinType, table);
      }
      private WhereToken ReadWhere()
      {
         SkipComments();
         var whereClause = new StringBuilder();
         var template = default(string);
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.GROUP, SQL.ORDER, SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            ReadAnyToken(whereClause);
         }
         if (_scanner.NextIs(SQL.TEMPLATE1) || _scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment().Expression;
         }

         return new WhereToken(whereClause.ToString(), template);
      }
      private GroupByToken ReadGroupBy()
      {
         SkipComments();
         _scanner.Expect(SQL.BY);
         SkipComments();
         var columns = new List<ColumnToken>();
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.ORDER, SQL.HAVING))
         {
            columns.Add(ReadColumn(false));
            SkipComments();
            if (_scanner.Peek() == ',')
            {
               _scanner.NextChar();
            }
         }
         return new GroupByToken(columns);
      }
      private HavingToken ReadHaving()
      {
         SkipComments();
         var havingClause = new StringBuilder();
         var template = default(string);
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.ORDER, SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            ReadAnyToken(havingClause);
         }
         if (_scanner.NextIs(SQL.TEMPLATE1) || _scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment().Expression;
         }

         return new HavingToken(havingClause.ToString(), template);
      }
      private OrderByToken ReadOrderBy()
      {
         SkipComments();
         _scanner.Expect(SQL.BY);
         SkipComments();
         var columns = new List<OrderColumnToken>();
         while (!_scanner.Eof() && !_scanner.PeekAnyOf(SQL.TEMPLATE1, SQL.TEMPLATE2))
         {
            var col = ReadColumn(false);
            var order = default(string);
            _scanner.SkipSpaces();
            if (_scanner.NextIs(SQL.ASC))
            {
               order = SQL.ASC;
               _scanner.SkipSpaces();
            }
            else if (_scanner.NextIs(SQL.DESC))
            {
               order = SQL.ASC;
               _scanner.SkipSpaces();
            }
            columns.Add(new OrderColumnToken(col.RawParts, order));
            if (_scanner.Peek() == ',')
            {
               _scanner.NextChar();
            }
         }
         string template = null;
         if (_scanner.NextIs(SQL.TEMPLATE1) || _scanner.NextIs(SQL.TEMPLATE2))
         {
            template = ReadLineComment().Expression;
         }
         return new OrderByToken(columns, template);
      }


      private string ReadAlias()
      {
         _scanner.SkipSpaces();
         return _scanner.IsStartQuote() ? _scanner.ReadQuoted() : _scanner.ReadIdentifier();
      }
      private ColumnToken ReadColumn(bool aliased = true)
      {
         return new ColumnToken(ReadMultiPart(aliased));
      }
      private MultiPartAliasedToken ReadMultiPart(bool aliased)
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
                  return new MultiPartAliasedToken(parts, ReadAlias());
               }
            }
            return new MultiPartAliasedToken(parts, null);
         }

      }
      private LineCommentToken ReadLineComment()
      {
         var sb = new StringBuilder();
         while (!_scanner.Eof() && !_scanner.PeekAnyOf('\r', '\n'))
         {
            _scanner.Take(sb);
         }
         return new LineCommentToken(sb.ToString());
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
      private void ReadAnyToken(StringBuilder sb)
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
