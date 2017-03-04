using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class XAdoDataBinderFactoryPropertiesAndFieldsImpl : XAdoDataBinderFactoryImpl
    {

        public XAdoDataBinderFactoryPropertiesAndFieldsImpl(IXAdoTypeConverterFactory typeConverterFactory, IXAdoClassBinder classBinder)
          : base(typeConverterFactory, classBinder)
        {
        }

        public override IEnumerable<MemberInfo> GetBindableMembers(Type type, bool canWrite = true)
        {
            return base.GetBindableMembers(type, canWrite).Union(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => IsBindableDataType(f.FieldType)));
        }


    }

    public partial class Extensions
    {
        public static IXAdoContextInitializer EnableFieldBinding(this IXAdoContextInitializer self)
        {
            self.BindSingleton<IXAdoDataBinderFactory, XAdoDataBinderFactoryPropertiesAndFieldsImpl>();
            return self;
        }
    }
}