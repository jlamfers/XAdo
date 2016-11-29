using System;
using System.ComponentModel;
using System.Reflection;
using XAdo.Quobs.Core.SqlExpression;

namespace XAdo.Quobs.Core
{
   internal static class Extension
   {
      public static T CreateInstance<T>(this Type self)
      {
         return Activator.CreateInstance(self).CastTo<T>();
      }

      public static void SetValue(this MemberInfo self, object instance, object value)
      {
         if (self == null) throw new ArgumentNullException("self");
         if (self.MemberType == MemberTypes.Property)
         {
            ((PropertyInfo)self).SetValue(instance,value);
         }
         else if (self.MemberType == MemberTypes.Field)
         {
            ((FieldInfo) self).SetValue(instance, value);
         }
         else
         {
            throw new InvalidOperationException("Invalid member type: "+self);
         }
      }
   }
}