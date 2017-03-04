using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public partial class XAdoDataReaderManagerImpl
   {
      #region Helper Types
      private interface IReaderHelperAsync
      {
         Task<IEnumerable> ReadAsync(IXAdoDataBinderFactory binderFactory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers);
      }

      private class ReaderHelperAsync<T> : IReaderHelperAsync
      {
         public async Task<IEnumerable> ReadAsync(IXAdoDataBinderFactory binderFactory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
         {
            var binder = binderFactory.CreateRecordBinder<T>(reader, allowUnbindableFetchResults, allowUnbindableMembers);

            var dbreader = (DbDataReader)reader;
            var list = new List<T>();
            while (await dbreader.ReadAsync())
            {
               list.Add(binder(reader));
            }
            return list;
         }
      }

      private interface IGraphReaderHelperAsync
      {
         Task<IEnumerable> ReadAllAsync(XAdoDataReaderManagerImpl parent, IXAdoGraphBinderFactory binderFactory, Func<object, object, object, object, object, object, object, object, object> factory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers);
      }

      private class GraphReaderHelperAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : IGraphReaderHelperAsync
      {
         public async Task<IEnumerable> ReadAllAsync(XAdoDataReaderManagerImpl parent, IXAdoGraphBinderFactory binderFactory, Func<object, object, object, object, object, object, object, object, object> factory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
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

            var list = new List<TResult>();

            var dbreader = (DbDataReader)reader;
            while (await dbreader.ReadAsync())
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
               list.Add((TResult)factory(target1, target2, target3, target4, target5, target6, target7, target8));
            }
            return list;

         }

         public static Func<object, object, object, object, object, object, object, object, object> CastFactory(object factory)
         {
            var f = (Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>)factory;
            return
                (x1, x2, x3, x4, x5, x6, x7, x8) =>
                    f((T1)x1, (T2)x2, (T3)x3, (T4)x4, (T5)x5, (T6)x6, (T7)x7, (T8)x8);
         }

         private static T BindTarget<T>(XAdoDataReaderManagerImpl parent, IDataReader reader, int index, Func<IDataReader, T> binder)
         {
            return parent.BindTarget(reader, index, binder);
         }
      }

      #endregion

      public virtual async Task<List<dynamic>> ReadAllAsync(IDataReader reader)
      {
         if (reader == null) throw new ArgumentNullException("reader");
         if (reader.FieldCount == 1)
         {
            var list = new List<dynamic>();
            var dbr = (DbDataReader)reader;
            while (await dbr.ReadAsync())
            {
               list.Add(dbr.IsDBNull(0) ? null : dbr.GetValue(0));
            }
            return list;
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
         var result = new List<dynamic>();
         var dbreader = (DbDataReader)reader;
         while (await dbreader.ReadAsync())
         {
            var values = new object[reader.FieldCount];
            dbreader.GetValues(values);
            result.Add(new XAdoRow(columnNames, values, columnTypes, index));
         }
         return result;
      }

      public virtual async Task<List<T>> ReadAllAsync<T>(IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
      {
         if (reader == null) throw new ArgumentNullException("reader");

         var dbreader = (DbDataReader)reader;
         if (typeof(T) == typeof(object) || typeof(T) == typeof(IDictionary<string, object>) || typeof(T) == typeof(XAdoRow))
         {
            var r = await ReadAllAsync(dbreader);
            return r.Cast<T>().ToList();
         }
         var result = new List<T>();
         if (dbreader.FieldCount == 1 && (typeof(T).IsValueType || CanConvert(dbreader.GetFieldType(0), typeof(T))))
         {
            var scalarBinder = _binderFactory.CreateScalarBinder<T>(dbreader.GetFieldType(0));
            while (await dbreader.ReadAsync())
            {
               result.Add(scalarBinder(dbreader));
            }
         }

         else
         {
            var concreteType = _concreteTypeBuilder.GetConcreteType(typeof(T));
            if (concreteType == typeof(T))
            {
               var recordBinder = _binderFactory.CreateRecordBinder<T>(reader, allowUnbindableFetchResults,allowUnbindableMembers);
               while (await dbreader.ReadAsync())
               {
                  result.Add(recordBinder(dbreader));
               }
            }
            else
            {

               var readerHelper =
                  (IReaderHelperAsync)
                     Activator.CreateInstance(typeof(ReaderHelperAsync<>).MakeGenericType(concreteType));

               var r =
                  await
                     readerHelper.ReadAsync(_binderFactory, reader, allowUnbindableFetchResults,
                        allowUnbindableMembers);
               return r.Cast<T>().ToList();
            }
         }
         return result;
      }

      public virtual async Task<List<TResult>> ReadAllAsync<T1, T2, T3, T4, T5, T6, T7, T8, TResult>(IDataReader reader, Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> factory, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
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
         var readerHelper = (IGraphReaderHelperAsync)Activator.CreateInstance(typeof(GraphReaderHelperAsync<,,,,,,,,>).MakeGenericType(t1, t2, t3, t4, t5, t6, t7, t8, tresult));
         var result = await readerHelper.ReadAllAsync(this, _graphBinderFactory, untypedFactory, reader, allowUnbindableFetchResults, allowUnbindableMembers);
         return result.Cast<TResult>().ToList();
      }
   }
}
