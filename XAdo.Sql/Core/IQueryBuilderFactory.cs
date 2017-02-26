namespace XAdo.Sql.Core
{
   public interface IQueryBuilderFactory
   {
      QueryBuilder Parse(string sql);
      QueryBuilder<T> Parse<T>(string sql);
   }
}