using System;
using System.Reflection;
using System.Reflection.Emit;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
    public class AdoReaderToMemberBinderImplEx<TEntity, TSetter, TGetter> : AdoReaderToMemberBinderImpl<TEntity, TSetter, TGetter>
    {
       public AdoReaderToMemberBinderImplEx(IGetterFactory<TSetter,TGetter> getterFactory)
          : base(getterFactory)
        {
        }

        protected override Action<TEntity, TSetter> CreateSetter(MemberInfo member)
        {
            try
            {
                return member.MemberType != MemberTypes.Field ? base.CreateSetter(member) : CreateSetterDelegate((FieldInfo)member);
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
}