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
            IEnumerable Read(IAdoDataBinderFactory binderFactory, IActivatorFactory activatorFactory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers);
        }

        private class ReaderHelper<T> : IReaderHelper
        {
            public IEnumerable Read(IAdoDataBinderFactory binderFactory, IActivatorFactory activatorFactory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
            {
                var binders = binderFactory.CreateMemberBinders<T>(reader, allowUnbindableFetchResults, allowUnbindableMembers);

                var activator = activatorFactory.GetActivator(typeof(T));

                while (reader.Read())
                {
                    var target = (T)activator();
                    foreach (var binder in binders)
                    {
                        binder.CopyValue(reader, target);
                    }
                    yield return target;
                }
            }
        }

        private interface IGraphReaderHelper
        {
            IEnumerable ReadAll(AdoDataReaderManagerImpl parent, IAdoMultiBinderFactory binderFactory, IActivatorFactory activatorFactory, Func<object, object, object, object, object, object, object, object, object> factory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers);
        }

        private class GraphReaderHelper<T1, T2, T3, T4, T5, T6, T7, T8, TResult> : IGraphReaderHelper
        {
            public IEnumerable ReadAll(AdoDataReaderManagerImpl parent, IAdoMultiBinderFactory binderFactory, IActivatorFactory activatorFactory, Func<object, object, object, object, object, object, object, object, object> factory, IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
            {
                var next = 0;
                const int index1 = 0;
                var binders1 = binderFactory.InitializeMemberBinders<T1, T2>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
                var index2 = next;
                var activator1 = activatorFactory.GetActivator(typeof(T1));
                var binders2 = binderFactory.InitializeMemberBinders<T2, T3>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
                var index3 = next;
                var activator2 = activatorFactory.GetActivator(typeof(T2));
                var binders3 = binderFactory.InitializeMemberBinders<T3, T4>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
                var index4 = next;
                var activator3 = activatorFactory.GetActivator(typeof(T3));
                var binders4 = binderFactory.InitializeMemberBinders<T4, T5>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
                var index5 = next;
                var activator4 = activatorFactory.GetActivator(typeof(T4));
                var binders5 = binderFactory.InitializeMemberBinders<T5, T6>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
                var index6 = next;
                var activator5 = activatorFactory.GetActivator(typeof(T5));
                var binders6 = binderFactory.InitializeMemberBinders<T6, T7>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
                var index7 = next;
                var activator6 = activatorFactory.GetActivator(typeof(T6));
                var binders7 = binderFactory.InitializeMemberBinders<T7, T8>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
                var index8 = next;
                var activator7 = activatorFactory.GetActivator(typeof(T7));
                var binders8 = binderFactory.InitializeMemberBinders<T8, TVoid>(reader, allowUnbindableFetchResults, allowUnbindableMembers, ref next);
                var activator8 = activatorFactory.GetActivator(typeof(T8));

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
                    target1 = BindTarget(parent, reader, index1,  binders1, activator1);
                    target2 = BindTarget(parent, reader, index2, binders2, activator2);
                    if (typeof(T3) == typeof(TVoid)) goto @yield;
                    target3 = BindTarget(parent, reader, index3, binders3, activator3);
                    if (typeof(T4) == typeof(TVoid)) goto @yield;
                    target4 = BindTarget(parent, reader, index4, binders4, activator4);
                    if (typeof(T5) == typeof(TVoid)) goto @yield;
                    target5 = BindTarget(parent, reader, index5, binders5, activator5);
                    if (typeof(T6) == typeof(TVoid)) goto @yield;
                    target6 = BindTarget(parent, reader, index6, binders6, activator6);
                    if (typeof(T7) == typeof(TVoid)) goto @yield;
                    target7 = BindTarget(parent, reader, index7, binders7, activator7);
                    if (typeof(T8) == typeof(TVoid)) goto @yield;
                    target8 = BindTarget(parent, reader, index8, binders8, activator8);
                @yield:
                    yield return (TResult)factory(target1, target2, target3, target4, target5, target6, target7, target8);
                }

            }

            public static Func<object, object, object, object, object, object, object, object, object> CastFactory(object factory )
            {
                var f = (Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult>) factory;
                return
                    (x1, x2, x3, x4, x5, x6, x7, x8) =>
                        f((T1) x1, (T2) x2, (T3) x3, (T4) x4, (T5) x5, (T6) x6, (T7) x7, (T8) x8);
            } 

            private static T BindTarget<T>(AdoDataReaderManagerImpl parent, IDataReader reader, int index, IList<IAdoReaderToMemberBinder<T>> binders, Func<object> factory)
            {
                return parent.BindTarget(reader, index, binders, factory);
            }
        }


        #endregion

        private readonly IAdoDataBinderFactory _binderFactory;
        private readonly IAdoMultiBinderFactory _multiBinderFactory;
        private readonly IActivatorFactory _activatorFactory;
        private readonly IConcreteTypeBuilder _concreteTypeBuilder;

        public AdoDataReaderManagerImpl(IAdoDataBinderFactory binderFactory, IAdoMultiBinderFactory multiBinderFactory, IActivatorFactory activatorFactory, IConcreteTypeBuilder concreteTypeBuilder)
        {
            if (binderFactory == null) throw new ArgumentNullException("binderFactory");
            if (multiBinderFactory == null) throw new ArgumentNullException("multiBinderFactory");
            if (activatorFactory == null) throw new ArgumentNullException("activatorFactory");
            if (concreteTypeBuilder == null) throw new ArgumentNullException("concreteTypeBuilder");
            _binderFactory = binderFactory;
            _multiBinderFactory = multiBinderFactory;
            _activatorFactory = activatorFactory;
            _concreteTypeBuilder = concreteTypeBuilder;
        }

        public virtual IEnumerable<dynamic> ReadAll(IDataReader reader)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            var columnNames = new string[reader.FieldCount].ToList();
            var columnTypes = new Type[reader.FieldCount].ToList();
            var index = new Dictionary<string, int>();
            for (var i = 0; i < reader.FieldCount; i++)
            {
                columnNames[i] = reader.GetName(i);
                columnTypes[i] = reader.GetFieldType(i);
                index[reader.GetName(i)] = i;
            }

            while (reader.Read())
            {
                var values = new object[reader.FieldCount];
                reader.GetValues(values);
                yield return new AdoRow(columnNames, values, columnTypes, index);
            }
        }

        public virtual IEnumerable<T> ReadAll<T>(IDataReader reader, bool allowUnbindableFetchResults, bool allowUnbindableMembers)
        {
            if (reader == null) throw new ArgumentNullException("reader");

            if (typeof (T) == typeof (object) || typeof (T) == typeof (IDictionary<string, object>) ||
                typeof (T) == typeof (AdoRow))
            {
                foreach (var e in ReadAll(reader)) yield return (T) e;
                yield break;
            }

            if (reader.FieldCount == 1 && (typeof (T).IsValueType || CanConvert(reader.GetFieldType(0), typeof (T))))
            {
                var scalarReader = _binderFactory.CreateScalarReader<T>(reader.GetFieldType(0));
                while (reader.Read())
                {
                    yield return scalarReader(reader);
                }
            }

            else
            {
                var concreteType = _concreteTypeBuilder.GetConcreteType(typeof (T));
                var readerHelper = (IReaderHelper)Activator.CreateInstance(typeof (ReaderHelper<>).MakeGenericType(concreteType));

                foreach (var item in readerHelper.Read(_binderFactory, _activatorFactory, reader, allowUnbindableFetchResults, allowUnbindableMembers)){
                    yield return (T) item;
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

            var t1 = _concreteTypeBuilder.GetConcreteType(typeof (T1));
            var t2 = _concreteTypeBuilder.GetConcreteType(typeof(T2));
            var t3 = _concreteTypeBuilder.GetConcreteType(typeof(T3));
            var t4 = _concreteTypeBuilder.GetConcreteType(typeof(T4));
            var t5 = _concreteTypeBuilder.GetConcreteType(typeof(T5));
            var t6 = _concreteTypeBuilder.GetConcreteType(typeof(T6));
            var t7 = _concreteTypeBuilder.GetConcreteType(typeof(T7));
            var t8 = _concreteTypeBuilder.GetConcreteType(typeof(T8));
            var tresult = _concreteTypeBuilder.GetConcreteType(typeof(TResult));
            var readerHelper = (IGraphReaderHelper)Activator.CreateInstance(typeof(GraphReaderHelper<,,,,,,,,>).MakeGenericType(t1, t2, t3, t4, t5, t6, t7, t8, tresult));
            return readerHelper.ReadAll(this, _multiBinderFactory, _activatorFactory, untypedFactory, reader, allowUnbindableFetchResults, allowUnbindableMembers).Cast<TResult>();
        }

        protected virtual T BindTarget<T>(IDataReader reader, int index, IList<IAdoReaderToMemberBinder<T>> binders, Func<object> factory )
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (binders == null) throw new ArgumentNullException("binders");

            if (reader.IsDBNull(index))
            {
                // assume key, probably outer join, no joined entity present
                return default(T);
            }
            var result = (T) factory();
            foreach (var binder in binders)
            {
                binder.CopyValue(reader, result);
            }
            return result;
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