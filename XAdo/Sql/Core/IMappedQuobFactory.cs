using System.Linq.Expressions;
using Sql.Parser;
using XAdo.Core;
using XAdo.Sql.Core;

namespace XAdo.Sql
{
   internal interface IMappedQuobFactory
   {
      IMappedQuobFactory SetRequestor(IQuob quob);
      IQuob CreateMappedQuob(Expression mappedBinder, SqlSelectInfo mappedSelectInfo);
   }
}