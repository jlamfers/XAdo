using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public partial class AdoDataReaderManagerImpl : IAdoDataReaderManager
   {
      #region Helper Types
      private interface IReaderHelper
      {
         IEnumerable Read(IAdoDataBinderFactory binderFactory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers);
      }

      private class ReaderHelper<T> : IReaderHelper
      {
         public IEnumerable Read(IAdoDataBinderFactory binderFactory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
         {
            var binder = binderFactory.CreateRecordBinder<T>(reader, allowUnbindableFetchResults, allowUnbindableMembers);

            while (reader.Read())
            {
               yield return binder(reader);
            }
         }
      }

      private interface IGraphReaderHelper
      {
         IEnumerable ReadAll(AdoDataReaderManagerImpl parent, IAdoGraphBinderFactory binderFactory, Func<object, object, object, object, object, object, object, object, object> factory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers);
      }

      private class GraphReaderHelper<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : IGraphReaderHelper
      {
         public IEnumerable ReadAll(AdoDataReaderManagerImpl parent, IAdoGraphBinderFactory binderFactory, Func<object, object, object, object, object, object, object, object, object> factory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
         {
            var next = 0;
            const int index1 = 0;
            var binder1 = binderFactory.CreateRecordBinder<T1, T2>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
            var index2 = next;
            var binder2 = binderFactory.CreateRecordBinder<T2, T3>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
            var index3 = next;
            var binder3 = binderFactory.CreateRecordBinder<T3, T4>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
            var index4 = next;
            var binder4 = binderFactory.CreateRecordBinder<T4, T5>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
            var index5 = next;
            var binder5 = binderFactory.CreateRecordBinder<T5, T6>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
            var index6 = next;
            var binder6 = binderFactory.CreateRecordBinder<T6, T7>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
            var index7 = next;
            var binder7 = binderFactory.CreateRecordBinder<T7, T8>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
            var index8 = next;
            var binder8 = binderFactory.CreateRecordBinder<T8, TVoid>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);

            var target1 = default(T1);
            var target2 = default(T2);
            var target3 = default(T3);
            var target4 = default(T4);
            var target5 = default(T5);
            var target6 = default(T6);
            var target7 = default(T7);
            var target8 = default(T8);

            while (reader.Read())
            {
               target1 = BindTarget(parent, reader, index1, binder1);
               target2 = BindTarget(parent, reader, index2, binder2);
               if (typeof(T3) == typeof(TVoid)) goto @yield;
               target3 = BindTarget(parent, reader, index3, binder3);
               if (typeof(T4) == typeof(TVoid)) goto @yield;
               target4 = BindTarget(parent, reader, index4, binder4);
               if (typeof(T5) == typeof(TVoid)) goto @yield;
               target5 = BindTarget(parent, reader, index5, binder5);
               if (typeof(T6) == typeof(TVoid)) goto @yield;
               target6 = BindTarget(parent, reader, index6, binder6);
               if (typeof(T7) == typeof(TVoid)) goto @yield;
               target7 = BindTarget(parent, reader, index7, binder7);
               if (typeof(T8) == typeof(TVoid)) goto @yield;
               target8 = BindTarget(parent, reader, index8, binder8);
            @yield:
               yield return (TResult)factory(target1, target2, target3, target4, target5, target6, target7, target8);
            }

         }

         public static Func<object, object, object, object, object, object, object, object, object> CastFactory(object factory)
         {
            var f = (Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)factory;
            return
                (x1, x2, x3, x4, x5, x6, x7, x8) =>
                    f((T1)x1, (T2)x2, (T3)x3, (T4)x4, (T5)x5, (T6)x6, (T7)x7, (T8)x8);
         }

         private static T BindTarget<T>(AdoDataReaderManagerImpl parent, IDataReader reader, int index, Func<IDataReader, T> recordBinder)
         {
            return parent.BindTarget(reader, index, recordBinder);
         }
      }


      #endregion

      private readonly IAdoDataBinderFactory _binderFactory;
      private readonly IAdoGraphBinderFactory _multiBinderFactory;
      private readonly IConcreteTypeBuilder _concreteTypeBuilder;

      public AdoDataReaderManagerImpl(IAdoDataBinderFactory binderFactory, IAdoGraphBinderFactory multiBinderFactory, IConcreteTypeBuilder concreteTypeBuilder)
      {
         if (binderFactory == null) throw new ArgumentNullException("binderFactory");
         if (multiBinderFactory == null) throw new ArgumentNullException("multiBinderFactory");
         if (concreteTypeBuilder == null) throw new ArgumentNullException("concreteTypeBuilder");
         _binderFactory = binderFactory;
         _multiBinderFactory = multiBinderFactory;
         _concreteTypeBuilder = concreteTypeBuilder;
      }

      public virtual IEnumerable<dynamic> ReadAll(IDataReader reader)
      {
         if (reader == null) throw new ArgumentNullException("reader");
         if (reader.FieldCount == 1)
         {
            while (reader.Read())
            {
               yield return reader.IsDBNull(0) ? null : reader.GetValue(0);
            }
            yield break;
         }

         var columnNames = new string[reader.FieldCount];
         var columnTypes = new Type[reader.FieldCount];
         var index = new Dictionary<string, int>();
         for (var i = 0; i < reader.FieldCount; i++)
         {
            columnNames[i] = reader.GetName(i);
            columnTypes[i] = reader.GetFieldType(i);
            index[reader.GetName(i)] = i;
         }

         var count = reader.FieldCount;
         var meta = new AdoRow.Meta { ColumnNames = columnNames, Index = index, Types = columnTypes };
         while (reader.Read())
         {
            var values = new object[count];
            reader.GetValues(values);
            yield return new AdoRow(meta, values);
         }
      }

      public virtual IEnumerable<T> ReadAll<T>(IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
      {
         if (reader == null) throw new ArgumentNullException("reader");

         if (typeof(T) == typeof(object) || typeof(T) == typeof(IDictionary<string, object>) ||
             typeof(T) == typeof(AdoRow))
         {
            foreach (var e in ReadAll(reader)) yield return (T)e;
            yield break;
         }

         if (reader.FieldCount == 1 && (typeof(T).IsValueType || CanConvert(reader.GetFieldType(0), typeof(T))))
         {
            var scalarBinder = _binderFactory.CreateScalarBinder<T>(reader.GetFieldType(0));
            while (reader.Read())
            {
               yield return scalarBinder(reader);
            }
         }
         else
         {
            var concreteType = _concreteTypeBuilder.GetConcreteType(typeof(T));

            if (concreteType == typeof(T))
            {
               var binder = _binderFactory.CreateRecordBinder<T>(reader, allowUnbindableFetchResults,allowUnbindableMembers);
               while (reader.Read())
               {
                  yield return binder(reader);
               }
               yield break;
            }

            var readerHelper = (IReaderHelper)Activator.CreateInstance(typeof(ReaderHelper<>).MakeGenericType(concreteType));
            foreach (var item in readerHelper.Read(_binderFactory, reader, allowUnbindableFetchResults, allowUnbindableMembers))
            {
               yield return (T)item;
            }
         }
      }

      public virtual IEnumerable<TResult> ReadAll<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IDataReader reader,
          Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, bool allowUnbindableFetchResults,
          bool allowUnbindableMembers)
      {

         if (reader == null) throw new ArgumentNullException("reader");
         if (factory == null) throw new ArgumentNullException("factory");

         var untypedFactory = GraphReaderHelper<T1, T2, T3, T4, T5, T6, T7, T8, TResult>.CastFactory(factory);

         var t1 = _concreteTypeBuilder.GetConcreteType(typeof(T1));
         var t2 = _concreteTypeBuilder.GetConcreteType(typeof(T2));
         var t3 = _concreteTypeBuilder.GetConcreteType(typeof(T3));
         var t4 = _concreteTypeBuilder.GetConcreteType(typeof(T4));
         var t5 = _concreteTypeBuilder.GetConcreteType(typeof(T5));
         var t6 = _concreteTypeBuilder.GetConcreteType(typeof(T6));
         var t7 = _concreteTypeBuilder.GetConcreteType(typeof(T7));
         var t8 = _concreteTypeBuilder.GetConcreteType(typeof(T8));
         var tresult = _concreteTypeBuilder.GetConcreteType(typeof(TResult));
         var readerHelper = (IGraphReaderHelper)Activator.CreateInstance(typeof(GraphReaderHelper<,,,,,,,,>).MakeGenericType(t1, t2, t3, t4, t5, t6, t7, t8, tresult));
         return readerHelper.ReadAll(this, _multiBinderFactory, untypedFactory, reader, allowUnbindableFetchResults, allowUnbindableMembers).Cast<TResult>();
      }

      protected virtual T BindTarget<T>(IDataReader reader, int index, Func<IDataReader, T> recordBinder)
      {
         if (reader == null) throw new ArgumentNullException("reader");
         if (recordBinder == null) throw new ArgumentNullException("recordBinder");
         return reader.IsDBNull(index) ? default(T) : recordBinder(reader);
      }

      protected virtual bool CanConvert(Type fromType, Type intoType)
      {
         if (fromType == null) throw new ArgumentNullException("fromType");
         if (intoType == null) throw new ArgumentNullException("intoType");

         if (intoType.IsEnum && CanConvert(fromType, Enum.GetUnderlyingType(intoType))) return true;
         return intoType.IsAssignableFrom(fromType) || TypeDescriptor.GetConverter(intoType).CanConvertFrom(fromType);
      }
   }
}