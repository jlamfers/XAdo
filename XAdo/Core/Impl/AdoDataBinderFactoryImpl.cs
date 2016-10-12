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


   public class AdoDataBinderFactoryImpl : IAdoDataBinderFactory
   {
      protected readonly IAdoTypeConverterFactory TypeConverterFactory;
      private readonly IAdoClassBinder _classBinder;

      private readonly ConcurrentDictionary<BinderIdentity, object>
          _binderCache = new ConcurrentDictionary<BinderIdentity, object>();

      private readonly ConcurrentDictionary<BinderIdentity, object>
        _ctorBinderCache = new ConcurrentDictionary<BinderIdentity, object>();


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

      public AdoDataBinderFactoryImpl(IAdoTypeConverterFactory typeConverterFactory, IAdoClassBinder classBinder)
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

      protected virtual Type GetAdoMemberBinderType()
      {
         return typeof(AdoReaderToMemberBinderImpl<,,>);
      }

      public virtual IAdoReaderToMemberBinder<TEntity> CreateMemberBinder<TEntity>(MemberInfo member, Type getterType, int index)
      {
         if (member == null) throw new ArgumentNullException("member");
         if (getterType == null) throw new ArgumentNullException("getterType");

         var binderGetterType = Nullable.GetUnderlyingType(member.GetMemberType()) == getterType
             ? member.GetMemberType()
             : getterType;
         return ((IAdoReaderToMemberBinder<TEntity>)_classBinder.Get(typeof(AdoReaderToMemberBinderImpl<,,>)
             .MakeGenericType(typeof(TEntity), member.GetMemberType(), binderGetterType)))
             .Initialize(member, index);
      }

      public virtual Func<IDataReader, TResult> CreateScalarReader<TResult>(Type getterType)
      {
         if (getterType == null) throw new ArgumentNullException("getterType");

         if (typeof(TResult).IsAssignableFrom(getterType))
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

      public virtual Func<IDataReader, TResult> TryCreateCtorBinder<TResult>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null)
      {
         var key = new BinderIdentity(typeof (TResult), record, allowUnbindableFetchResults, allowUnbindableMembers,firstColumnIndex, lastColumnIndex);
         return (Func<IDataReader, TResult>)_ctorBinderCache.GetOrAdd(key, s =>
         {
            var ctor = TryFindBinderConstructor(typeof (TResult), record);
            return ctor == null ? null : CompileCtorBinder<TResult>(record, ctor, allowUnbindableFetchResults, allowUnbindableMembers, firstColumnIndex, lastColumnIndex);
         });
      }





      
      // initializes and caches a property binders list by entity type and a datareader structure
      public virtual IList<IAdoReaderToMemberBinder<T>> CreateMemberBinders<T>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null)
      {
         if (record == null) throw new ArgumentNullException("record");
         return
             (IList<IAdoReaderToMemberBinder<T>>)_binderCache.GetOrAdd(
                 new BinderIdentity(typeof(T), record, allowUnbindableFetchResults, allowUnbindableMembers, firstColumnIndex, lastColumnIndex),
                 k =>
                 {
                    var type = typeof(T);
                    var binders = new List<IAdoReaderToMemberBinder<T>>();
                    var first = firstColumnIndex.GetValueOrDefault(0);
                    var last = lastColumnIndex.GetValueOrDefault(record.FieldCount - 1);
                    for (var i = first; i <= last; i++)
                    {
                       var m = GetMemberOrNull(type, record.GetName(i), !allowUnbindableFetchResults);
                       if (m != null)
                       {
                          binders.Add(CreateMemberBinder<T>(m, record.GetFieldType(i), i));
                       }
                    }

                    if (binders.Count == 0)
                    {
                       if (record.FieldCount == 1)
                       {
                          throw new AdoBindingException("Cannot bind " + record.GetFieldType(0) + " to " + typeof(T));
                       }
                       throw new AdoBindingException("Type " + typeof(T) + " has no bindable properties");
                    }

                    if (!allowUnbindableMembers)
                    {
                       var set = new HashSet<string>();
                       for (var i = first; i <= last; i++)
                       {
                          set.Add(record.GetName(i));
                       }
                       if (GetBindableMembers(type).Any(p => !set.Contains(p.Name)))
                       {
                          throw new AdoBindingException("No bindable results for following " + type.Name +
                                                        " members: " +
                                                        string.Join(", ",
                                                            GetBindableMembers(type)
                                                                .Where(p => !set.Contains(p.Name))
                                                                .Select(p => p.Name)
                                                                .ToArray()));
                       }
                    }

                    return binders.ToArray();
                 });
      }

      public virtual bool IsBindableDataType(Type type)
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

      protected virtual MemberInfo GetMemberOrNull(Type type, string name, bool throwException)
      {
         var m = type.GetProperty(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
         if (throwException && m == null)
         {
            throw new AdoBindingException("Cannot bind fetched column [" + name + "] result to any member of type " + type.Name);
         }
         return m;
      }

      public virtual IEnumerable<MemberInfo> GetBindableMembers(Type type, bool canWrite = true)
      {
         return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => (!canWrite || p.CanWrite) && IsBindableDataType(p.PropertyType) && p.GetIndexParameters().Length == 0);
      }

      private ConstructorInfo TryFindBinderConstructor(Type type, IDataRecord record)
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

      class MemberBinder
      {
         public object Getter;
         public int Ordinal;
         public MemberInfo Member;
         public Type GetterType;
         public Type SetterType { get { return Member != null ? (Member.MemberType == MemberTypes.Property ? Member.CastTo<PropertyInfo>().PropertyType : Member.CastTo<FieldInfo>().FieldType) : null; } }
         public string Name;
      }


      private Func<IDataRecord, T> CompileCtorBinder<T>(IDataRecord record, ConstructorInfo ctorInfo, bool allowUnbindableFetchResults, bool allowUnbindableMembers, int? firstColumnIndex = null, int? lastColumnIndex = null)
      {
        // return CompileMemberBinder<T>(record, allowUnbindableFetchResults, allowUnbindableMembers, firstColumnIndex,lastColumnIndex);

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
               throw new AdoBindingException("Cannot bind fetched column [" + binder.Name + "] result to any member of type " + binder.GetterType.Name);
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
               throw new AdoBindingException("No bindable results for parameter " + p.Name);
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
               if (b.GetterType != b.SetterType || b.SetterType == typeof(byte[]) || b.SetterType == typeof(string) || Nullable.GetUnderlyingType(b.SetterType) != null)
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
                  il.Emit(OpCodes.Callvirt,GetGetterMethod(b.SetterType));
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

      private static MethodInfo GetGetterMethod(Type type)
      {
         type = Nullable.GetUnderlyingType(type) ?? type;
         var name = "Get" + (type == typeof(Single) ? "Float" : type.Name);
         IDataRecord r = null;
         return typeof (IDataRecord).GetMethod(name);
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
               throw new AdoBindingException("Cannot bind fetched column [" + binder.Name + "] result to any member of type " + binder.GetterType.Name);
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
               throw new AdoBindingException("No bindable results for parameter " + m.Name);
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
            if (b.GetterType != b.SetterType || b.SetterType == typeof(byte[]) || b.SetterType == typeof(string) || Nullable.GetUnderlyingType(b.SetterType) != null)
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
               il.Emit(OpCodes.Callvirt, GetGetterMethod(b.SetterType));
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

   }
}


