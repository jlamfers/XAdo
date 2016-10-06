﻿using System.Collections.Generic;

namespace XAdo.Quobs.Sql
{
   public interface ISqlExecuter
   {
      T ExecuteScalar<T>(string sql, IDictionary<string, object> args);
      IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args);
      IEnumerable<T> ExecuteQuery<T>(string sql, IDictionary<string, object> args, string sqlCount, out long count);
   }
}