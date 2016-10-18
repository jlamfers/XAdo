using System;
using System.Collections.Generic;
using System.Data;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   public class AdoParameterFactoryImpl : IAdoParameterFactory
   {

      private readonly IAdoClassBinder _classBinder;
      private Dictionary<Type, DbType> _customDefaultTypeMappings;

      public AdoParameterFactoryImpl(IAdoClassBinder classBinder)
      {
         if (classBinder == null) throw new ArgumentNullException("classBinder");
         _classBinder = classBinder;
      }

      public virtual IAdoParameter Create(object value = null, DbType? dbType = null, ParameterDirection? direction = null, byte? precision = null, byte? scale = null, int? size = null)
      {
         var p = _classBinder.Get<IAdoParameter>();
         if (dbType == null && _customDefaultTypeMappings != null && value != null)
         {
            dbType = _customDefaultTypeMappings[value.GetType()];
         }
         p.Value = value;
         p.DbType = dbType;
         p.Direction = direction;
         p.Precision = precision;
         p.Scale = scale;
         p.Size = size;
         OnParameterCreated(p);
         return p;
      }

      public virtual IAdoParameterFactory SetCustomDefaultTypeMapping(Type parameterType, DbType dbType)
      {
         _customDefaultTypeMappings = _customDefaultTypeMappings ?? new Dictionary<Type, DbType>();
         _customDefaultTypeMappings[parameterType] = dbType;
         return this;
      }

      public DbType? GetCustomDefaultTypeMapping(Type parameterType)
      {
         DbType dbType;
         return _customDefaultTypeMappings != null && _customDefaultTypeMappings.TryGetValue(parameterType, out dbType)
            ? (DbType?)dbType
            : null;
      }

      protected virtual void OnParameterCreated(IAdoParameter parameter)
      {
      }

   }
}
