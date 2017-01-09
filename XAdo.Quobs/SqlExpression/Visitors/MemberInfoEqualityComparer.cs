using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XAdo.SqlObjects.SqlExpression.Visitors
{
   public class MemberInfoEqualityComparer : IEqualityComparer<MemberInfo>
   {
      public bool Equals(MemberInfo x, MemberInfo y)
      {
         if (x == y) return true;

         if(x.Name != y.Name || x.GetMemberType() != y.GetMemberType())
         {
            return false;
         }

         if (x.DeclaringType == y.DeclaringType)
         {
            return true;
         }

         if (x.DeclaringType.IsInterface && x.DeclaringType.IsAssignableFrom(y.DeclaringType))
         {
            return true;
         }
         if (y.DeclaringType.IsInterface && y.DeclaringType.IsAssignableFrom(x.DeclaringType))
         {
            return true;
         }
         return false;
      }

      public int GetHashCode(MemberInfo obj)
      {
         if (obj == null) return 0;
         unchecked
         {
            int hash = obj.Name.GetHashCode();
            hash = hash * 1973 + obj.GetMemberType().GetHashCode();
            return hash;
         }
      }
   }
}
