using System;
using XAdo.Sql.Core.Common;
using XAdo.Sql.Core.Linq;
using XPression.Core.Functions;
using XPression.Language.Syntax;

namespace XAdo.Sql.Dialects
{
   public class XPressionSyntaxExtender : IAutoSyntaxExtender
   {
      public void ExtendSyntax(ISyntax syntax)
      {
         syntax.Functions.Add(new FunctionMap("avg",MemberInfoFinder.GetMethodInfo(() => SqlMethods.Avg(null))));
         syntax.Functions.Add(new FunctionMap("min", MemberInfoFinder.GetMethodInfo(() => SqlMethods.Min((object)null))));
         syntax.Functions.Add(new FunctionMap("max", MemberInfoFinder.GetMethodInfo(() => SqlMethods.Max((object)null))));
         syntax.Functions.Add(new FunctionMap("sum", MemberInfoFinder.GetMethodInfo(() => SqlMethods.Sum((object)null))));
         syntax.Functions.Add(new FunctionMap("count", MemberInfoFinder.GetMethodInfo(() => SqlMethods.Count((object)null))));
         syntax.Functions.Add(new FunctionMap("stdev", MemberInfoFinder.GetMethodInfo(() => SqlMethods.StDev((object)null))));
         syntax.Functions.Add(new FunctionMap("stdevp", MemberInfoFinder.GetMethodInfo(() => SqlMethods.StDevP((object)null))));

         syntax.Functions.Add(new FunctionMap("concat", MemberInfoFinder.GetMethodInfo(() => SqlMethods.Concat((new string[0])))));
         syntax.Functions.Add(new FunctionMap("wknr", MemberInfoFinder.GetMethodInfo(() => SqlMethods.WeekNumber(new DateTime()))));
         syntax.Functions.Add(new FunctionMap("wknr", MemberInfoFinder.GetMethodInfo(() => SqlMethods.WeekNumber(null))));
      }
   }
}
