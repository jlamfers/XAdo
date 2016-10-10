using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoDataBinderFactoryPropertiesAndFieldsImpl : AdoDataBinderFactoryImpl
    {

        public AdoDataBinderFactoryPropertiesAndFieldsImpl(IAdoTypeConverterFactory typeConverterFactory, IAdoClassBinder classBinder)
          : base(typeConverterFactory, classBinder)
        {
        }

        public override IEnumerable<MemberInfo> GetBindableMembers(Type type, bool canWrite = true)
        {
            return base.GetBindableMembers(type, canWrite).Union(type.GetFields(BindingFlags.Public | BindingFlags.Instance).Where(f => IsBindableDataType(f.FieldType)));
        }

        protected override MemberInfo GetMemberOrNull(Type type, string name, bool throwException)
        {
            return type.GetField(name) ?? base.GetMemberOrNull(type, name, throwException);
        }

    }

    public partial class Extensions
    {
        public static IAdoContextInitializer EnableFieldBinding(this IAdoContextInitializer self)
        {
            self.BindSingleton<IAdoDataBinderFactory, AdoDataBinderFactoryPropertiesAndFieldsImpl>();
            self.Bind(typeof(AdoReaderToMemberBinderImpl<,,>), typeof(AdoReaderToMemberBinderImplEx<,,>));
            return self;
        }
    }
}