using System.Collections.Generic;
using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials2
{
   public abstract class MultiPartAliasedPartial : SqlPartial
   {
      
      public IList<string> RawParts { get; protected set; }
      public string RawAlias { get; protected set; }

      public IList<string> Parts { get; protected set; }
      public string Alias { get; protected set; }

      public virtual void WriteAliased(TextWriter w, object args)
      {
         w.Write(Expression);
         if (RawAlias != null)
         {
            w.Write(" AS ");
            w.Write(RawAlias);
         }
      }
      public virtual void WriteNonAliased(TextWriter w, object args)
      {
         w.Write(Expression);
      }

      public override void Write(TextWriter w, object args)
      {
         WriteAliased(w, args);
      }

   }
}
