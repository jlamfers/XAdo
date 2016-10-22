using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   // test implementation

   public class AdoGraphBinderCompilerImpl : IAdoGraphBinderCompilerImpl
   {
      private class Key
      {
         private readonly string _key;
         private readonly int _hashcode;
         public Key(IDataReader reader, Type[] binderTypes, Type resultType)
         {
            var sb = new StringBuilder();
            for (var i = 0; i < reader.FieldCount; i++)
            {
               sb.Append(reader.GetName(i));
               sb.Append(reader.GetFieldType(i).Name);
            }
            for (var i = 0; i < binderTypes.Length; i++)
            {
               sb.Append(binderTypes[i].Name);
            }
            sb.Append(resultType.Name);
            _key = sb.ToString();
            _hashcode = _key.GetHashCode();
         }

         public override bool Equals(object obj)
         {
            var other = obj as Key;
            return other != null && other._key == _key;
         }

         public override int GetHashCode()
         {
            return _hashcode;
         }
      }

      private static readonly HashSet<MethodInfo> 
         DelegateMethods = new HashSet<MethodInfo>(typeof(Delegate).GetMethods());

      private readonly ConcurrentDictionary<Key, object>
         _cache = new ConcurrentDictionary<Key, object>();

      private readonly IAdoGraphBinderFactory 
         _graphBinderFactory;


      public AdoGraphBinderCompilerImpl(IAdoGraphBinderFactory graphBinderFactory)
      {
         if (graphBinderFactory == null) throw new ArgumentNullException("graphBinderFactory");
         _graphBinderFactory = graphBinderFactory;
      }


      public Func<IDataReader, object> CompileGraphReader(IDataReader reader, Type[] binderTypes, Type resultType, bool allowUnbindableFetchResults, bool allowUnbindableMembers, Delegate handler_BinderTypes_ResultType)
      {
         var key = new Key(reader, binderTypes, resultType);
         return (Func<IDataReader, object>)_cache.GetOrAdd(key, t => _CompileGraphReader(reader, binderTypes, resultType, allowUnbindableFetchResults, allowUnbindableMembers, handler_BinderTypes_ResultType));

      }

      private Func<IDataReader, object> _CompileGraphReader(IDataReader reader, Type[] binderTypes, Type resultType, bool allowUnbindableFetchResults, bool allowUnbindableMembers, Delegate handler_BinderTypes_ResultType)
      {
         var delegates = new Delegate[binderTypes.Length + 1];
         var next = 0;
         var dm = new DynamicMethod("__dm_graph" + resultType.Name, typeof(object), new[] { typeof(IDataReader), typeof(Delegate[]) }, Assembly.GetExecutingAssembly().ManifestModule, true);
         var il = dm.GetILGenerator();
         il.Emit(OpCodes.Ldarg_1);
         il.Emit(OpCodes.Ldc_I4, binderTypes.Length);
         il.Emit(OpCodes.Ldelem_Ref);
         //il.Emit(OpCodes.Castclass, handler_BinderTypes_ResultType.GetType());
         int i;
         for (i = 0; i < binderTypes.Length; i++)
         {
            var t1 = binderTypes[i];
            var t2 = i == binderTypes.Length - 1 ? typeof(TVoid) : binderTypes[i + 1];
            var d = CreateRecordBinder(t1, t2, reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
            il.Emit(OpCodes.Ldarg_1);
            il.Emit(OpCodes.Ldc_I4, i);
            il.Emit(OpCodes.Ldelem_Ref);
            //il.Emit(OpCodes.Castclass, d.GetType());
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Callvirt, GetTypedInvoke(d));
            delegates[i] = d;
         }
         il.Emit(OpCodes.Callvirt, GetTypedInvoke(handler_BinderTypes_ResultType));
         il.Emit(OpCodes.Castclass, typeof(object));
         il.Emit(OpCodes.Ret);
         delegates[i] = handler_BinderTypes_ResultType;

         var factory = (Func<IDataReader, Delegate[], object>)dm.CreateDelegate(typeof(Func<IDataReader, Delegate[], object>));
         return r => factory(r, delegates);
      }

      private Delegate CreateRecordBinder(Type type, Type typeNext, IDataReader reader,bool allowUnbindableFetchResults, bool allowUnbindableMembers, ref int next)
      {
         var m = typeof(IAdoGraphBinderFactory).GetMethods().Single(x => x.Name == "CreateRecordBinder");
         m = m.MakeGenericMethod(type, typeNext);
         var args = new object[] { reader, allowUnbindableFetchResults, allowUnbindableMembers, next };
         var result = (Delegate)m.Invoke(_graphBinderFactory, args);
         next = (int)args[3];
         return result;
      }

      private static MethodInfo GetTypedInvoke(Delegate d)
      {
         return d.GetType().GetMethods().Single(m => m.Name == "Invoke" && !DelegateMethods.Contains(m));
      }


   }
}
