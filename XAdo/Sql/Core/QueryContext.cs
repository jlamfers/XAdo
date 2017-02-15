using System.Collections.Generic;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Sql.Core
{
   public class QueryContext
   {

      public QueryContext(ISqlDialect dialect)
      {
         Dialect = dialect;
         WhereClauses = new List<string>();
         HavingClauses = new List<string>();
         Arguments = new Dictionary<string, object>();
         Order = new List<string>();
      }
      public virtual string SkipParameterName
      {
         get { return "__skip"; }
      }
      public virtual string TakeParameterName
      {
         get { return "__take"; }
      }

      public ISqlDialect Dialect { get; private set; }

      public List<string> WhereClauses { get; private set; }
      public List<string> HavingClauses { get; private set; }
      public IDictionary<string, object> Arguments { get; private set; }
      public IList<string> Order { get; private set; }
      public int? Skip { get; set; }
      public int? Take { get; set; }
      public IAdoSession Session { get; set; }
      public bool? Inner { get; set; }

      public virtual IDictionary<string, object> GetArguments()
      {
         var args = new Dictionary<string, object>().AddRange(Arguments);
         if (Skip != null || Take != null)
         {
            args[SkipParameterName] = Skip ?? 0;
            args[TakeParameterName] = Take ?? int.MaxValue-100;
         }
         return args;
      }

      public virtual SqlTemplateArgs GetSqlTemplateArgs()
      {
         return new SqlTemplateArgs().Init(this);
      }


      public virtual QueryContext Clone(bool forInnerQuery = false)
      {
         var clone = new QueryContext(Dialect)
         {
            WhereClauses = WhereClauses.ToList(),
            HavingClauses = HavingClauses.ToList(),
            Session = Session,
            Arguments = Arguments.ToDictionary(x => x.Key, x => x.Value),
            Inner = forInnerQuery ? (bool?)true : null
         };
         if (!forInnerQuery)
         {
            clone.Skip = Skip;
            clone.Take = Take;
            clone.Order = Order.ToList();
         }
         return clone;
         
      }
      
   }

}