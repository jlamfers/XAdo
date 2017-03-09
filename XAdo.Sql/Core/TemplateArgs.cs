using System.Linq;
using System.Linq.Expressions;

namespace XAdo.Quobs.Core
{
   public class TemplateArgs
   {
      public virtual TemplateArgs Init(QuobSession context)
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
            var d = context.Quob.SqlResource.Dialect;
            Skip = d.ParameterFormat.FormatWith(context.SkipParameterName);
            Take = d.ParameterFormat.FormatWith(context.TakeParameterName);
         }
         Distinct = context.Quob.SqlResource.Select.Distinct ? (bool?)true : null;
         Top = context.Quob.SqlResource.Select.MaxRows;
         return this;
      }

      public virtual string Where { get; set; }
      public virtual string Group { get; set; }
      public virtual string Having { get; set; }
      public virtual string Order { get; set; }
      public virtual object Skip { get; set; }
      public virtual object Take { get; set; }
      public virtual bool? Distinct { get; set; }
      public virtual int? Top { get; set; }
   }
}