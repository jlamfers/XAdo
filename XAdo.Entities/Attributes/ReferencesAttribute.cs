using System;

namespace XAdo.Quobs.Attributes
{
   public class ReferencesAttribute : Attribute
   {
      public Type Type { get; set; }
      public string MemberName { get; set; }
      public string ColumnName { get; set; }
      public string FKeyName { get; set; }
   }
}