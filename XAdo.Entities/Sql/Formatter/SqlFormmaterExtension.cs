using System;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Attributes;
using XAdo.Quobs.Expressions;
using XAdo.Quobs.Meta;

namespace XAdo.Quobs.Sql.Formatter
{
   public static class SqlFormmaterExtension
   {
      public static string FormatColumn<T>(this ISqlFormatter self, Expression<Func<T, object>> column)
      {
         var m = column.GetMemberInfo();
         return self.FormatColumn(m);
      }

      public static string FormatAlias<T>(this ISqlFormatter self, Expression<Func<T, object>> column)
      {
         var m = column.GetMemberInfo();
         return self.DelimitIdentifier(m.Name);
      }

      public static string FormatSelectColumn(this ISqlFormatter self, MemberInfo column)
      {
         var d = column.GetColumnDescriptor();
         return self.FormatColumn(d.Member) + (d.Name != d.Member.Name ? " AS " + self.DelimitIdentifier(d.Member.Name) : "");
      }

      public static string FormatSelectColumn<T>(this ISqlFormatter self, Expression<Func<T, object>> column)
      {
         var m = column.GetMemberInfo();
         return self.FormatSelectColumn(m);
      }


      public static OrderColumn FormatOrderByColumn<T>(this ISqlFormatter self, Expression<Func<T, object>> column, bool descending)
      {
         return new OrderColumn(self.FormatAlias(column), descending);
      }

   }
}