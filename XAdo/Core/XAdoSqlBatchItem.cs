using System;
using System.Text;

namespace XAdo.Core
{
   public class XAdoSqlBatchItem
   {
      private StringBuilder _sql = new StringBuilder();
      public XAdoSqlBatchItem(string sql, object args = null, Action<object> callback = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         _sql.Append(sql);
         Args = args;
         Callback = callback;
      }

      public string Sql
      {
         get { return _sql.ToString(); }
         internal set { _sql = new StringBuilder(value);} // from interceptor
      }
      public object Args { get; internal set; }
      public Action<object> Callback { get; private set; }

      internal void Append(string sql)
      {
         if (sql != null)
         {
            _sql.Append(sql);
         }
      }
   }
}