using System.Collections.Generic;
using System.Linq;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Interface;

namespace XAdo.Quobs.Core
{
   public class QuobSession
   {

      public QuobSession(IQuob quob)
      {
         Quob = quob;
         WhereClauses = new List<string>();
         HavingClauses = new List<string>();
         Arguments = new Dictionary<string, object>();
         Order = new List<string>();
         Group = new List<string>();
      }
      public virtual string SkipParameterName
      {
         get { return "__xado_skip"; }
      }
      public virtual string TakeParameterName
      {
         get { return "__xado_take"; }
      }

      public IQuob Quob { get; private set; }

      public List<string> WhereClauses { get; private set; }
      public IList<string> Group { get; private set; }
      public List<string> HavingClauses { get; private set; }
      public IDictionary<string, object> Arguments { get; private set; }
      public IList<string> Order { get; private set; }
      public int? Skip { get; set; }
      public int? Take { get; set; }

      public IXAdoDbSession DbSession { get; set; }

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

      public virtual TemplateArgs GetSqlTemplateArgs()
      {
         return new TemplateArgs().Init(this);
      }


      public virtual QuobSession Clone()
      {
         var clone = new QuobSession(Quob)
         {
            WhereClauses = WhereClauses.ToList(),
            HavingClauses = HavingClauses.ToList(),
            DbSession = DbSession,
            Arguments = Arguments.ToDictionary(x => x.Key, x => x.Value),
            Group = Group.ToList(),
            Skip = Skip,
            Take = Take,
            Order = Order.ToList()
         };
         return clone;
         
      }
      
   }

}