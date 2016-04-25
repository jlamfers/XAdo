using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoMultiBinderFactoryImpl : IAdoMultiBinderFactory
    {
        private readonly IAdoDataBinderFactory _binderFactory;

        public AdoMultiBinderFactoryImpl(IAdoDataBinderFactory binderFactory)
        {
            if (binderFactory == null) throw new ArgumentNullException("binderFactory");
            _binderFactory = binderFactory;
        }

        // finds all column indices in datarecord and initizalizes corresponding binders for type T
        public virtual IList<IAdoPropertyBinder<T>> InitializePropertyBinders<T, TNext>(IDataRecord record,
            bool allowUnbindableFetchResults, bool allowUnbindableProperties, ref int nextIndex)
        {
            if (record == null) throw new ArgumentNullException("record");

            if (typeof (T) == typeof (TVoid)) return null;
            var allProperties =
                new HashSet<string>(
                    typeof (T).GetProperties()
                        .Where(
                            p =>
                                p.CanWrite && p.GetIndexParameters().Length == 0 &&
                                _binderFactory.IsBindableDataType(p.PropertyType))
                        .Select(p => p.Name));
            var set = new HashSet<string>(allProperties);
            var allNextProperties =
                new HashSet<string>(
                    typeof (TNext).GetProperties()
                        .Where(
                            p =>
                                p.CanWrite && p.GetIndexParameters().Length == 0 &&
                                _binderFactory.IsBindableDataType(p.PropertyType))
                        .Select(p => p.Name));
            var first = nextIndex;
            for (var i = first; i < record.FieldCount; i++)
            {
                nextIndex = i;
                var name = record.GetName(i);
                if (set.Remove(name) || !allNextProperties.Contains(name))
                {
                    // while type T has property <name>, or not type TNext has property <name>, continue with next
                    continue;
                }
                if (set.Count == 0)
                {
                    // we finished type set T
                    return _binderFactory.CreatePropertyBinders<T>(record, allowUnbindableFetchResults,
                        allowUnbindableProperties, first, nextIndex - 1);
                }
            }
            // we finished the datarecord, so we finished type set T as well
            nextIndex = record.FieldCount;
            return _binderFactory.CreatePropertyBinders<T>(record, allowUnbindableFetchResults,
                allowUnbindableProperties, first, nextIndex - 1);
        }

    }
}
