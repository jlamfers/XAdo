using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using XAdo.Quobs.Core.Common;
using XPression;

namespace XAdo.Quobs.Linq
{
   public class FilterParser : IFilterParser
   {
      private readonly UrlParser _parser = new UrlParser();

      public FilterParser()
      {
         ColumnSepChars = new HashSet<char>(new[]{' ','~',','}).AsReadOnly();
         AliasSepChars = new HashSet<char>(new[] { '|', ';', ':' }).AsReadOnly();
      }

      public Expression Parse(string filter, Type inputType, Type resultType)
      {
         return _parser.Parse(filter, inputType, resultType);
      }

      public ICollection<char> ColumnSepChars { get; private set; }
      public ICollection<char> AliasSepChars { get; private set; }
   }
}