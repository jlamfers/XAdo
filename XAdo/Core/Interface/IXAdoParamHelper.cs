using System.Data;

namespace XAdo.Core.Interface
{
   public interface IXAdoParamHelper
   {
      IXAdoParameter In(object value, DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null);
      IXAdoParameter Out(DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null);
      IXAdoParameter InOut(object value, DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null);
      IXAdoParameter Return(DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null);
   }
}