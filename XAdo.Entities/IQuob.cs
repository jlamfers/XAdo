using System;
using System.Collections.Generic;

namespace XAdo.Quobs
{
   public interface IQuob
   {
      IQuob Select(params Tuple<string, string>[] raw);
      IQuob Select(params string[] raw);
      IQuob Where(params string[] raw);
      IQuob Having(params string[] raw);
      IQuob OrderBy(params string[] raw);
      IQuob GroupBy(params string[] raw);
      IQuob Skip(int? value);
      IQuob Take(int? value);
      IQuob Distinct();
      long Count();
      IEnumerable<dynamic> ToEnumerable();
      IList<dynamic> ToList();
      IEnumerable<dynamic> ToEnumerable(out long count);
      IList<dynamic> ToList(out long count);
   }
}