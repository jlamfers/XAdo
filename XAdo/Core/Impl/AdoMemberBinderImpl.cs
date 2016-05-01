using System;
using System.Data;
using System.Reflection;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoMemberBinderImpl<TEntity, TSetter, TGetter> : IAdoMemberBinder<TEntity>
    {
        private readonly IAdoTypeConverterFactory _typeConverterFactory;
        private Action<TEntity, TSetter> _setter;
        private Func<IDataRecord, int, TSetter> _getter;
        private int _dataRecordIndex;

        public AdoMemberBinderImpl(IAdoTypeConverterFactory typeConverterFactory)
        {
            _typeConverterFactory = typeConverterFactory;
        }

        public virtual IAdoMemberBinder<TEntity> Initialize(MemberInfo member, int dataRecordIndex)
        {
            _dataRecordIndex = dataRecordIndex;
            _setter = CreateSetter(member);
            _getter = CreateGetter();
            return this;
        }

        protected virtual Action<TEntity, TSetter> CreateSetter(MemberInfo member)
        {
            var property = (PropertyInfo)member;
            var setMethod = property.GetSetMethod(true);
            if (setMethod == null)
            {
                throw new AdoException("No setter available for property " + property);
            }
            return
                (Action<TEntity, TSetter>)
                    Delegate.CreateDelegate(typeof(Action<TEntity, TSetter>), setMethod);
        }

        protected virtual Func<IDataRecord, int, TSetter> CreateGetter()
        {
            Func<IDataRecord, int, TSetter> getter;
            if (!typeof(TSetter).IsAssignableFrom(typeof(TGetter)))
            {
                var converter = _typeConverterFactory.GetConverter<TSetter>(typeof(TGetter));
                if (typeof(TSetter).IsValueType && Nullable.GetUnderlyingType(typeof(TSetter)) == null)
                {
                    getter = (d, i) => converter((TGetter)d.GetValue(i));
                }
                else
                {
                    getter = (d, i) => d.IsDBNull(i) ? default(TSetter) : converter((TGetter)d.GetValue(i));
                }
            }
            else
            {
                getter = GetterDelegate<TSetter>.Getter;
            }
            return getter;
        }

        public void CopyValue(IDataRecord reader, TEntity entity)
        {
            _setter(entity, _getter(reader, _dataRecordIndex));
        }

        public int DataRecordIndex
        {
            get { return _dataRecordIndex; }
        }
    }
}