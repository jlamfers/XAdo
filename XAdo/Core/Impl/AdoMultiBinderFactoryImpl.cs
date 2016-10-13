using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoGraphBinderFactoryImpl : IAdoGraphBinderFactory
    {
        private readonly IAdoDataBinderFactory _binderFactory;

        public AdoGraphBinderFactoryImpl(IAdoDataBinderFactory binderFactory)
        {
            if (binderFactory == null) throw new ArgumentNullException("binderFactory");
            _binderFactory = binderFactory;
        }

        // finds all column indices in datarecord and initizalizes corresponding binders for type T
        public virtual Func<IDataReader, T> CreateRecordBinder<T, TNext>(IDataRecord record, bool allowUnbindableFetchResults, bool allowUnbindableMembers, ref int nextIndex)
        {
            if (record == null) throw new ArgumentNullException("record");

            if (typeof (T) == typeof (TVoid)) return null;
            var allMembers = new HashSet<string>(_binderFactory.GetBindableMembers(typeof(T)).Select(p => p.Name));
            var set = new HashSet<string>(allMembers);
            var allNextMembers = new HashSet<string>(_binderFactory.GetBindableMembers(typeof(TNext)).Select(p => p.Name));
            var first = nextIndex;
            for (var i = first; i < record.FieldCount; i++)
            {
                nextIndex = i;
                var name = record.GetName(i);
                if (set.Remove(name) || !allNextMembers.Contains(name))
                {
                    // while type T has property <name>, or not type TNext has property <name>, continue with next
                    continue;
                }
                if (set.Count == 0)
                {
                    // we finished type set T
                    return _binderFactory.CreateRecordBinder<T>(record, allowUnbindableFetchResults,allowUnbindableMembers, first, nextIndex - 1);
                }
            }
            // we finished the datarecord, so we finished type set T as well
            nextIndex = record.FieldCount;
            return _binderFactory.CreateRecordBinder<T>(record, allowUnbindableFetchResults,allowUnbindableMembers, first, nextIndex - 1);
        }

    }
}
