using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core.SqlExpression.Core;
using XAdo.Quobs.DbSchema;
using XAdo.Quobs.DbSchema.Attributes;

namespace XAdo.Quobs.Core
{
   public static class Extensions
   {

      public static bool IsJoinMethod(this Expression expression)
      {
         var callExpression = expression as MethodCallExpression;
         return callExpression != null && callExpression.Method.GetAnnotations<JoinMethodAttribute>().Any();
      }

      public static bool IsParameter(this Expression expression)
      {
         return expression != null && expression.NodeType == ExpressionType.Parameter;
      }

      public static IEnumerable<DbSchemaDescriptor.JoinDescriptor> GetJoinDescriptors(this MethodCallExpression self)
      {
         var atts = self.Method.GetAnnotations<JoinMethodAttribute>();
         if (!atts.Any()) return new DbSchemaDescriptor.JoinDescriptor[0];
         var joinType = JoinType.Inner;
         if (self.Method.GetParameters().Count() == 2)
         {
            joinType = (JoinType)self.Arguments[1].GetExpressionValue();
         }
         return self.Method.GetJoinDescriptors(joinType);
      }


      public static DbSchemaDescriptor.JoinPath GetJoinPath(this Expression exp)
      {
         var e = exp as MethodCallExpression;
         if (e == null) return null;
         var joinList = new List<DbSchemaDescriptor.JoinDescriptor>();
         while (CollectJoins(e, joinList))
         {
            e = e.Arguments[0] as MethodCallExpression;
            if (e == null) break;
         }
         if (joinList.Count == 0)
         {
            return null;
         }
         return new DbSchemaDescriptor.JoinPath(joinList);
      }

      private static bool CollectJoins(Expression exp, List<DbSchemaDescriptor.JoinDescriptor> joinList)
      {
         var m = exp as MethodCallExpression;
         if (m == null) return false;
         if (!m.Method.GetAnnotations<JoinMethodAttribute>().Any()) return false;
         var joinType = m.Arguments.Count > 1 ? (JoinType)m.Arguments[1].GetExpressionValue() : JoinType.Inner;
         var descriptors = m.Method.GetJoinDescriptors(joinType);
         joinList.InsertRange(0, descriptors);
         return true;
      }

   }
}