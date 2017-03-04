using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{

   // copies value from datarecord to entity


   public class XAdoDataBinderFactoryImpl : IXAdoDataBinderFactory
   {
      protected readonly IXAdoTypeConverterFactory 
         TypeConverterFactory;

      private readonly IXAdoClassBinder 
         _classBinder;

      private readonly LRUCache<BinderIdentity, object>
          _recordBinderCache = new LRUCache<BinderIdentity, object>("LRUCache.XAdo.RecordBinder.Size", 1000);

      protected static readonly HashSet<Type> NonPrimitiveBindableTypes = new HashSet<Type>(new[]
        {
            typeof (String),
            typeof (Decimal),
            typeof (DateTime),
            typeof (DateTimeOffset),
            typeof (TimeSpan),
            typeof (Guid), 
            typeof (byte[])
        });

      public XAdoDataBinderFactoryImpl(IXAdoTypeConverterFactory typeConverterFactory, IXAdoClassBinder classBinder)
      {
         TypeConverterFactory = typeConverterFactory;
         _classBinder = classBinder;
      }

      #region Types

      // identity (key) for datarecord/type based property binders list
      protected class BinderIdentity
      {
         private readonly Type _type;
         private readonly bool _allowUnbindableFetchResults;
         private readonly bool _allowUnbindableMembers;
         private readonly int? _firstColumnIndex;
         private readonly int? _lastColumnIndex;
         private readonly string[] _columnNames;
         private readonly Type[] _columnTypes;
         private readonly int _hash;

         public BinderIdentity(Type type, IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers,
             int? firstColumnIndex, int? lastColumnIndex)
         {
            _type = type;
            _allowUnbindableFetchResults = allowUnbindableFetchResults;
            _allowUnbindableMembers = allowUnbindableMembers;
            _firstColumnIndex = firstColumnIndex;
            _lastColumnIndex = lastColumnIndex;
            _columnNames = new string[record.FieldCount];
            _columnTypes = new Type[record.FieldCount];
            unchecked
            {
               const int factor = 1699;
               _hash = _type.GetHashCode();
               for (var i = 0; i < record.FieldCount; i++)
               {
                  _columnNames[i] = record.GetName(i);
                  _columnTypes[i] = record.GetFieldType(i);
                  _hash = _hash * factor + _columnNames[i].GetHashCode();
                  _hash = _hash * factor + _columnTypes[i].GetHashCode();
                  if (allowUnbindableFetchResults) _hash = _hash * factor + 1;
                  if (allowUnbindableMembers) _hash = _hash * factor + 1;
                  if (firstColumnIndex != null) _hash = _hash * factor + firstColumnIndex.Value;
                  if (lastColumnIndex != null) _hash = _hash * factor + lastColumnIndex.Value;
               }
            }
         }

         public override int GetHashCode()
         {
            return _hash;
         }

         public override bool Equals(object obj)
         {
            var other = (BinderIdentity)obj;
            return _type == other._type
                   && _allowUnbindableFetchResults == other._allowUnbindableFetchResults
                   && _allowUnbindableMembers == other._allowUnbindableMembers
                   && _firstColumnIndex == other._firstColumnIndex
                   && _lastColumnIndex == other._lastColumnIndex
                   && _columnNames.SequenceEqual(other._columnNames)
                   && _columnTypes.SequenceEqual(other._columnTypes);
         }
      }

      #endregion

      public Func<IDataReader, TResult> CreateRecordBinder<TResult>(IDataRecord record, bool allowUnbindableFetchResults,
         bool allowUnbindableMembers,
         int? firstColumnIndex = null, int? lastColumnIndex = null)
      {
         var key = new BinderIdentity(typeof (TResult), record, allowUnbindableFetchResults, allowUnbindableMembers,
            firstColumnIndex, lastColumnIndex);

         return
            (Func<IDataReader, TResult>)
               _recordBinderCache.GetOrAdd(key,
                  k =>
                     TryCompileCtorBinder<TResult>(record, allowUnbindableFetchResults, allowUnbindableMembers,
                        firstColumnIndex, lastColumnIndex)
                     ??
                     CompileMemberBinder<TResult>(record, allowUnbindableFetchResults, allowUnbindableMembers,
                        firstColumnIndex, lastColumnIndex));
      }

      public virtual Func<IDataReader, TResult> CreateScalarBinder<TResult>(Type getterType)
      {
         if (getterType == null) throw new ArgumentNullException("getterType");

         if (IsAssignable(getterType,typeof(TResult)))
         {
            var getter = GetterDelegate<TResult>.Getter;
            if (getter != null)
            {
               return r => getter(r, 0);
            }
         }
         var converter = TypeConverterFactory.GetConverter<TResult>(getterType);
         if (typeof(TResult).IsValueType && Nullable.GetUnderlyingType(typeof(TResult)) == null)
         {
            return r => converter(r.GetValue(0));
         }
         return r => r.IsDBNull(0) ? default(TResult) : converter(r.GetValue(0));
      }

      protected virtual Func<IDataReader, TResult> TryCompileCtorBinder<TResult>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null)
      {
         var ctor = TryFindBinderConstructor(typeof (TResult));
         return ctor == null ? null : CompileCtorBinder<TResult>(record, ctor, allowUnbindableFetchResults, allowUnbindableMembers, firstColumnIndex, lastColumnIndex);
      }

      protected virtual bool IsBindableDataType(Type type)
      {
         if (type == null) return false;
         type = Nullable.GetUnderlyingType(type) ?? type;
         if (type.IsPrimitive || type.IsEnum) return true;
         if (NonPrimitiveBindableTypes.Contains(type)) return true;

         // we do not need a reference to Microsoft.SqlServer.Types.SqlGeography here
         return type.FullName.StartsWith("Microsoft.SqlServer.Types.") && (
                type.FullName.EndsWith(".SqlGeography")
             || type.FullName.EndsWith(".SqlGeometry")
             || type.FullName.EndsWith(".SqlHierarchyId")
         );
      }

      protected virtual bool IsAssignable(Type fromType, Type intoType)
      {
         if (fromType == null) throw new ArgumentNullException("fromType");
         if (intoType == null) throw new ArgumentNullException("intoType");

         if (intoType.IsEnum && IsAssignable(fromType, Enum.GetUnderlyingType(intoType))) return true;
         return intoType.IsAssignableFrom(fromType);
      }

      public virtual IEnumerable<MemberInfo> GetBindableMembers(Type type, bool canWrite = true)
      {
         return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => (!canWrite || p.CanWrite) && IsBindableDataType(p.PropertyType) && p.GetIndexParameters().Length == 0);
      }

      private ConstructorInfo TryFindBinderConstructor(Type type)
      {
         return type.GetConstructors()
            .Where(c => c.GetParameters().Length == GetBindableMembers(type, false).Count())
            .SingleOrDefault(c => c.GetParameters().All(p =>
            {
               var m = type.GetMember(p.Name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.IgnoreCase);
               return m.Length == 1 && m[0].GetMemberType() == p.ParameterType;
            }));
      }

      class ParameterBinder
      {
         public object Getter;
         public int Ordinal;
         public ParameterInfo Parameter;
         public Type GetterType;
         public Type SetterType { get { return Parameter != null ? Parameter.ParameterType : null; }}
         public string Name;
      }

      private Func<IDataRecord, T> CompileCtorBinder<T>(IDataRecord record, ConstructorInfo ctorInfo, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null)
      {
         var binders = new List<ParameterBinder>();

         var first = firstColumnIndex.GetValueOrDefault(0);
         var last = lastColumnIndex.GetValueOrDefault(record.FieldCount - 1);
         for (var index = first; index <= last; index++)
         {
            var binder = new ParameterBinder
            {
               GetterType = record.GetFieldType(index), 
               Name = record.GetName(index),
            };
            binder.Parameter = ctorInfo.GetParameters().FirstOrDefault(x => string.Equals(x.Name, binder.Name, StringComparison.InvariantCultureIgnoreCase));
            if (binder.Parameter == null)
            {
               if (allowUnbindableFetchResults)
               {
                  continue;
               }
               throw new XAdoBindingException("Cannot bind fetched column [" + binder.Name + "] result to any member of type " + binder.GetterType.Name);
            }
            binder.Ordinal = index;
            binder.Getter = ((IGetterFactory)_classBinder.Get(typeof(IGetterFactory<,>).MakeGenericType(binder.SetterType, binder.GetterType))).CreateTypedGetter();
            binders.Add(binder);
         }
         var bindersByParameterOrder = new List<ParameterBinder>();
         foreach (var p in ctorInfo.GetParameters())
         {
            var binder = binders.SingleOrDefault(b => b.Parameter == p);
            if (binder == null)
            {
               if (allowUnbindableMembers)
               {
                  bindersByParameterOrder.Add(new ParameterBinder
                  {
                     Parameter = p
                  });
                  continue;
               }
               throw new XAdoBindingException("No bindable results for parameter " + p.Name);
            }
            bindersByParameterOrder.Add(binder);
         }
         var dm = new DynamicMethod("__dm_" + ctorInfo.Name, typeof (T),new[] {typeof (IDataRecord), typeof (Delegate[])}, Assembly.GetExecutingAssembly().ManifestModule, true);
         var il = dm.GetILGenerator();
         var i = 0;
         foreach (var b in bindersByParameterOrder)
         {
            var getter = b.Getter;
            if (getter != null)
            {
               if (!IsAssignable(b.GetterType, b.SetterType) || IsNullableBindableType(b.SetterType))
               {
                  il.Emit(OpCodes.Ldarg_1);
                  il.Emit(OpCodes.Ldc_I4, i);
                  il.Emit(OpCodes.Ldelem_Ref);
                  il.Emit(OpCodes.Castclass, getter.GetType());
                  il.Emit(OpCodes.Ldarg_0);
                  il.Emit(OpCodes.Ldc_I4, b.Ordinal);
                  il.Emit(OpCodes.Callvirt,
                     getter.GetType().GetMethod("Invoke", new[] {typeof (IDataRecord), typeof (int)}));
               }
               else
               {
                  il.Emit(OpCodes.Ldarg_0);
                  il.Emit(OpCodes.Ldc_I4, b.Ordinal);
                  il.Emit(OpCodes.Callvirt,GetRecordGetterMethod(b.SetterType));
               }
            }
            else
            {
               var loc = il.DeclareLocal(b.Parameter.ParameterType);
               il.Emit(OpCodes.Ldloc,loc);
            }
            i++;
         }
         il.Emit(OpCodes.Newobj, ctorInfo);
         il.Emit(OpCodes.Ret);
         var factory = (Func<IDataRecord, Delegate[], T>) dm.CreateDelegate(typeof (Func<IDataRecord, Delegate[], T>));
         var delegates = bindersByParameterOrder.Select(b => b.Getter).Cast<Delegate>().ToArray();
         return r => factory(r, delegates);
      }

      class MemberBinder
      {
         public object Getter;
         public int Ordinal;
         public MemberInfo Member;
         public Type GetterType;
         public Type SetterType { get { return Member != null ? (Member.MemberType == MemberTypes.Property ? Member.CastTo<PropertyInfo>().PropertyType : Member.CastTo<FieldInfo>().FieldType) : null; } }
         public string Name;
      }

      private Func<IDataRecord, T> CompileMemberBinder<T>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null)
      {

         var binders = new List<MemberBinder>();

         var bindableMembers = GetBindableMembers(typeof (T));

         var first = firstColumnIndex.GetValueOrDefault(0);
         var last = lastColumnIndex.GetValueOrDefault(record.FieldCount - 1);
         for (var index = first; index <= last; index++)
         {
            var binder = new MemberBinder
            {
               GetterType = record.GetFieldType(index),
               Name = record.GetName(index),
            };
            binder.Member = bindableMembers.FirstOrDefault(x => string.Equals(x.Name, binder.Name, StringComparison.InvariantCulture));
            if (binder.Member == null)
            {
               if (allowUnbindableFetchResults)
               {
                  continue;
               }
               throw new XAdoBindingException("Cannot bind fetched column [" + binder.Name + "] result to any member of type " + binder.GetterType.Name);
            }
            binder.Ordinal = index;
            binder.Getter = ((IGetterFactory)_classBinder.Get(typeof(IGetterFactory<,>).MakeGenericType(binder.SetterType, binder.GetterType))).CreateTypedGetter();
            binders.Add(binder);
         }
         foreach (var m in bindableMembers)
         {
            var binder = binders.SingleOrDefault(b => b.Member == m);
            if (binder == null)
            {
               if (allowUnbindableMembers)
               {
                  continue;
               }
               throw new XAdoBindingException("No bindable results for parameter " + m.Name);
            }
         }
         var dm = new DynamicMethod("__dm_" + typeof(T).Name, typeof(T), new[] { typeof(IDataRecord),typeof(Delegate[]) }, Assembly.GetExecutingAssembly().ManifestModule, true);
         var il = dm.GetILGenerator();
         var obj = il.DeclareLocal(typeof (T));
         il.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
         il.Emit(OpCodes.Stloc,obj);
         var i = 0;
         foreach (var b in binders)
         {
            var getter = b.Getter;
            il.Emit(OpCodes.Ldloc, obj);
            if (!IsAssignable(b.GetterType,b.SetterType)  || IsNullableBindableType(b.SetterType))
            {
               il.Emit(OpCodes.Ldarg_1);
               il.Emit(OpCodes.Ldc_I4, i);
               il.Emit(OpCodes.Ldelem_Ref);
               il.Emit(OpCodes.Castclass, getter.GetType());
               il.Emit(OpCodes.Ldarg_0);
               il.Emit(OpCodes.Ldc_I4, b.Ordinal);
               il.Emit(OpCodes.Callvirt, getter.GetType().GetMethod("Invoke", new[] {typeof (IDataRecord), typeof (int)}));
            }
            else
            {
               il.Emit(OpCodes.Ldarg_0);
               il.Emit(OpCodes.Ldc_I4, b.Ordinal);
               il.Emit(OpCodes.Callvirt, GetRecordGetterMethod(b.SetterType));
            }
            if (b.Member.MemberType == MemberTypes.Property)
            {
               il.Emit(OpCodes.Callvirt, b.Member.CastTo<PropertyInfo>().GetSetMethod());
            }
            else
            {
               il.Emit(OpCodes.Stfld, b.Member.CastTo<FieldInfo>());
            }
            i++;
         }
         il.Emit(OpCodes.Ldloc, obj);
         il.Emit(OpCodes.Ret);
         var factory = (Func<IDataRecord, Delegate[], T>)dm.CreateDelegate(typeof(Func<IDataRecord, Delegate[], T>));
         var delegates = binders.Select(b => b.Getter).Cast<Delegate>().ToArray();
         return r => factory(r, delegates);
      }

      private static bool IsNullableBindableType(Type type)
      {
         return !type.IsValueType || Nullable.GetUnderlyingType(type) != null;
      }

      private static MethodInfo GetRecordGetterMethod(Type type)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         var name = "Get" + (type == typeof(Single) ? "Float" : type.Name);
         return typeof(IDataRecord).GetMethod(name);
      }


   }
}


