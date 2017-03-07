using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using XAdo.Quobs.Core.Interface;
using XPression;

namespace XAdo.Quobs.Core.Impl
{
   public class FilterParserImpl : IFilterParser
   {
      private readonly UrlParser _parser = new UrlParser();

      public FilterParserImpl()
      {
         ColumnSepChars = new HashSet<char>(new[]{' ','~',','}).AsReadOnly();
         AliasSepChars = new HashSet<char>(new[] { '|', ';', ':' }).AsReadOnly();
      }

      public virtual Expression Parse(string filter, Type inputType, Type resultType)
      {
         return _parser.Parse(filter, inputType, resultType);
      }

      // e.g., split: select(id~name).where(name~ct~J).order(-name~id).page(1-10)
      public virtual IList<KeyValuePair<string, string>> SplitQuery(string query)
      {
         var method = new StringBuilder();
         var expression = new StringBuilder();
         var current = method;
         var quoted = false;
         var parenDepth = 0;

         var tokens = new List<KeyValuePair<string, string>>();
         for (var i = 0; i < query.Length; i++)
         {
            var ch = query[i];
            switch (ch)
            {
               case '\'':
                  if (!quoted)
                  {
                     quoted = true;
                  }
                  else
                  {
                     if (i == query.Length - 1 || query[i + 1] != '\'')
                     {
                        quoted = false;
                     }
                  }
                  break;
               case'.':
                  if (quoted)
                  {
                     break;
                  }
                  if (current == expression && expression.Length == 0)
                  {
                     continue;
                  }
                  break;
               case '(':
                  if (quoted)
                  {
                     break;
                  }
                  if (parenDepth++ == 0)
                  {
                     current = expression;
                     continue;
                  }
                  break;
               case ')':
                  if (quoted)
                  {
                     break;
                  }
                  if (--parenDepth == 0)
                  {
                     tokens.Add(new KeyValuePair<string, string>(method.ToString(),expression.ToString()));
                     method.Length = 0;
                     expression.Length = 0;
                     current = method;
                     continue;
                  }
                  break;

            }
            current.Append(ch);
         }

         return tokens;
      }

      public virtual ICollection<char> ColumnSepChars { get; private set; }
      public virtual ICollection<char> AliasSepChars { get; private set; }
   }

   public class UrlQuery
   {
      private readonly IEnumerable<KeyValuePair<string, string>> _tokens;

      private static readonly Regex NumericsRegex = new Regex(@"\d+",RegexOptions.Compiled);

      public UrlQuery(IEnumerable<KeyValuePair<string, string>> tokens)
      {
         _tokens = tokens;
      }

      public IQuob Apply(IQuob quob)
      {
         foreach (var token in _tokens)
         {
            switch (token.Key.ToLower())
            {
               case "select":
                  quob = quob.Select(token.Value);
                  continue;
               case "where":
                  quob = quob.Where(token.Value);
                  continue;
               case "order":
                  quob = quob.OrderBy(token.Value);
                  continue;
               case "page":
                  try
                  {
                     var values = NumericsRegex.Matches(token.Value);
                     if (values.Count != 2)
                     {
                        throw new Exception("expected two numeric value in .page(..)");
                     }
                     var page = int.Parse(values[0].Value);
                     var limit = int.Parse(values[1].Value);
                     quob.Skip((page - 1)*limit);
                     quob.Take(limit);
                  }
                  catch (Exception ex)
                  {
                     throw new QuobException("invalid expression: " + token.Value,ex);
                  }
                  continue;
            }
         }
         return quob;

      }

   }
}