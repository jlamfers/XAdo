﻿using System;
using XAdo.Quobs.Attributes.Core;

namespace XAdo.Quobs.Attributes
{
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
   public class QuobsColumnAttribute : QuobsAttribute
   {
      private string _columnName;

      public QuobsColumnAttribute() : base(null)
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

   }
}