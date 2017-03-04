using System;
using System.Data;
using XAdo.Core.Interface;

namespace XAdo.Core.Impl
{
   /// <summary>
   /// Helper class for creating ADO parameters with non default settings
   /// </summary>
   /// <example>
   /// session.Query("select * from table where name = @name",new{ id = context.AdoParamHelper.In(10)})
   /// </example>
   public class XAdoParamHelperImpl : IXAdoParamHelper
   {
      private readonly IXAdoParameterFactory _parameterFactory;

      public XAdoParamHelperImpl(IXAdoParameterFactory parameterFactory)
      {
         if (parameterFactory == null) throw new ArgumentNullException("parameterFactory");
         _parameterFactory = parameterFactory;
      }

      public virtual IXAdoParameter In(object value, DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null)
      {
         return _parameterFactory.Create(value, dbType, ParameterDirection.Input, precision, scale, size);
      }
      public virtual IXAdoParameter Out(DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null)
      {
         return _parameterFactory.Create(null, dbType, ParameterDirection.Output, precision, scale, size);
      }
      public virtual IXAdoParameter InOut(object value, DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null)
      {
         return _parameterFactory.Create(value, dbType, ParameterDirection.InputOutput, precision, scale, size);
      }
      public virtual IXAdoParameter Return(DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null)
      {
         return _parameterFactory.Create(null, dbType, ParameterDirection.ReturnValue, precision, scale, size);
      }
   }
}