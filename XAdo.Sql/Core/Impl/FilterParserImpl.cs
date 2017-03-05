using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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

      public Expression Parse(string filter, Type inputType, Type resultType)
      {
         return _parser.Parse(filter, inputType, resultType);
      }

      public ICollection<char> ColumnSepChars { get; private set; }
      public ICollection<char> AliasSepChars { get; private set; }
   }
}