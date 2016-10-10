using System;
using System.Data;
using System.Reflection;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public class AdoReaderToMemberBinderImpl<TEntity, TSetter, TGetter> : IAdoReaderToMemberBinder<TEntity>
   {
      private readonly IGetterFactory<TSetter, TGetter> _getterFactory;
      private Action<TEntity, TSetter> _setter;
      private Func<IDataRecord, int, TSetter> _getter;
      private int _dataRecordIndex;

      public AdoReaderToMemberBinderImpl(IGetterFactory<TSetter, TGetter> getterFactory)
      {
         _getterFactory = getterFactory;
      }

      public virtual IAdoReaderToMemberBinder<TEntity> Initialize(MemberInfo member, int dataRecordIndex)
      {
         _dataRecordIndex = dataRecordIndex;
         _setter = CreateSetter(member);
         _getter = _getterFactory.CreateGetter();
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

      // not virtual for performance reasons
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