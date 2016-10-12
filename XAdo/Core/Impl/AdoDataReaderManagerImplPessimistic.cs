using System;
using System.Collections.Generic;
using System.Data;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    /// <summary>
    /// This pessimistic binding implementation always attempts to bind all columns in a multi bind scenario, instead of 
    /// breaking off the record's binding process if the first column is null.
    /// Normally you would break off the further binding processing, for the concerning record, when the first column represents 
    /// a (non nullable) primary key. In that case the whole record record result must be empty as a result of an outer join, en 
    /// no new record must be created. But if the first column CAN be null while others MAY provide values then all columns must 
    /// be attempted to bind. Still no record is created ONLY if ALL values are NULL.
    /// </summary>
    public class AdoDataReaderManagerImplPessimistic : AdoDataReaderManagerImpl
    {
        public AdoDataReaderManagerImplPessimistic(IAdoDataBinderFactory binderFactory, IAdoMultiBinderFactory multiBinderFactory, IActivatorFactory activatorFactory, IConcreteTypeBuilder proxyBuilder)
            : base(binderFactory, multiBinderFactory, activatorFactory, proxyBuilder)
        {
        }

        protected override T BindTarget<T>(IDataReader reader, int index, Func<IDataReader, T> recordBinder, Func<object> activator)
        {
            if (reader == null) throw new ArgumentNullException("reader");
            if (recordBinder == null) throw new ArgumentNullException("recordBinder");

           var result = recordBinder(reader);
           var allNull = true;
            foreach (var binder in typeof(T))
            {
                allNull = allNull && reader.IsDBNull(binder.DataRecordIndex);
                binder.CopyValue(reader, result);
            }
            if (allNull)
            {
                // assume empty record, probably outer join, no joined entity present
                result = default(T);
            }
            return result;

        }
    }

    public static partial class Extensions
    {
        public static IAdoContextInitializer EnablePessimisticDataReader(this IAdoContextInitializer self)
        {
            return self.BindSingleton<IAdoDataReaderManager, AdoDataReaderManagerImplPessimistic>();
        }
    }

}