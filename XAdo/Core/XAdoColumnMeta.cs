using System;

namespace XAdo.Core
{
   public class XAdoColumnMeta
   {
      public string ColumnName { get; internal set; }
      public Type DataType { get; internal set; }
      public bool PKey { get; internal set; }
      public bool AutoIncrement { get; internal set; }
      public bool AllowDBNull { get; internal set; }
      public bool Unique { get; internal set; }
      public object DefaultValue { get; internal set; }
      public int MaxLength { get; internal set; }
      public bool ReadOnly { get; internal set; }
   }
}