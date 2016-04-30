using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoDataBinderFactoryPropertiesAndFieldsImpl : AdoDataBinderFactoryImpl
    {
        protected class AdoMemberBinderEx<TEntity, TSetter, TGetter> : AdoMemberBinder<TEntity, TSetter, TGetter>
        {
            protected override Action<TEntity, TSetter> CreateSetter(MemberInfo member)
            {
                try
                {
                    return member.MemberType != MemberTypes.Field ? base.CreateSetter(member) : CreateSetterDelegate((FieldInfo) member);
                }
                catch
                {
                    throw new AdoException("No setter available for property or field " + member);
                }
            }

            private static Action<TEntity, TSetter> CreateSetterDelegate(FieldInfo field)
            {
                string methodName = field.ReflectedType.FullName + ".set_" + field.Name;
                var setterMethod = new DynamicMethod(methodName, null, new[] { typeof(TEntity), typeof(TSetter) }, true);
                var gen = setterMethod.GetILGenerator();
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Stfld, field);
                gen.Emit(OpCodes.Ret);
                return (Action<TEntity, TSetter>)setterMethod.CreateDelegate(typeof(Action<TEntity, TSetter>));
            }
        }

        public AdoDataBinderFactoryPropertiesAndFieldsImpl(IAdoTypeConverterFactory typeConverterFactory)
            : base(typeConverterFactory)
        {
        }

        protected override Type GetAdoMemberBinderType()
        {
            return typeof(AdoMemberBinderEx<,,>);
        }

        protected override IEnumerable<MemberInfo> GetBindableMembers(Type type)
        {
            return base.GetBindableMembers(type).Union(type.GetFields().Where(f => IsBindableDataType(f.FieldType)));
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
            return self;
        }
    }
}