using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Sql.Parser.Tokens
{
   public class GroupByToken : SqlToken
   {

      public GroupByToken(IList<ColumnToken> columns)
         : base(string.Join(", ", columns.Select(c => c.ToString())))
      {
         Columns = columns.ToList().AsReadOnly();
      }

      public ReadOnlyCollection<ColumnToken> Columns { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("GROUP BY ");
         base.Write(w, args);
      }

   }
}