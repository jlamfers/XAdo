using System;

namespace XAdo.Quobs.Core.DbSchema.Attributes
{
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
   public class DbUniqueAttribute : Attribute { }
}