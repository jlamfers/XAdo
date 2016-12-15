using System;
using System.Collections.Generic;
using System.Linq;

namespace XAdo.Quobs.DbSchema.Attributes
{
   public class ReferencedByAttribute : Attribute
   {
      public IList<Type> Types { get; private set; }

      public ReferencedByAttribute(Type[] types)
      {
         Types = types.ToList().AsReadOnly();
      }
   }
}