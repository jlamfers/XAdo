using System;
using System.Text;

namespace XAdo.Core.Interface
{
   public class BatchItem
   {
      private StringBuilder _sql = new StringBuilder();
      public BatchItem(string sql, object args = null, Action<object> callback = null)
      {
         if (sql == null) throw new ArgumentNullException("sql");
         _sql.Append(sql);
         Args = args;
         Callback = callback;
      }
      public string Sql { get { return _sql.ToString(); } }
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

   public partial interface ISqlBatch
   {
      ISqlBatch Add(BatchItem item);
      bool Flush(IAdoSession session);
      int Count { get; }
      bool Clear();
   }
}