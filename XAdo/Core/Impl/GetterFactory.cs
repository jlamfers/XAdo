using System;
using System.Data;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public class GetterFactory<TSetter, TGetter> : IGetterFactory<TSetter, TGetter>
   {
      private readonly IAdoTypeConverterFactory _typeConverterFactory;

      public GetterFactory(IAdoTypeConverterFactory typeConverterFactory)
      {
         _typeConverterFactory = typeConverterFactory;
      }

      public virtual Func<IDataRecord, int, TSetter> CreateGetter()
      {
         Func<IDataRecord, int, TSetter> getter;
         if (!typeof (TSetter).IsAssignableFrom(typeof (TGetter)))
         {
            var converter = _typeConverterFactory.GetConverter<TSetter>(typeof (TGetter));
            if (typeof (TSetter).IsValueType && Nullable.GetUnderlyingType(typeof (TSetter)) == null)
            {
               getter = (d, i) => converter((TGetter) d.GetValue(i));
            }
            else
            {
               getter = (d, i) => d.IsDBNull(i) ? default(TSetter) : converter((TGetter) d.GetValue(i));
            }
         }
         else
         {
            getter = GetterDelegate<TSetter>.Getter;
         }
         return getter;
      }

      public object CreateTypedGetter()
      {
         return CreateGetter();
      }

      Func<IDataRecord, int, object> IGetterFactory.CreateGetter()
      {
         return (r, i) => CreateGetter()(r,i);
      }
   }
}