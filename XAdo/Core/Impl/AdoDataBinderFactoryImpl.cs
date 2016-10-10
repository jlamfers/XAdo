using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
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

      private readonly ConcurrentDictionary<string, object>
        _ctorBinderCache = new ConcurrentDictionary<string, object>();


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

      public virtual Func<IDataReader, TResult> TryCreateCtorBinder<TResult>(IDataRecord record)
      {
         var ctors = typeof (TResult).GetConstructors();
         if (ctors.Length == 1 && ctors[0].GetParameters().Length == 0) 
            return null;
         var sb = new StringBuilder(typeof(TResult).FullName);
            sb.Append(":");
            for (var i = 0; i < record.FieldCount; i++)
            {
               sb.Append(record.GetName(i));
            }
         var key = sb.ToString();
         return (Func<IDataReader, TResult>)_ctorBinderCache.GetOrAdd(key, s =>
         {
            var ctor = TryFindBinderConstructor(typeof (TResult), record);
            return ctor == null ? null : CompileCtorBinder<TResult>(record,ctor);
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
               if (m.Length != 1 || m[0].GetMemberType() != p.ParameterType)
               {
                  // parameter types must be identical to member types
                  return false;
               }
               try
               {
                  // parameter name must be bindable
                  return record.GetOrdinal(p.Name) >= 0;
               }
               catch (IndexOutOfRangeException)
               {
                  return false;
               }
            }));
      }

      public static class Converter<T>
      {
        public static TypeConverter Instance = TypeDescriptor.GetConverter(typeof(T));
      }

      private Func<IDataRecord, T> CompileCtorBinder<T>(IDataRecord record, ConstructorInfo ctorInfo)
      {
         var pars = ctorInfo.GetParameters();
         if (!pars.All(p =>
         {
            try
            {
               return CanConvertFrom(record.GetFieldType(record.GetOrdinal(p.Name)),
                  Nullable.GetUnderlyingType(p.ParameterType) ?? p.ParameterType);
            }
            catch (IndexOutOfRangeException)
            {
               return false;
            }
         }))
         {
            var getters = new List<Func<IDataRecord, object>>();
            foreach (var p in pars)
            {
               var ordinal = record.GetOrdinal(p.Name);
               var getterType = record.GetFieldType(ordinal);
               var setterType = p.ParameterType;
               var getter = ((IGetterFactory)_classBinder.Get(typeof (IGetterFactory<,>).MakeGenericType(setterType, getterType))).CreateGetter();
               var f = new Func<IDataRecord, object>(r => getter(r, ordinal));
               getters.Add(f);
            }
            var getterArray = getters.ToArray();
            //TODO: optimize with individual args
            return r =>
            {
               var args = getterArray.Select(a => a(r)).ToArray();
               return (T) ctorInfo.Invoke(args);
            };
         } 


         var dm = new DynamicMethod("__dm_" + ctorInfo.Name, typeof(T), new[] { typeof(IDataRecord) }, Assembly.GetExecutingAssembly().ManifestModule, true);
         var il = dm.GetILGenerator();

         foreach (ParameterInfo p in pars)
         {
            var parameterType = p.ParameterType;
            var parameterName = p.Name;
            var ordinal = record.GetOrdinal(parameterName);
            var fieldType = record.GetFieldType(ordinal);
            var normalizedParameterType = Nullable.GetUnderlyingType(parameterType) ?? parameterType;
            var ft = !fieldType.IsValueType || Nullable.GetUnderlyingType(parameterType) == null
               ? fieldType
               : typeof (Nullable<>).MakeGenericType(fieldType);

            if (!IsAssignable(fieldType, normalizedParameterType))
            {
               il.Emit(OpCodes.Ldsfld, typeof(Converter<>).MakeGenericType(parameterType).GetField("Instance"));
            }
            il.Emit(OpCodes.Ldsfld, typeof(GetterDelegate<>).MakeGenericType(ft).GetField("Getter"));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, ordinal);
            il.Emit(OpCodes.Callvirt, typeof(Func<,,>).MakeGenericType(typeof(IDataRecord), typeof(int), ft).GetMethod("Invoke", new[] { typeof(IDataReader), typeof(int) }));
            if (!IsAssignable(fieldType, normalizedParameterType))
            {
               if (ft.IsValueType)
               {
                  il.Emit(OpCodes.Box);
               }
               il.Emit(OpCodes.Call, typeof(TypeConverter).GetMethod("ConvertFrom", new[] { typeof(object) }));
               il.Emit(parameterType.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, parameterType);
            }
         }
         il.Emit(OpCodes.Newobj, ctorInfo);
         il.Emit(OpCodes.Ret);
         var factory = (Func<IDataRecord, T>)dm.CreateDelegate(typeof(Func<IDataRecord, T>));

         return factory;
      }

      protected virtual bool IsAssignable(Type fromType, Type intoType)
      {
         if (fromType == null) throw new ArgumentNullException("fromType");
         if (intoType == null) throw new ArgumentNullException("intoType");

         if (intoType.IsEnum && IsAssignable(fromType, Enum.GetUnderlyingType(intoType))) return true;
         return intoType.IsAssignableFrom(fromType);
      }
      protected virtual bool CanConvertFrom(Type fromType, Type intoType)
      {
         return IsAssignable(fromType,intoType) || TypeDescriptor.GetConverter(intoType).CanConvertFrom(fromType);
      }


   }
}


