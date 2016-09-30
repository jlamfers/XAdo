using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using XAdo.Entities.Attributes;

namespace XAdo.Entities.Sql
{
   public class SqlBuilder
   {
      private string
         _sqlInsert,
         _sqlSelect,
         _sqlPagedSelect,
         _sqlUpdate,
         _sqlDelete;

      public Type DtoType { get; private set; }

      public SqlBuilder(Type dtoType)
      {
         if (dtoType == null) throw new ArgumentNullException("dtoType");
         DtoType = dtoType;
      }

      public virtual string SqlSelect
      {
         get { return _sqlSelect ?? (_sqlSelect = BuildSelect()); }
      }
      public virtual string SqlPagedSelect
      {
         get { return _sqlPagedSelect ?? (_sqlPagedSelect = BuildPagedSelect()); }
      }

      protected virtual string BuildSelect()
      {
         var sb = new StringBuilder();
         sb.AppendLine("SELECT ");
         sb.AppendLine("   " + string.Join(",\r\n   ", GetSelectColumns().ToArray()));
         sb.Append("FROM ");
         sb.AppendLine(GetTableName());
         sb.AppendLine("   "+Comment("WHERE_CLAUSE"));
         sb.AppendLine("   "+Comment("ORDER_BY_CLAUSE"));
         return sb.ToString();
      }

      protected virtual string BuildPagedSelect(string skipParameterName = "skip", string limitParameterName = "limit")
      {
         var sb = new StringBuilder(BuildSelect());
         sb.AppendFormat("   OFFSET {0} ROWS\r\n", FormatParameter(skipParameterName));
         sb.AppendFormat("   FETCH NEXT {0} ROWS ONLY\r\n", FormatParameter(limitParameterName));
         return sb.ToString();
      }

      protected virtual IEnumerable<string> GetSelectColumns()
      {
         return DtoType.GetFields().Cast<MemberInfo>().Concat(DtoType.GetProperties()).Select(GetName);
      }

      protected virtual string GetTableName()
      {
         return GetName(DtoType);
      }

      protected virtual string GetName(MemberInfo member)
      {
         var a = member.GetCustomAttribute<DbNameAttribute>();
         return a != null ? a.Name : member.Name;
      }

      protected virtual string Comment(string text)
      {
         return string.Format("/*{0}*/", text);
      }

      protected virtual string FormatParameter(string parameterName)
      {
         return "@" + parameterName.TrimStart('@');
      }
   }
}
