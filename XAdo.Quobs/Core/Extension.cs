using System;
using XAdo.Quobs.Core.SqlExpression;

namespace XAdo.Quobs.Core
{
   internal static class Extension
   {
      public static T CreateInstance<T>(this Type self)
      {
         return Activator.CreateInstance(self).CastTo<T>();
      }
   }
}