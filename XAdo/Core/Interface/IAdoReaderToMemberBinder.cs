using System.Data;
using System.Reflection;

namespace XAdo.Core.Interface
{
    public interface IAdoReaderToMemberBinder<in TEntity>
    {
        IAdoReaderToMemberBinder<TEntity> Initialize(MemberInfo member, int index);
        void CopyValue(IDataRecord source, TEntity target);
        int DataRecordIndex { get; }
    }
}