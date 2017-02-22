using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace Sql.Parser.Partials
{
   public class GroupByPartial : SqlPartial
   {

      public GroupByPartial(IList<ColumnPartial> columns)
         : base(string.Join(", ", columns.Select(c => c.ToString())))
      {
         Columns = columns.ToList().AsReadOnly();
      }

      public ReadOnlyCollection<ColumnPartial> Columns { get; private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("GROUP BY ");
         base.Write(w, args);
      }

   }
}