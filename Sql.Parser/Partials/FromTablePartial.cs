﻿using System.Collections.Generic;
using System.IO;

namespace Sql.Parser.Partials
{
   public class FromTablePartial : TablePartial
   {
      public FromTablePartial(IList<string> parts, string alias) : base(parts, alias)
      {
      }

      public FromTablePartial(MultiPartAliasedPartial other) : base(other)
      {
      }

      public override void Write(TextWriter w, object args)
      {
         w.Write("FROM ");
         base.Write(w, args);
      }

   }
}