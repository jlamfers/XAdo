using System;
using XAdo.Quobs.Attributes.Core;

namespace XAdo.Quobs.Attributes
{
   [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
   public class QuobsTableAttribute : QuobsAttribute
   {
      private string _tableName;

      public QuobsTableAttribute() : base(null)
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