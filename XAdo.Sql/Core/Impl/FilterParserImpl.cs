using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using XAdo.Quobs.Core.Interface;
using XPression;

namespace XAdo.Quobs.Core.Impl
{
   public class UrlFilterParserImpl : IUrlFilterParser
   {
      private readonly UrlParser _parser = new UrlParser();

      public UrlFilterParserImpl()
      {
         ColumnSepChars = new HashSet<char>(new[]{' ','~',','}).AsReadOnly();
         AliasSepChars = new HashSet<char>(new[] { '|', ';', ':' }).AsReadOnly();
      }

      public virtual Expression ParseExpression(string filterExpression, Type inputType, Type resultType)
      {
         return _parser.Parse(filterExpression, inputType, resultType);
      }

      public virtual ICollection<char> ColumnSepChars { get; private set; }
      public virtual ICollection<char> AliasSepChars { get; private set; }
   }
}