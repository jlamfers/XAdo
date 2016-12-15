using System;

namespace XAdo.SqlObjects.DbSchema.Attributes
{
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
   public class DbAutoIncrementAttribute : Attribute { }
}