using System;
using System.Collections.Generic;
using XAdo.Quobs.Sql;

namespace XAdo.Quobs
{
   public interface IQuob
   {
      IQuob Select(params SelectColumn[] columns);
      IQuob Select(params string[] expressions);
      IQuob Where(params string[] expressions);
      IQuob Having(params string[] expressions);
      IQuob OrderBy(params OrderColumn[] columns);
      IQuob GroupBy(params string[] expressions);
      IQuob Skip(int? value);
      IQuob Take(int? value);
      IQuob Distinct();
      long Count();
      IEnumerable<dynamic> ToEnumerable();
      IList<dynamic> ToList();
      IEnumerable<dynamic> ToEnumerable(out long count);
      IList<dynamic> ToList(out long count);
      IQuob Join(params string[] joins);
   }
}