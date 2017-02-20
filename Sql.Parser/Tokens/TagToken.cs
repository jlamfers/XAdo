﻿using System;
using System.IO;
using System.Linq;

namespace Sql.Parser.Tokens
{
   public class TagToken : SqlToken
   {
      public TagToken(string expression)
         : base(expression)
      {
         Tag = expression.Split(new []{"//"}, StringSplitOptions.None).First().Trim();
      }

      public string Tag { get;private set; }

      public override void Write(TextWriter w, object args)
      {
         w.Write("-- >");
         base.Write(w, args);
      }

   }
}