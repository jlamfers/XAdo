using System.Data;
using System.Reflection;

namespace XAdo.Core.Interface
{
    public interface IAdoPropertyBinder<in TEntity>
    {
        IAdoPropertyBinder<TEntity> Initialize(PropertyInfo property, int index, IAdoTypeConverterFactory typeConverterFactory);
        void CopyValue(IDataRecord source, TEntity target);
        int Index { get; }
    }
}