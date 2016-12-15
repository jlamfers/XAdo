using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using XAdo.SqlObjects.SqlExpression;

namespace XAdo.SqlObjects.SqlObjects
{
   internal static class Extensions
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
            var pi = (PropertyInfo) self;
            pi.SetValue(instance,ConvertValue(value,pi.PropertyType));
         }
         else if (self.MemberType == MemberTypes.Field)
         {
            var fi = (FieldInfo)self;
            fi.SetValue(instance, ConvertValue(value, fi.FieldType));
         }
         else
         {
            throw new InvalidOperationException("Invalid member type: "+self);
         }
      }

      private static object ConvertValue(object value, Type type)
      {
         if (value == null)
         {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
         }
         type = Nullable.GetUnderlyingType(type) ?? type;
         if (type.IsAssignableFrom(value.GetType())) return value;
         var typeConverter = TypeDescriptor.GetConverter(type);
         if (typeConverter.CanConvertFrom(value.GetType()))
         {
            return typeConverter.ConvertFrom(value);
         }
         value = typeConverter.ConvertFromInvariantString(string.Format(CultureInfo.InvariantCulture, "{0}", value));
         return value;
      }
   }
}