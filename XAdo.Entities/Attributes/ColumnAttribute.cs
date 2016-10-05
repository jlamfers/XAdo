using System;

namespace XAdo.Quobs.Attributes
{
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
   public class ColumnAttribute : QuobsAttribute
   {
      private string _columnName;

      public ColumnAttribute() : base(null)
      {
         
      }

      public string ColumnName
      {
         get
         {
            return _columnName;
         }
         set
         {
            if (value != null)
            {
               ColumnNameParts = NameParser.FindParts(value).ToArray();
            }
            _columnName = value;
            SqlExpression = value;
         }
      }

      public string[] ColumnNameParts { get; private set; }

      public bool IsPKey { get; set; }
      public bool IsOutputOnInsert { get; set; }
      public bool IsOutputOnUpdate { get; set; }
   }
}