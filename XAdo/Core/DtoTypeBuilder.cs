using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;

namespace XAdo.Core
{
    public class DtoTypeBuilder
    {
        private readonly ConcurrentDictionary<string,FieldBuilder>
            _fields = new ConcurrentDictionary<string, FieldBuilder>();
        private static readonly ModuleBuilder 
            _moduleBuilder;
        private static long 
            _uniqueCounter;
        private static readonly AssemblyBuilder 
            _assemblyBuilder;
        private static readonly AssemblyName 
            _assemblyName;
        private TypeBuilder 
            _typeBuilder;

        static DtoTypeBuilder()
        {
            var domain = AppDomain.CurrentDomain;
            _assemblyName = new AssemblyName("DtoTypeBuilder");
#if DEBUG
            _assemblyBuilder = domain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.RunAndSave);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyName.Name, _assemblyName.Name + ".dll");            
#else
            _assemblyBuilder = domain.DefineDynamicAssembly(_assemblyName, AssemblyBuilderAccess.Run);
            _moduleBuilder = _assemblyBuilder.DefineDynamicModule(_assemblyName.Name);
#endif
        }

        protected virtual long NextUniqueNumber()
        {
            return Interlocked.Increment(ref _uniqueCounter);
        }

        protected virtual string GetUniqueName()
        {
            return "<type_" + NextUniqueNumber() + ">";
        }

        public virtual string TypeName { get; set; }

        protected virtual TypeBuilder TypeBuilder
        {
            get
            {
                if (_typeBuilder == null)
                {
                    _typeBuilder = _moduleBuilder.DefineType(TypeName ?? GetUniqueName(), TypeAttributes.Public);
                    
                }
                return _typeBuilder;
            }
            set { _typeBuilder = value; }
        }

        protected virtual bool AddInterface(Type interfaceType)
        {
            if (_typeBuilder == null && TypeName == null)
            {
                TypeName = "XAdo.Implementations." + (interfaceType.Name.StartsWith("I") ? interfaceType.Name.Substring(1) : interfaceType.Name);
            }
            else if (TypeBuilder.GetInterfaces().Contains(interfaceType))
            {
                return false;
            }
            TypeBuilder.AddInterfaceImplementation(interfaceType);
            return true;
        }

        protected virtual FieldBuilder GetOrAddField(string name, Type type)
        {
            var fieldBuilder = _fields.GetOrAdd(name, p =>
            {
                var attBuilder =
                    new CustomAttributeBuilder(
                        typeof(DebuggerBrowsableAttribute).GetConstructor(new[] { typeof(DebuggerBrowsableState) }),
                        new object[] { DebuggerBrowsableState.Never });

                var fb = TypeBuilder.DefineField("_" + name, type, FieldAttributes.Private);
                fb.SetCustomAttribute(attBuilder);
                return fb;
            });
            if (fieldBuilder.FieldType != type)
            {
                throw new InvalidOperationException("Duplicate field (" + name + ") with different types.");
            }
            return fieldBuilder;
            
        }

        protected virtual void ImplementInterfaceProperty(PropertyInfo property, bool shareBackingFieldsForSameNamesAndTypes = true)
        {
            if (property == null) throw new ArgumentNullException("property");
            if (property.DeclaringType == null || !property.DeclaringType.IsInterface)
            {
                throw new InvalidOperationException("Property must be an interface declared property.");
            }
            AddInterface(property.DeclaringType);
            const MethodAttributes baseAttributes = MethodAttributes.HideBySig
                            | MethodAttributes.NewSlot
                            | MethodAttributes.Virtual;

            var attributes = baseAttributes;
            var name = property.Name;
            FieldBuilder field;
            _fields.TryGetValue(property.Name, out field);
            if (field != null)
            {
                // make implementation explicit, access same field
                attributes |= (MethodAttributes.Final | MethodAttributes.Private);
                name = property.DeclaringType.Namespace + "." + property.DeclaringType.Name + "." + property.Name;
                if (field.FieldType != property.PropertyType)
                {
                    // in all cases we need a new backing field because of the different field type
                    field = null;
                }
            }
            else
            {
                attributes |= MethodAttributes.Public;
            }

            if (!shareBackingFieldsForSameNamesAndTypes)
            {
                field = null;
            }
            var propertyBuilder = ImplementProperty(name, property.PropertyType, attributes, PropertyAttributes.HasDefault, field);

            if (property.CanRead)
            {
                TypeBuilder.DefineMethodOverride(propertyBuilder.GetGetMethod(true),property.GetGetMethod());
            }
            if (property.CanWrite)
            {
                TypeBuilder.DefineMethodOverride(propertyBuilder.GetSetMethod(true), property.GetSetMethod());
            }
        }

        public virtual void ImplementInterface(Type interfaceType, bool shareBackingFieldsForSameNamesAndTypes = true)
        {
            if (interfaceType == null) throw new ArgumentNullException("interfaceType");
            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException("interfaceType is not an interface", "interfaceType");
            }
            if (!AddInterface(interfaceType))
            {
                // already added
                return;
            }
            foreach (var prop in new[] {interfaceType}.Union(interfaceType.GetInterfaces()).SelectMany(t => t.GetProperties()))
            {
                ImplementInterfaceProperty(prop, shareBackingFieldsForSameNamesAndTypes);
            }
        }

        public virtual PropertyBuilder ImplementProperty(string name, Type type, MethodAttributes methodAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, PropertyAttributes propertyAttributes = PropertyAttributes.HasDefault, FieldBuilder field = null)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException("name");
            if (type == null) throw new ArgumentNullException("type");
            field = field ?? GetOrAddField(name, type);
            var property = TypeBuilder.DefineProperty(name, propertyAttributes, type, null);
            var setter = TypeBuilder.DefineMethod("set_" + name, methodAttributes, null, new[] { type });
            var getter = TypeBuilder.DefineMethod("get_" + name, methodAttributes, type, Type.EmptyTypes);

            // emit setter
            var ilgen = setter.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Stfld, field);
            ilgen.Emit(OpCodes.Ret);

            // emit getter
            ilgen = getter.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldfld, field);
            ilgen.Emit(OpCodes.Ret);

            property.SetSetMethod(setter);
            property.SetGetMethod(getter);

            return property;
        }

        public virtual Type CreateType()
        {
           // define default ctor
           var defaultCtor = _typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

           // default ctor overload to set all members in one call
           var ctor = _typeBuilder.DefineConstructor(
              MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName, CallingConventions.Standard, _fields.Values.Select(f => f.FieldType).ToArray());
           var il = ctor.GetILGenerator();
           il.Emit(OpCodes.Ldarg_0);
           il.Emit(OpCodes.Call, defaultCtor);
           var i = 1;
           foreach (var f in _fields.Values)
           {
              ctor.DefineParameter(i, ParameterAttributes.None, f.Name.Substring(1));
              il.Emit(OpCodes.Ldarg_0);
              il.Emit(OpCodes.Ldarg, i++);
              il.Emit(OpCodes.Stfld,f);
           }
           il.Emit(OpCodes.Ret);

           // override ToString method
           var m = _typeBuilder.DefineMethod("ToString",
              MethodAttributes.Public
              | MethodAttributes.HideBySig
              | MethodAttributes.NewSlot
              | MethodAttributes.Virtual
              | MethodAttributes.Final,
              CallingConventions.HasThis,
              typeof (string),
              Type.EmptyTypes);
           il = m.GetILGenerator();
           il.Emit(OpCodes.Ldarg_0);
           il.Emit(OpCodes.Call, typeof(DtoTypeBuilder).GetMethod("BuildToString",BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));
           il.Emit(OpCodes.Ret);
           _typeBuilder.DefineMethodOverride(m, typeof(object).GetMethod("ToString"));

            return TypeBuilder.CreateType();
        }

       public static string BuildToString(object entity)
       {
          var sb = new StringBuilder();
          foreach (var p in entity.GetType().GetProperties())
          {
             var v = p.GetValue(entity);
             sb.AppendFormat(v == null ? "{0}=NULL, " : "{0}='{1}', ", p.Name, v);
          }
          if (sb.Length > 1) sb.Length -= 2;
          return sb.Append('}').ToString();
       }

#if DEBUG
        public virtual void Save()
        {
            _assemblyBuilder.Save(_assemblyName.Name + ".dll");
        }
#endif
    }
}