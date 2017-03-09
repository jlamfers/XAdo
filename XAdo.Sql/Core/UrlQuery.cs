using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using XAdo.Quobs.Core.Interface;

namespace XAdo.Quobs.Core
{
   public class UrlQuery
   {
      private static class Constants
      {
         public const string
            Select = "select",
            Where = "where",
            Order = "order",
            Page = "page";

      }

      private static readonly Regex 
         NumericsRegex = new Regex(@"\d+",RegexOptions.Compiled);

      private readonly List<Tuple<string, string>> 
         _tokens = new List<Tuple<string, string>>();

      public UrlQuery(string query)
      {
         if (query != null)
         {
            // order matters!
            _tokens = query.SplitQuery().ToList();
         }
      }

      public string Select
      {
         get { return GetTokenByName(Constants.Select); }
      }
      public string Where
      {
         get { return GetTokenByName(Constants.Where); }
      }
      public string Order
      {
         get { return GetTokenByName(Constants.Order); }
      }
      public Tuple<int,int> Page
      {
         get
         {
            var token = GetTokenByName(Constants.Page);
            if (token == null) return null;
            try
            {
               var matches = NumericsRegex.Matches(token);
               return Tuple.Create(int.Parse(matches[0].Value), int.Parse(matches[1].Value));
            }
            catch
            {
               throw new QuobException("invalid page expression: " + token);
            }
         }
      }

      public UrlQuery AddWhere(string expression)
      {
         _tokens.Add(Tuple.Create(Constants.Where,expression));
         return this;
      }
      public UrlQuery SetPage(int page, int limit)
      {
         return SetTokenByName(Constants.Page, string.Format("{0}-{1}", page, limit));
      }
      public UrlQuery SetOrder(string expression)
      {
         return SetTokenByName(Constants.Order, expression);
      }
      public UrlQuery SetSelect(string expression)
      {
         return SetTokenByName(Constants.Select, expression);
      }

      public IList<Tuple<string, string>> Tokens
      {
         get { return _tokens.AsReadOnly(); }
      }

      public virtual IQuob Apply(ref IQuob quob)
      {
         // note that order matters here!
         foreach (var token in Tokens)
         {
            switch (token.Item1.ToLower())
            {
               case Constants.Select:
                  quob = quob.Select(token.Item2);
                  continue;
               case Constants.Where:
                  quob = quob.Where(token.Item2);
                  continue;
               case Constants.Order:
                  quob = quob.OrderBy(token.Item2);
                  continue;
               case Constants.Page:
                  try
                  {
                     var values = NumericsRegex.Matches(token.Item2);
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
                     throw new QuobException("invalid expression: " + token.Item2, ex);
                  }
                  continue;
            }
         }
         if (Page != null && Order == null)
         {
            quob = quob.OrderBy(quob.SqlResource.Select.Columns.First().Map.FullName);
         }
         return quob;

      }

      private string GetTokenByName(string name)
      {
         if (name == null) return null;
         var result = string.Join(Environment.NewLine, _tokens.Where(t => t.Item1 == name).Select(t => t.Item2)).Trim();
         return result.Length == 0 ? null : result;
      }

      private UrlQuery SetTokenByName(string name, string value)
      {
         if (name == null) throw new ArgumentNullException("name");
         if (value == null) throw new ArgumentNullException("value");

         var index = _tokens.FindIndex(t => t.Item1 == name);
         var token = Tuple.Create(name, value);
         if (index >= 0)
         {
            _tokens[index] = token;
         }
         else
         {
            _tokens.Add(token);
         }
         return this;

      }

   }
}