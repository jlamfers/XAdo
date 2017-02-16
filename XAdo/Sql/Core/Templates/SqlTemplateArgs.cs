using System.Linq;

namespace XAdo.Sql.Core
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
      public virtual string Having { get; set; }
      public virtual string Order { get; set; }
      public virtual object Skip { get; set; }
      public virtual object Take { get; set; }
      public virtual bool? Inner { get; set; }
   }
}