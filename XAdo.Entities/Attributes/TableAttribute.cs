using System;

namespace XAdo.Quobs.Attributes
{
   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
   public class TableAttribute : QuobsAttribute
   {
      private string _tableName;

      public TableAttribute() : base(null)
      {
         
      }
      public string TableName
      {
         get
         {
            return _tableName;
         }
         set
         {
            if (value != null)
            {
               TableNameParts = NameParser.FindParts(value).ToArray();
            }
            _tableName = value;
            SqlExpression = value;
         }
      }

      public string[] TableNameParts { get; private set; }
   }
}