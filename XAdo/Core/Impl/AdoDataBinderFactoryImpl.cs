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

   // copies value from datarecord to entity


   public class AdoDataBinderFactoryImpl : IAdoDataBinderFactory
   {
      protected readonly IAdoTypeConverterFactory TypeConverterFactory;
      private readonly IAdoClassBinder _classBinder;

      private readonly ConcurrentDictionary<BinderIdentity, object>
          _binderCache = new ConcurrentDictionary<BinderIdentity, object>();

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

      public virtual Func<IDataReader, TResult> CreateCtorBinder<TResult>(IDataRecord record)
      {
         return CompileAnonymousCtor<TResult>(record, typeof(TResult).GetConstructors()[0]);
      }

      private static object FilterDbNull(object value)
      {
         return value == DBNull.Value ? null : value;
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

      public virtual IEnumerable<MemberInfo> GetBindableMembers(Type type)
      {
         return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.CanWrite && IsBindableDataType(p.PropertyType) && p.GetIndexParameters().Length == 0);
      }


      #region Anonymous ctor
      private static readonly ConcurrentDictionary<string, object>
        CtorCache = new ConcurrentDictionary<string, object>();

      private static Func<IDataRecord, T> CompileAnonymousCtor<T>(IDataRecord reader, ConstructorInfo ctorInfo)
      {
         var sb = new StringBuilder(ctorInfo.ToString());
         sb.Append(":");
         for (var i = 0; i < reader.FieldCount; i++)
         {
            sb.Append(reader.GetName(i));
         }
         return (Func<IDataRecord, T>)CtorCache.GetOrAdd(sb.ToString(), k => Compile2<T>(reader, ctorInfo));
      }
      private static Func<IDataRecord, T> Compile2<T>(IDataRecord reader, ConstructorInfo ctorInfo)
      {
         var pars = ctorInfo.GetParameters();
         var dm = new DynamicMethod("__dm_" + ctorInfo.Name, typeof(T), new[] { typeof(IDataRecord) }, Assembly.GetExecutingAssembly().ManifestModule, true);
         var il = dm.GetILGenerator();

         for (var i = 0; i < pars.Length; i++)
         {
            var type = pars[i].ParameterType;
            var name = pars[i].Name;

            il.Emit(OpCodes.Ldsfld, typeof(GetterDelegate<>).MakeGenericType(type).GetField("Getter"));
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldc_I4, reader.GetOrdinal(name));
            il.Emit(OpCodes.Callvirt, typeof(Func<,,>).MakeGenericType(typeof(IDataRecord), typeof(int), type).GetMethod("Invoke", new[] { typeof(IDataReader), typeof(int) }));
         }
         il.Emit(OpCodes.Newobj, ctorInfo);
         il.Emit(OpCodes.Ret);
         var factory = (Func<IDataRecord, T>)dm.CreateDelegate(typeof(Func<IDataRecord, T>));

         return factory;
      }
      #endregion


   }
}


