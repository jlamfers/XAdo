using System;

namespace XAdo.Quobs.Attributes
{
   [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
   public class DbUniqueAttribute : Attribute { }
}