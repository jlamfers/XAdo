using System;

namespace XAdo.Quobs
{
   public static class Extensions
   {
      internal static T EnsureNotNull<T>(this T value, string name)
         where T : class
      {
         if (value == null)
         {
            throw new ArgumentNullException(name);
         }
         return value;
      }

      public static T CastTo<T>(this object self)
      {
         return self == null ? default(T) : (T) self;
      }
   }
}
