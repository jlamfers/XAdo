using System.Linq;
using XAdo.Sql.Core.Common;

namespace XAdo.Sql
{
   public class SqlTemplateArgs
   {
      public virtual SqlTemplateArgs Init(QueryContext context)
      {
         var and = "";
         foreach (var wc in context.WhereClauses)
         {
            Where = Where ?? "" + and + "(" + wc + ")";
            and = " AND ";
         }

         if (context.Group.Any())
         {
            Group = string.Join(", ", context.Group);
         }

         and = "";
         foreach (var hc in context.HavingClauses)
         {
            Having = Having ?? "" + and + "(" + hc + ")";
            and = " AND ";
         }
         if (context.Order.Any())
         {
            Order = string.Join(", ", context.Order);
         }
         if (context.Skip != null || context.Take != null)
         {
            Skip = context.Dialect.ParameterFormat.FormatWith(context.SkipParameterName);
            Take = context.Dialect.ParameterFormat.FormatWith(context.TakeParameterName);
         }
         Inner = context.Inner;
         return this;
      }

      public virtual string Where { get; set; }
      public virtual string Group { get; set; }
      public virtual string Having { get; set; }
      public virtual string Order { get; set; }
      public virtual object Skip { get; set; }
      public virtual object Take { get; set; }
      public virtual bool? Inner { get; set; }
   }
}