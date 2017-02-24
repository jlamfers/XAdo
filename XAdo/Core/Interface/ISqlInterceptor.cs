using System;
using System.Data;

namespace XAdo.Core.Interface
{
   public enum SqlExecutionType
   {
      Query,
      Scalar,
      Multiple,
      EnumerableParam,
      Execute,
      Batch
   }

   public class AdoSqlInterception
   {
      public AdoSqlInterception(SqlExecutionType type, Type requestedType = null)
      {
         RequestedType = requestedType ?? typeof(object);
         Type = type;
      }

      public string Sql { get; set; }
      public object Arguments { get; set; }
      public CommandType? CommandType { get; set; }
      public SqlExecutionType Type { get; private set; }
      public Type RequestedType { get; private set; }
   }

   public interface IAdoSqlInterceptor
   {
      void BeforeExecute(AdoSqlInterception interception);
   }
}
