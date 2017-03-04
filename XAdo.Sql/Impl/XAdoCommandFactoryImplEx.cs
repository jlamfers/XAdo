using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XAdo.Core;
using XAdo.Core.Impl;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Mapper;

namespace XAdo.Quobs.Impl
{
   public class XAdoCommandFactoryImplEx : XAdoCommandFactoryImpl
   {
      private readonly IXAdoParameterFactory _parameterFactory;

      private readonly LRUCache<Type,IDictionary<string,Func<object,object>>>
         _mapCache = new LRUCache<Type, IDictionary<string, Func<object,object>>>("LRUCache.MapCache.Size", 100);

      public XAdoCommandFactoryImplEx(IXAdoParameterFactory parameterFactory) : base(parameterFactory)
      {
         _parameterFactory = parameterFactory;
      }

      protected override IDictionary<string, IXAdoParameter> AsAdoParameterDictionary(object target)
      {
         if (target == null) return new Dictionary<string, IXAdoParameter>();

         var paramDict = target as IDictionary<string, IXAdoParameter>;
         if (paramDict != null) return paramDict;

         var objectDict = target as IDictionary<string, object>;
         if (objectDict != null)
         {
            return objectDict.ToDictionary(kv => kv.Key, kv => kv.Value as IXAdoParameter ?? _parameterFactory.Create(kv.Value));
         }

         
         var dict = _mapCache.GetOrAdd(target.GetType(), t => t.GetFullNameToMemberMap().ToDictionary(x => x.Key.Replace(".", "_"), x => BuildGetter(t,x.Value,x.Key)));

         return dict.ToDictionary(p => p.Key, p =>
         {
            var v = p.Value(target);
            return v as IXAdoParameter ?? _parameterFactory.Create(v);
         });

      }

      private static Func<object,object> BuildGetter(Type type, MemberInfo lastMember, string path)
      {
         var partials = path.Split('.');
         if (partials.Length == 1)
         {
            return lastMember.GetValue;
         }
         Func<object, object> result = null;
         foreach (var p in partials)
         {
            var m = type.GetPropertyOrField(p);
            type = MemberInfoFinder.GetMemberType(m);
            var result1 = result;
            result = result1 == null 
               ? new Func<object, object>(m.GetValue) 
               : x => m.GetValue(result1(x));

         }
         return result;


      }




   }
}
