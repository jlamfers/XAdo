using System;

namespace XAdo.Core.Interface
{
    public interface IAdoTypeConverterFactory
    {
        IAdoTypeConverterFactory SetCustomTypeConverter<TSource, TTarget>(Func<TSource, TTarget> convertDelegate);
        bool CanCustomConvert(Type sourceType, Type targetType);
        Func<object, TTarget> GetConverter<TTarget>(Type sourceType);
    }

}
