using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XAdo.Core.Cache;
using XAdo.Core.Impl;
using XAdo.Core.Interface;
using XAdo.Quobs.Core.Common;
using XAdo.Quobs.Core.Mapper;

namespace XAdo.Quobs.Core.Impl
{
   public class AdoCommandFactoryImplEx : AdoCommandFactoryImpl
   {
      private readonly IAdoParameterFactory _parameterFactory;

      private readonly LRUCache<Type,IDictionary<string,Func<object,object>>>
         _mapCache = new LRUCache<Type, IDictionary<string, Func<object,object>>>("LRUCache.MapCache.Size", 100);

      public AdoCommandFactoryImplEx(IAdoParameterFactory parameterFactory) : base(parameterFactory)
      {
         _parameterFactory = parameterFactory;
      }

      protected override IDictionary<string, IAdoParameter> AsAdoParameterDictionary(object target)
      {
         if (target == null) return new Dictionary<string, IAdoParameter>();

         var paramDict = target as IDictionary<string, IAdoParameter>;
         if (paramDict != null) return paramDict;

         var objectDict = target as IDictionary<string, object>;
         if (objectDict != null)
         {
            return objectDict.ToDictionary(kv => kv.Key, kv => kv.Value as IAdoParameter ?? _parameterFactory.Create(kv.Value));
         }

         
         var dict = _mapCache.GetOrAdd(target.GetType(), t => t.GetFullNameToMemberMap().ToDictionary(x => x.Key.Replace(".", "_"), x => BuildGetter(t,x.Value,x.Key)));

         return dict.ToDictionary(p => p.Key, p =>
         {
            var v = p.Value(target);
            return v as IAdoParameter ?? _parameterFactory.Create(v);
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
            type = m.GetMemberType();
            var result1 = result;
            result = result1 == null 
               ? new Func<object, object>(m.GetValue) 
               : x => m.GetValue(result1(x));

         }
         return result;


      }




   }
}
