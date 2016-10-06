using System;
using System.Linq.Expressions;
using System.Reflection;
using XAdo.Quobs.Attributes;
using XAdo.Quobs.Expressions;

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
         var m = column;
         return self.FormatColumn(m) + (m.GetCustomAttribute<QuobsAttribute>() != null ? " AS " + self.DelimitIdentifier(m.Name) : "");
      }

      public static string FormatSelectColumn<T>(this ISqlFormatter self, Expression<Func<T, object>> column)
      {
         var m = column.GetMemberInfo();
         return self.FormatSelectColumn(m);
      }

      //tuple holds column spec and alias
      public static SelectColumn FormatSelectTuple(this ISqlFormatter self, MemberInfo column)
      {
         var m = column;
         return new SelectColumn(self.FormatColumn(m), m.GetCustomAttribute<QuobsAttribute>() != null ? self.DelimitIdentifier(m.Name) : null);
      }

      public static SelectColumn FormatSelectTuple<T>(this ISqlFormatter self, Expression<Func<T, object>> column)
      {
         return self.FormatSelectTuple(column.GetMemberInfo());
      }


      public static OrderColumn FormatOrderByColumn<T>(this ISqlFormatter self, Expression<Func<T, object>> column, bool descending)
      {
         return new OrderColumn(self.FormatAlias(column), descending);
      }

   }
}