using System.Data;

namespace XAdo.Core.Interface
{
   public interface IAdoParamHelper
   {
      IAdoParameter In(object value, DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null);
      IAdoParameter Out(DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null);
      IAdoParameter InOut(object value, DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null);
      IAdoParameter Return(DbType? dbType = null, byte? precision = null, byte? scale = null, int? size = null);
   }
}