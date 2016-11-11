using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using XAdo.Quobs.Core.DbSchema;
using XAdo.Quobs.Core.DbSchema.Attributes;
using XAdo.Quobs.Core.SqlExpression.Core;

namespace XAdo.Quobs.Core
{
   public static class ExpressionExtensions
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
   }
}