using System;
using System.ComponentModel;
using System.Globalization;
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
            var pi = (PropertyInfo) self;
            pi.SetValue(instance,SanitizeType(value,pi.PropertyType));
         }
         else if (self.MemberType == MemberTypes.Field)
         {
            var fi = (FieldInfo)self;
            fi.SetValue(instance, SanitizeType(value, fi.FieldType));
         }
         else
         {
            throw new InvalidOperationException("Invalid member type: "+self);
         }
      }

      private static object SanitizeType(object value, Type type)
      {
         if (value == null)
         {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
         }
         type = Nullable.GetUnderlyingType(type) ?? type;
         if (type.IsAssignableFrom(value.GetType())) return value;
         TypeConverter typeConverter = TypeDescriptor.GetConverter(type);
         if (typeConverter.CanConvertFrom(value.GetType()))
         {
            return typeConverter.ConvertFrom(value);
         }
         value = typeConverter.ConvertFromInvariantString(string.Format(CultureInfo.InvariantCulture, "{0}", value));
         return value;
      }
   }
}