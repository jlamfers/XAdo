using System;

namespace XAdo.Quobs.DbSchema.Attributes
{
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
   public class DbUniqueAttribute : Attribute { }
}