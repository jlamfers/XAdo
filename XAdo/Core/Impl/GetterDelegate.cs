using System;
using System.Data;

namespace XAdo.Core.Impl
{
    public static class GetterDelegate<TGetter>
    {
        public static readonly Func<IDataRecord, int, TGetter> Getter;

        static GetterDelegate()
        {
            var getterType = typeof(TGetter);
            var underlyingGetterType = !getterType.IsValueType ? getterType : Nullable.GetUnderlyingType(getterType);
            var isNullable = underlyingGetterType != null;
            var getterMethod = isNullable
                ? typeof(NullableGetters).GetMethod(GetGetterName(underlyingGetterType))
                : typeof(IDataRecord).GetMethod(GetGetterName(getterType));

            Getter = getterMethod == null
               ? (d, i) =>
               {
                  var v = d.GetValue(i);
                  if (v == DBNull.Value)
                  {
                     v = null;
                  }
                  return (TGetter) v;
               } 
                : (Func<IDataRecord, int, TGetter>)
                    Delegate.CreateDelegate(typeof(Func<IDataRecord, int, TGetter>), getterMethod);
        }

        private static string GetGetterName(Type type)
        {
            return "Get" + (type == typeof(Single) ? "Float" : type.Name);
        }

    }
}