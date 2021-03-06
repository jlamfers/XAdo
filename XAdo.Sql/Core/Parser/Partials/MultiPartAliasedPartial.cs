﻿using System.Collections.Generic;
using System.IO;

namespace XAdo.Quobs.Core.Parser.Partials
{
   public abstract class MultiPartAliasedPartial : SqlPartial
   {
      protected MultiPartAliasedPartial() { }
      protected MultiPartAliasedPartial(string expression)
         : base(expression)
      {
         
      }

      public IList<string> RawParts { get; protected set; }
      public string RawAlias { get; protected set; }

      public IList<string> Parts { get; protected set; }
      public string Alias { get; protected set; }

      public virtual void WriteAliased(TextWriter w)
      {
         w.Write(Expression);
         if (RawAlias != null)
         {
            w.Write(" AS ");
            w.Write(RawAlias);
         }
      }
      public virtual void WriteNonAliased(TextWriter w)
      {
         w.Write(Expression);
      }

      public override void Write(TextWriter w)
      {
         WriteAliased(w);
      }

   }
}
