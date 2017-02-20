using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;

namespace XAdo.Parser
{
      public class SelectParser
      {
         private static HashSet<char> Spaces = new HashSet<char>(new[] { ' ', '\t', '\n', '\r' });
         private string _source;
         private int _pos;

         public SelectInfo Parse(string source, int startpos = 0)
         {
            _pos = startpos;
            _source = source;
            var columns = new List<ColumnInfo>();
            StringBuilder
               expr = new StringBuilder(),
               alias = new StringBuilder(),
               map = new StringBuilder();
            var readingExpr = true;
            _pos = _source.IndexOf("SELECT", startpos, StringComparison.OrdinalIgnoreCase);
            Expect("SELECT");
            SkipAnyOff("ALL", "DISTINCTROW");
            SkipSpaces();
            var distinct = NextIs("DISTINCT");
            int top;
            SkipTop(out top);
            var selectProperties = new SelectProperties(distinct, top);

            var selectColumnsPos = _pos;

            while (!Eof())
            {
               // now we are ready to read our columns
               switch (Peek())
               {
                  case '\'':
                  case '"':
                  case '`':
                  case '[':
                     if (readingExpr)
                     {
                        expr.Append(ReadQuoted(Peek()));
                     }
                     else
                     {
                        var quoted = ReadQuoted(Peek());
                        alias.Append(quoted.Substring(1, quoted.Length - 2));
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
                     NextChar();
                     SkipSpaces();
                     if (NextIs("-->") || NextIs("-- >"))
                     {
                        // read the mapping
                        ReadUntilEndOfLine(map);
                     }
                     columns.Add(new ColumnInfo(expr.ToString().Trim(), alias.ToString().Trim(), map.ToString().Trim()));
                     expr.Length = 0;
                     alias.Length = 0;
                     map.Length = 0;
                     readingExpr = true;
                     break;
                  default:
                     if (NextIs("-->") || NextIs("-- >"))
                     {
                        // read the mapping
                        ReadUntilEndOfLine(map);
                     }
                     if (NextIs("--"))
                     {
                        // skip the comments
                        ReadUntilEndOfLine(null);
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
                              if (!columns.Any() && expr[0] == '*')
                              {
                                 // probe next SELECT
                                 return Parse(_source, _pos);
                              }
                              columns.Add(new ColumnInfo(expr.ToString().Trim(), alias.ToString().Trim(), map.ToString().Trim()));
                           }
                           var fromPos = _pos - 4;
                           SkipSpaces();
                           var tables = new List<TableInfo>();
                           var tablename = NextToken();
                           string tablealias = null;
                           SkipSpaces();
                           if (NextIs("AS"))
                           {
                              SkipSpaces();
                              tablealias = NextToken();
                              SkipSpaces();
                           }
                           tables.Add(new TableInfo(tablealias != null ? tablename + " AS " + tablealias : tablename));
                           while (SkipAnyOff("INNER", "OUTER", "LEFT", "RIGHT", "JOIN", "FULL") && !Eof())
                           {
                              SkipSpaces();
                              while (SkipAnyOff("INNER", "OUTER", "JOIN"))
                              {
                                 SkipSpaces();
                              }
                              tablename = NextToken();
                              tablealias = null;
                              SkipSpaces();
                              if (NextIs("AS"))
                              {
                                 SkipSpaces();
                                 tablealias = NextToken();
                                 SkipSpaces();
                              }
                              tables.Add(new TableInfo(tablealias != null ? tablename + " AS " + tablealias : tablename));
                              Expect("ON");
                              SkipSpaces();
                              while (!SkipAnyOff("INNER", "OUTER", "LEFT", "RIGHT", "JOIN", "FULL") && !Eof())
                              {
                                 if (SkipAnyOff("WHERE", "HAVING", "ORDER", "UNION", "GROUP"))
                                 {
                                    return new SqlSelectInfo(_source, columns, tables, distinct, selectColumnsPos, fromPos, _pos);
                                 }
                                 _pos++;
                              }

                           }

                           return new SqlSelectInfo(_source, columns, tables, distinct, selectColumnsPos, fromPos, _pos);
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

         private TableInfo ReadFrom()
         {

         }

         public enum JoinType
         {
            Inner,
            Left,
            Right,
            Full
         }

         private TableInfo ReadTable()
         {
            SkipSpaces();
            var expression = NextToken();
            string alias = null;
            SkipSpaces();
            if (NextIs("AS"))
            {
               SkipSpaces();
               alias = NextToken();
               SkipSpaces();
            }
            return new TableInfo(expression,alias);
         }

         private IList<JoinInfo> ReadJoins(IList<JoinInfo> joins = null)
         {
            joins = joins ?? new List<JoinInfo>();
            JoinType joinType;
            if (NextIs("INNER") || NextIs("JOIN"))
            {
               joinType = JoinType.Inner;
            }
            else if (NextIs("LEFT"))
            {
               joinType = JoinType.Left;
               SkipAnyOff("OUTER", "JOIN");
            }
            else if (NextIs("RIGHT"))
            {
               joinType = JoinType.Right;
               SkipAnyOff("OUTER", "JOIN");
            }
            else if (NextIs("FULL"))
            {
               joinType = JoinType.Full;
               SkipAnyOff("JOIN");
            }
            else
            {
               return joins;
            }
            var table = ReadTable();
            Expect("ON");
            SkipSpaces();

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
         private string ReadQuoted()
         {
            return ReadQuoted(Peek());
         }
         private string ReadQuoted(char left)
         {
            var right = left == '[' ? ']' : left;
            var sb = new StringBuilder();
            if (Peek() != left)
            {
               throw new Exception("Invalid quoted start char");
            }
            Take(sb);
            while (!Eof())
            {
               char ch;
               sb.Append(ch = NextChar());
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
         private void ReadUntilEndOfLine(StringBuilder sb)
         {
            if (sb != null)
            {
               while (!Eof() && Peek() != '\n' && Peek() != '\r')
               {
                  Take(sb);
               }
               return;
            }
            while (!Eof() && Peek() != '\n')
            {
               NextChar();
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
         private bool SkipAnyOff(params string[] identifiers)
         {
            SkipSpaces();
            if (identifiers.Any(NextIs))
            {
               SkipSpaces();
               return true;
            }
            return false;
         }
         private void SkipTop(out int value)
         {
            value = 0;
            SkipSpaces();
            if (NextIs("TOP"))
            {
               // read any argument
               SkipSpaces();
               while (!Eof() && !IsSpace())
               {
                  value = int.Parse(NextToken());
               }
               SkipSpaces();
               if (NextIs("PERCENT"))
               {
                  throw new NotImplementedException("TOP PERCENT is not interpretated");
               }
               if (NextIs("WITH"))
               {
                  SkipSpaces();
                  Expect("TIES");
                  SkipSpaces();
               }
            }
         }
         private void SkipSpaces()
         {
            while (IsSpace())
            {
               NextChar();
            }
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
         private string NextToken()
         {
            var sb = new StringBuilder();
            while (!Eof() && !IsSpace())
            {
               if (IsQuote())
               {
                  sb.Append(ReadQuoted());
               }
               else
               {
                  Take(sb);
               }
            }
            return sb.ToString();
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
